using System.Text.RegularExpressions;
using Moongate.Plugins.Data;

namespace Moongate.Plugins.Internal;

internal static partial class PluginDependencySorter
{
    public static IReadOnlyList<LoadedPlugin> ValidateAndSort(IReadOnlyList<LoadedPlugin> plugins)
    {
        ArgumentNullException.ThrowIfNull(plugins);

        var byId = new Dictionary<string, LoadedPlugin>(StringComparer.OrdinalIgnoreCase);

        foreach (var plugin in plugins)
        {
            ValidateMetadata(plugin);

            if (!byId.TryAdd(plugin.Metadata.Id, plugin))
            {
                throw new InvalidOperationException($"Duplicate plugin id '{plugin.Metadata.Id}'.");
            }
        }

        foreach (var plugin in plugins)
        {
            foreach (var dependencyId in plugin.Metadata.Dependencies)
            {
                if (string.IsNullOrWhiteSpace(dependencyId) || !PluginIdRegex().IsMatch(dependencyId))
                {
                    throw new InvalidOperationException(
                        $"Plugin '{plugin.Metadata.Id}' declares invalid dependency id '{dependencyId}'."
                    );
                }

                if (!byId.ContainsKey(dependencyId))
                {
                    throw new InvalidOperationException(
                        $"Plugin '{plugin.Metadata.Id}' has missing dependency '{dependencyId}'."
                    );
                }
            }
        }

        var ordered = new List<LoadedPlugin>(plugins.Count);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visiting = new Stack<string>();

        foreach (var plugin in plugins)
        {
            Visit(plugin.Metadata.Id, byId, ordered, visited, visiting);
        }

        return ordered;
    }

    [GeneratedRegex("^[a-z0-9]+(\\.[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex PluginIdRegex();

    private static void ValidateMetadata(LoadedPlugin plugin)
    {
        var metadata = plugin.Metadata;

        if (string.IsNullOrWhiteSpace(metadata.Id) || !PluginIdRegex().IsMatch(metadata.Id))
        {
            throw new InvalidOperationException(
                $"Plugin at '{plugin.PluginDirectory}' has invalid plugin id '{metadata.Id}'."
            );
        }

        if (string.IsNullOrWhiteSpace(metadata.Name))
        {
            throw new InvalidOperationException($"Plugin '{metadata.Id}' has missing name.");
        }

        if (metadata.Version is null)
        {
            throw new InvalidOperationException($"Plugin '{metadata.Id}' has missing version.");
        }

        if (string.IsNullOrWhiteSpace(metadata.Author))
        {
            throw new InvalidOperationException($"Plugin '{metadata.Id}' has missing author.");
        }
    }

    private static void Visit(
        string id,
        IReadOnlyDictionary<string, LoadedPlugin> byId,
        List<LoadedPlugin> ordered,
        HashSet<string> visited,
        Stack<string> visiting
    )
    {
        if (visited.Contains(id))
        {
            return;
        }

        if (visiting.Contains(id, StringComparer.OrdinalIgnoreCase))
        {
            var path = string.Join(" -> ", visiting.Reverse().Append(id));

            throw new InvalidOperationException($"Plugin dependency cycle detected: {path}.");
        }

        visiting.Push(id);
        var plugin = byId[id];

        foreach (var dependencyId in plugin.Metadata.Dependencies)
        {
            Visit(dependencyId, byId, ordered, visited, visiting);
        }

        visiting.Pop();
        visited.Add(id);
        ordered.Add(plugin);
    }
}

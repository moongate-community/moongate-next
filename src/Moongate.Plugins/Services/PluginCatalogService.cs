using System.Globalization;
using Moongate.Plugins.Data;
using Moongate.Plugins.Interfaces.Plugins;
using YamlDotNet.RepresentationModel;

namespace Moongate.Plugins.Services;

/// <summary>Default loaded-plugin catalog with conservative config redaction.</summary>
public sealed class PluginCatalogService : IPluginCatalogService
{
    private const string RedactedValue = "***REDACTED***";

    private static readonly string[] SensitiveKeyTokens =
    [
        "password",
        "passwd",
        "pwd",
        "secret",
        "token",
        "key",
        "credential",
        "credentials",
        "connection_string"
    ];

    private readonly IReadOnlyList<LoadedPlugin> _plugins;

    public PluginCatalogService(IEnumerable<LoadedPlugin> plugins)
    {
        ArgumentNullException.ThrowIfNull(plugins);

        _plugins = plugins.ToArray();
    }

    public ValueTask<PluginConfigView?> GetConfigAsync(
        string pluginId,
        CancellationToken cancellationToken = default
    )
    {
        var plugin = FindPlugin(pluginId);

        if (plugin is null)
        {
            return ValueTask.FromResult<PluginConfigView?>(null);
        }

        return GetConfigCoreAsync(plugin, cancellationToken);
    }

    public async ValueTask<PluginConfigForm?> GetConfigFormAsync(
        string pluginId,
        CancellationToken cancellationToken = default
    )
    {
        var plugin = FindPlugin(pluginId);

        if (plugin?.Instance is not IConfigurablePlugin configurable)
        {
            return null;
        }

        return await configurable.GetConfigFormAsync(cancellationToken);
    }

    public IReadOnlyList<PluginCatalogEntry> GetLoadedPlugins()
        => _plugins.Select(ToEntry).ToArray();

    public async ValueTask<PluginConfigSaveResult?> SaveConfigAsync(
        string pluginId,
        PluginConfigSaveRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        var plugin = FindPlugin(pluginId);

        if (plugin?.Instance is not IConfigurablePlugin configurable)
        {
            return null;
        }

        var result = await configurable.SaveConfigAsync(request, cancellationToken);
        var config = result.Config ?? await GetConfigCoreAsync(plugin, cancellationToken);

        return result with { Config = config };
    }

    public async ValueTask<PluginTestResult?> TestAsync(
        string pluginId,
        CancellationToken cancellationToken = default
    )
    {
        var plugin = FindPlugin(pluginId);

        if (plugin?.Instance is not ITestablePlugin testable)
        {
            return null;
        }

        return await testable.TestAsync(cancellationToken);
    }

    internal static string SanitizeYaml(string yaml, out IReadOnlyList<string> redactedKeys)
    {
        if (string.IsNullOrWhiteSpace(yaml))
        {
            redactedKeys = [];

            return "";
        }

        var stream = new YamlStream();

        using (var reader = new StringReader(yaml))
        {
            stream.Load(reader);
        }

        var redacted = new List<string>();

        foreach (var document in stream.Documents)
        {
            SanitizeNode(document.RootNode, "", redacted);
        }

        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        stream.Save(writer, false);
        redactedKeys = redacted;

        return writer.ToString();
    }

    private static string ConfigDisplayPath(LoadedPlugin plugin)
        => Path.Combine(Path.GetFileName(plugin.PluginDirectory), PluginContext.PluginConfigFileName);

    private LoadedPlugin? FindPlugin(string pluginId)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
        {
            return null;
        }

        return _plugins.FirstOrDefault(
            candidate => string.Equals(candidate.Metadata.Id, pluginId.Trim(), StringComparison.OrdinalIgnoreCase)
        );
    }

    private static async ValueTask<PluginConfigView?> GetConfigCoreAsync(
        LoadedPlugin plugin,
        CancellationToken cancellationToken
    )
    {
        var configPath = Path.Combine(plugin.PluginDirectory, PluginContext.PluginConfigFileName);
        var displayPath = ConfigDisplayPath(plugin);

        if (!File.Exists(configPath))
        {
            return new(plugin.Metadata.Id, false, displayPath, "", []);
        }

        var yaml = await File.ReadAllTextAsync(configPath, cancellationToken);
        var sanitizedYaml = SanitizeYaml(yaml, out var redactedKeys);

        return new(plugin.Metadata.Id, true, displayPath, sanitizedYaml, redactedKeys);
    }

    private static bool IsSecretReferenceKey(string key)
    {
        var normalized = key.Trim().ToLowerInvariant();

        return normalized.EndsWith("_secret", StringComparison.Ordinal) ||
               normalized.EndsWith("_secret_id", StringComparison.Ordinal) ||
               normalized.EndsWith("_secret_name", StringComparison.Ordinal);
    }

    private static bool IsSensitiveKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || IsSecretReferenceKey(key))
        {
            return false;
        }

        var normalized = key.Trim().ToLowerInvariant();

        return SensitiveKeyTokens.Any(token => normalized.Contains(token, StringComparison.Ordinal));
    }

    private static void SanitizeMapping(YamlMappingNode mapping, string path, List<string> redactedKeys)
    {
        foreach (var (keyNode, valueNode) in mapping.Children.ToArray())
        {
            var key = keyNode is YamlScalarNode scalar ? scalar.Value ?? "" : keyNode.ToString() ?? "";
            var keyPath = string.IsNullOrEmpty(path) ? key : $"{path}.{key}";

            if (IsSensitiveKey(key))
            {
                mapping.Children[keyNode] = new YamlScalarNode(RedactedValue);
                redactedKeys.Add(keyPath);

                continue;
            }

            SanitizeNode(valueNode, keyPath, redactedKeys);
        }
    }

    private static void SanitizeNode(YamlNode node, string path, List<string> redactedKeys)
    {
        switch (node)
        {
            case YamlMappingNode mapping:
                SanitizeMapping(mapping, path, redactedKeys);

                break;
            case YamlSequenceNode sequence:
                for (var i = 0; i < sequence.Children.Count; i++)
                {
                    SanitizeNode(sequence.Children[i], $"{path}[{i}]", redactedKeys);
                }

                break;
        }
    }

    private static PluginCatalogEntry ToEntry(LoadedPlugin plugin)
        => new(
            plugin.Metadata.Id,
            plugin.Metadata.Name,
            plugin.Metadata.Version.ToString(),
            plugin.Metadata.Author,
            plugin.Metadata.Description,
            plugin.Metadata.Dependencies.ToArray(),
            plugin.Assembly.GetName().Name ?? "",
            Path.GetFileName(plugin.PluginDirectory),
            File.Exists(Path.Combine(plugin.PluginDirectory, PluginContext.PluginConfigFileName)),
            plugin.Instance is IConfigurablePlugin,
            plugin.Instance is ITestablePlugin
        );
}

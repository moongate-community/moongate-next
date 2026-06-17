using System.Collections;
using System.Text.Json;
using Moongate.Abstractions.Configuration;

namespace Moongate.Plugins.Configuration;

/// <summary>
///     Overlays a flat set of dotted-path config values onto an existing typed config object and
///     returns the merged typed object. Values absent from <c>flatValues</c> are preserved from
///     <c>existing</c>. Binding goes through <see cref="ConfigYamlOptions" /> so it uses the same
///     naming convention and converters the config uses on disk.
/// </summary>
public static class PluginConfigBinder
{
    /// <summary>
    ///     Returns a copy of <paramref name="existing" /> with the leaves named by the dotted keys of
    ///     <paramref name="flatValues" /> overwritten. Throws if a value cannot be bound to the target type.
    /// </summary>
    public static T Apply<T>(T existing, IReadOnlyDictionary<string, object?> flatValues)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(flatValues);

        var map = ToNestedMap(existing);

        foreach (var (path, value) in flatValues)
        {
            SetLeaf(map, path.Split('.'), Unwrap(value));
        }

        return FromNestedMap<T>(map);
    }

    private static T FromNestedMap<T>(IDictionary map)
    {
        var yaml = ConfigYamlOptions.Serializer.Serialize(map);

        return ConfigYamlOptions.Deserializer.Deserialize<T>(yaml);
    }

    private static void SetLeaf(IDictionary root, string[] parts, object? leaf)
    {
        var node = root;

        for (var i = 0; i < parts.Length - 1; i++)
        {
            if (node[parts[i]] is IDictionary childMap)
            {
                node = childMap;
            }
            else
            {
                childMap = new Dictionary<object, object?>();
                node[parts[i]] = childMap;
                node = childMap;
            }
        }

        node[parts[^1]] = leaf;
    }

    private static IDictionary ToNestedMap<T>(T existing)
    {
        var yaml = ConfigYamlOptions.Serializer.Serialize(existing);

        return ConfigYamlOptions.Deserializer.Deserialize<Dictionary<object, object?>>(yaml) ??
               new Dictionary<object, object?>();
    }

    private static object? Unwrap(object? value)
    {
        if (value is not JsonElement element)
        {
            return value;
        }

        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number => element.TryGetInt64(out var number) ? number : element.GetDouble(),
            JsonValueKind.Null => null,
            JsonValueKind.Undefined => null,
            _ => element.GetRawText()
        };
    }
}

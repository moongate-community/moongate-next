using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Moongate.AssetDataConverter;

internal static class Program
{
    private static readonly ISerializer _serializer = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
        .Build();

    public static int Main(string[] args)
    {
        try
        {
            var root = GetRequiredOption(args, "--root");
            var mode = GetRequiredOption(args, "--mode");

            if (string.Equals(mode, "json", StringComparison.OrdinalIgnoreCase))
            {
                ConvertJsonFiles(root);

                return 0;
            }

            if (string.Equals(mode, "cfg", StringComparison.OrdinalIgnoreCase))
            {
                ConvertCfgFiles(root);

                return 0;
            }

            throw new ArgumentException($"Unsupported mode '{mode}'. Use 'json' or 'cfg'.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);

            return 1;
        }
    }

    private static Dictionary<string, object?> ConvertBodyTable(string path)
    {
        var body = new List<Dictionary<string, object?>>();

        foreach (var line in ReadDataLines(path))
        {
            var parts = SplitWhitespace(line);

            if (parts.Length < 2 || !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
            {
                continue;
            }

            body.Add(
                new()
                {
                    ["id"] = id,
                    ["type"] = parts[1]
                }
            );
        }

        return new() { ["body"] = body };
    }

    private static Dictionary<string, object?> ConvertContainerLayouts(string path)
    {
        var layouts = new List<Dictionary<string, object?>>();

        foreach (var line in ReadDataLines(path))
        {
            var fields = line.Split('\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (fields.Length < 3)
            {
                continue;
            }

            var itemIds = fields.Length > 3
                ? fields[3].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                           .Select(ParseInt)
                           .ToList()
                : [];

            layouts.Add(
                new()
                {
                    ["gump_id"] = ParseInt(fields[0]),
                    ["bounds"] = SplitWhitespace(fields[1]).Select(ParseInt).ToList(),
                    ["drop_sound"] = ParseInt(fields[2]),
                    ["item_ids"] = itemIds
                }
            );
        }

        return new() { ["container_layout"] = layouts };
    }

    private static Dictionary<string, object?> ConvertConversionSections(string path)
    {
        var sections = new List<Dictionary<string, object?>>();
        Dictionary<string, object?>? current = null;
        List<Dictionary<string, object?>>? entries = null;
        string? pendingSection = null;

        foreach (var rawLine in File.ReadLines(path))
        {
            var line = rawLine.Trim();

            if (line.Length == 0 || line.StartsWith('#') || line.StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            if (line == "{")
            {
                if (string.IsNullOrWhiteSpace(pendingSection))
                {
                    throw new InvalidOperationException($"Malformed conversion section in {path}.");
                }

                entries = [];
                current = new()
                {
                    ["name"] = pendingSection,
                    ["entries"] = entries
                };
                sections.Add(current);
                pendingSection = null;

                continue;
            }

            if (line == "}")
            {
                current = null;
                entries = null;

                continue;
            }

            if (current is null)
            {
                pendingSection = line;

                continue;
            }

            var parts = SplitWhitespace(line);

            if (parts.Length == 0)
            {
                continue;
            }

            entries!.Add(
                new()
                {
                    ["name"] = parts[0],
                    ["values"] = parts.Skip(1).ToList()
                }
            );
        }

        return new() { ["conversion_section"] = sections };
    }

    private static Dictionary<string, object?> ConvertDecoration(string path)
    {
        var decorations = new List<Dictionary<string, object?>>();
        var pendingDescriptions = new List<string>();
        Dictionary<string, object?>? current = null;
        List<Dictionary<string, object?>>? placements = null;

        foreach (var rawLine in File.ReadLines(path))
        {
            var line = rawLine.Trim();

            if (line.Length == 0)
            {
                continue;
            }

            if (line.StartsWith('#'))
            {
                var description = line.TrimStart('#').Trim();

                if (description.Length > 0)
                {
                    pendingDescriptions.Add(description);
                }

                continue;
            }

            if (TryParsePlacement(line, out var placement))
            {
                if (placements is null)
                {
                    throw new InvalidOperationException($"Placement without decoration header in {path}: {line}");
                }

                placements.Add(placement);

                continue;
            }

            placements = [];
            current = ParseDecorationHeader(line, pendingDescriptions);
            current["placements"] = placements;
            decorations.Add(current);
            pendingDescriptions.Clear();
        }

        return new() { ["decoration"] = decorations };
    }

    private static Dictionary<string, object?> ConvertDoors(string path)
    {
        var doors = new List<Dictionary<string, object?>>();

        foreach (var line in File.ReadLines(path))
        {
            var parts = line.Split('\t', StringSplitOptions.TrimEntries);

            if (parts.Length < 10 || !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var category))
            {
                continue;
            }

            doors.Add(
                new()
                {
                    ["category"] = category,
                    ["pieces"] = parts.Skip(1).Take(8).Select(ParseInt).ToList(),
                    ["feature_mask"] = ParseInt(parts[9]),
                    ["comment"] = parts.Length > 10 ? string.Join('\t', parts.Skip(10)).Trim() : ""
                }
            );
        }

        return new() { ["door"] = doors };
    }

    private static void ConvertCfgFiles(string root)
    {
        foreach (var path in Directory.EnumerateFiles(root, "*.cfg", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
        {
            var relativePath = NormalizeRelativePath(root, path);
            var converted = relativePath switch
            {
                "bodyTable.cfg" => ConvertBodyTable(path),
                "containers/containers.cfg" => ConvertContainerLayouts(path),
                "signs/signs.cfg" => ConvertSigns(path),
                "support/uoconvert.cfg" => ConvertConversionSections(path),
                _ when relativePath.StartsWith("decoration/", StringComparison.Ordinal) => ConvertDecoration(path),
                _ => throw new InvalidOperationException($"Unsupported CFG asset: {relativePath}")
            };

            WriteYaml(Path.ChangeExtension(path, ".yaml"), converted);
        }

        var doorsPath = Path.Combine(root, "components", "doors.txt");

        if (File.Exists(doorsPath))
        {
            WriteYaml(Path.ChangeExtension(doorsPath, ".yaml"), ConvertDoors(doorsPath));
        }
    }

    private static void ConvertJsonFiles(string root)
    {
        foreach (var path in Directory.EnumerateFiles(root, "*.json", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
        {
            var relativePath = NormalizeRelativePath(root, path);
            var node = JsonNode.Parse(File.ReadAllText(path)) ??
                       throw new InvalidOperationException($"Could not parse JSON asset {path}.");
            var converted = ConvertJsonRoot(relativePath, node);

            WriteYaml(Path.ChangeExtension(path, ".yaml"), converted);
        }
    }

    private static object? ConvertJsonRoot(string relativePath, JsonNode node)
    {
        if (string.Equals(relativePath, "Professions/professions.json", StringComparison.Ordinal))
        {
            return new Dictionary<string, object?> { ["profession"] = TransformJson(node["professions"]) };
        }

        if (string.Equals(relativePath, "containers/default_containers.json", StringComparison.Ordinal))
        {
            return new Dictionary<string, object?> { ["container"] = TransformJson(node) };
        }

        if (string.Equals(relativePath, "expansions.json", StringComparison.Ordinal))
        {
            return new Dictionary<string, object?> { ["expansion"] = TransformJson(node) };
        }

        if (string.Equals(relativePath, "names/modernuo_names.json", StringComparison.Ordinal))
        {
            return new Dictionary<string, object?> { ["name_group"] = TransformJson(node) };
        }

        if (string.Equals(relativePath, "regions/regions.json", StringComparison.Ordinal))
        {
            return new Dictionary<string, object?> { ["region"] = TransformJson(node) };
        }

        if (string.Equals(relativePath, "skills.json", StringComparison.Ordinal))
        {
            return new Dictionary<string, object?> { ["skill"] = TransformJson(node) };
        }

        if (string.Equals(relativePath, "teleporters/teleporters.json", StringComparison.Ordinal))
        {
            return new Dictionary<string, object?> { ["teleporter"] = TransformJson(node) };
        }

        if (string.Equals(relativePath, "weather/weather.json", StringComparison.Ordinal))
        {
            return new Dictionary<string, object?>
            {
                ["header"] = TransformJson(node["header"]),
                ["weather_type"] = TransformJson(node["weatherTypes"])
            };
        }

        if (relativePath.StartsWith("locations/", StringComparison.Ordinal))
        {
            return TransformJson(node);
        }

        if (relativePath.StartsWith("spawns/", StringComparison.Ordinal))
        {
            return new Dictionary<string, object?> { ["spawn"] = TransformJson(node) };
        }

        throw new InvalidOperationException($"Unsupported JSON asset: {relativePath}");
    }

    private static Dictionary<string, object?> ConvertSigns(string path)
    {
        var signs = new List<Dictionary<string, object?>>();

        foreach (var line in ReadDataLines(path))
        {
            var parts = line.Split([' ', '\t'], 6, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length < 5)
            {
                continue;
            }

            signs.Add(
                new()
                {
                    ["map"] = ParseInt(parts[0]),
                    ["item_id"] = ParseInt(parts[1]),
                    ["location"] = new List<int> { ParseInt(parts[2]), ParseInt(parts[3]), ParseInt(parts[4]) },
                    ["text"] = parts.Length > 5 ? parts[5] : ""
                }
            );
        }

        return new() { ["sign"] = signs };
    }

    private static string GetRequiredOption(string[] args, string name)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.Ordinal))
            {
                return args[i + 1];
            }
        }

        throw new ArgumentException($"Missing required option {name}.");
    }

    private static string NormalizeKey(string key)
    {
        return key == "$type" ? "type" : ToSnakeCase(key);
    }

    private static string NormalizeRelativePath(string root, string path)
    {
        return Path.GetRelativePath(root, path).Replace('\\', '/');
    }

    private static Dictionary<string, object?> ParseDecorationHeader(
        string line,
        IReadOnlyList<string> descriptions
    )
    {
        var parts = SplitWhitespace(line);

        if (parts.Length == 0)
        {
            throw new InvalidOperationException("Decoration header cannot be empty.");
        }

        var result = new Dictionary<string, object?>
        {
            ["type"] = parts[0],
            ["arguments"] = "",
            ["description"] = string.Join(" ", descriptions)
        };

        var argumentStart = 1;

        if (parts.Length > 1 && IsIntegerToken(parts[1]))
        {
            result["item_id"] = ParseInt(parts[1]);
            argumentStart = 2;
        }

        if (parts.Length > argumentStart)
        {
            result["arguments"] = string.Join(' ', parts.Skip(argumentStart));
        }

        return result;
    }

    private static int ParseInt(string value)
    {
        var normalized = value.Trim();

        if (normalized.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return Convert.ToInt32(normalized[2..], 16);
        }

        return int.Parse(normalized, CultureInfo.InvariantCulture);
    }

    private static IEnumerable<string> ReadDataLines(string path)
    {
        foreach (var rawLine in File.ReadLines(path))
        {
            var line = rawLine.Trim();

            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            yield return line;
        }
    }

    private static string[] SplitWhitespace(string line)
    {
        return Regex.Split(line.Trim(), @"\s+").Where(part => part.Length > 0).ToArray();
    }

    private static string ToSnakeCase(string key)
    {
        var builder = new StringBuilder(key.Length + 8);

        for (var i = 0; i < key.Length; i++)
        {
            var current = key[i];

            if (!char.IsLetterOrDigit(current))
            {
                if (builder.Length > 0 && builder[^1] != '_')
                {
                    builder.Append('_');
                }

                continue;
            }

            if (char.IsUpper(current) && ShouldInsertUnderscore(key, i))
            {
                builder.Append('_');
            }

            builder.Append(char.ToLowerInvariant(current));
        }

        return builder.ToString().Trim('_');
    }

    private static object? TransformJson(JsonNode? node)
    {
        if (node is null)
        {
            return null;
        }

        if (node is JsonObject obj)
        {
            var result = new Dictionary<string, object?>(StringComparer.Ordinal);

            foreach (var property in obj)
            {
                var value = TransformJson(property.Value);

                if (value is not null)
                {
                    result[NormalizeKey(property.Key)] = value;
                }
            }

            return result;
        }

        if (node is JsonArray array)
        {
            return array.Select(TransformJson).ToList();
        }

        var element = node.GetValue<JsonElement>();

        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number when element.TryGetInt32(out var value) => value,
            JsonValueKind.Number when element.TryGetInt64(out var value) => value,
            JsonValueKind.Number => element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.ToString()
        };
    }

    private static bool IsIntegerToken(string value)
    {
        var normalized = value.Trim();

        if (normalized.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return normalized.Length > 2 && normalized[2..].All(Uri.IsHexDigit);
        }

        return int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
    }

    private static bool ShouldInsertUnderscore(string key, int index)
    {
        if (index == 0)
        {
            return false;
        }

        var previous = key[index - 1];

        if (!char.IsLetterOrDigit(previous) || previous == '_')
        {
            return false;
        }

        if (char.IsLower(previous) || char.IsDigit(previous))
        {
            return true;
        }

        return index + 1 < key.Length && char.IsLower(key[index + 1]);
    }

    private static bool TryParsePlacement(string line, out Dictionary<string, object?> placement)
    {
        placement = [];
        var match = Regex.Match(line, @"^\s*(-?\d+)\s+(-?\d+)\s+(-?\d+)(?<rest>.*)$");

        if (!match.Success)
        {
            return false;
        }

        var rest = match.Groups["rest"].Value.Trim();
        placement["location"] = new List<int>
        {
            ParseInt(match.Groups[1].Value),
            ParseInt(match.Groups[2].Value),
            ParseInt(match.Groups[3].Value)
        };

        var targetMatch = Regex.Match(rest, @"\((-?\d+),\s*(-?\d+),\s*(-?\d+)\)");

        if (targetMatch.Success)
        {
            placement["target"] = new List<int>
            {
                ParseInt(targetMatch.Groups[1].Value),
                ParseInt(targetMatch.Groups[2].Value),
                ParseInt(targetMatch.Groups[3].Value)
            };
            rest = rest.Replace(targetMatch.Value, "", StringComparison.Ordinal).Trim();
        }

        var noteIndex = rest.IndexOf("//", StringComparison.Ordinal);

        if (noteIndex >= 0)
        {
            placement["note"] = rest[(noteIndex + 2)..].Trim();
        }
        else if (rest.Length > 0)
        {
            placement["note"] = rest;
        }

        return true;
    }

    private static void WriteYaml(string path, object? content)
    {
        File.WriteAllText(path, _serializer.Serialize(content));
        Console.WriteLine(path);
    }
}

using Moongate.UO.Data.Templates.Loot;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.Loot;

/// <summary>
/// Loads loot table YAML files from a directory, merging every file's
/// <c>loot_tables</c> list. Parsing is strict (fail fast with the file path);
/// a missing or empty directory is a warning and yields no tables. Node
/// collections are normalized because YamlDotNet overwrites pre-initialized
/// collections with null when a key is present but empty.
/// </summary>
public sealed class LootTableYamlLoader
{
    private readonly ILogger _logger = Log.ForContext<LootTableYamlLoader>();
    private readonly string _lootDirectory;

    public LootTableYamlLoader(string lootDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lootDirectory);

        _lootDirectory = Path.GetFullPath(lootDirectory);
    }

    public List<LootTableDefinition> LoadAll()
    {
        if (!Directory.Exists(_lootDirectory))
        {
            _logger.Warning("Loot table directory {Directory} not found; no loot tables loaded", _lootDirectory);

            return [];
        }

        var files = Directory.GetFiles(_lootDirectory, "*.yaml", SearchOption.AllDirectories)
                             .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
                             .ToArray();

        if (files.Length == 0)
        {
            _logger.Warning("No loot table files found in {Directory}", _lootDirectory);

            return [];
        }

        var tables = new List<LootTableDefinition>();

        foreach (var file in files)
        {
            LootTableTable table;

            try
            {
                table = LootYaml.Deserializer.Deserialize<LootTableTable>(File.ReadAllText(file));
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Failed to parse loot table file '{file}'.", exception);
            }

            foreach (var definition in table?.LootTables ?? [])
            {
                definition.Content ??= [];
                NormalizeNodes(definition.Content, $"{definition.Id}/content", file);
                tables.Add(definition);
            }
        }

        _logger.Information("Loaded {TableCount} loot tables from {FileCount} files", tables.Count, files.Length);

        return tables;
    }

    private static void NormalizeNodes(List<LootNode> nodes, string context, string file)
    {
        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];

            if (node is null)
            {
                throw new InvalidOperationException(
                    $"Loot table file '{file}' has an empty node entry in '{context}'."
                );
            }

            if (node.PickOneOf is not null)
            {
                NormalizeNodes(node.PickOneOf, $"{context}[{i}]/pick_one_of", file);
            }

            if (node.Group is not null)
            {
                NormalizeNodes(node.Group, $"{context}[{i}]/group", file);
            }
        }
    }
}

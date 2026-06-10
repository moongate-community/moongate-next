using Moongate.Core.Yaml;
using Moongate.UO.Data.Templates.Loadouts;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.Loadouts;

/// <summary>
/// Loads the starter loadout YAML file. Parsing is strict (fail fast); a missing
/// directory or file is only a warning and yields no loadout. Collections are
/// normalized because YamlDotNet overwrites pre-initialized collections with
/// null when a key is present but empty.
/// </summary>
public sealed class StarterLoadoutYamlLoader
{
    public const string StarterLoadoutFileName = "starter.yaml";

    private readonly ILogger _logger = Log.ForContext<StarterLoadoutYamlLoader>();
    private readonly string _loadoutsDirectory;

    public string StarterLoadoutFilePath => Path.Combine(_loadoutsDirectory, StarterLoadoutFileName);

    public StarterLoadoutYamlLoader(string loadoutsDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loadoutsDirectory);

        _loadoutsDirectory = Path.GetFullPath(loadoutsDirectory);
    }

    public StarterLoadoutDefinition? Load()
    {
        var filePath = StarterLoadoutFilePath;

        if (!File.Exists(filePath))
        {
            _logger.Warning("Starter loadout file {File} not found; no starter loadout configured", filePath);

            return null;
        }

        StarterLoadoutTable table;

        try
        {
            table = YamlUtils.DeserializeFromFile<StarterLoadoutTable>(filePath);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Failed to parse starter loadout file '{filePath}'.", exception);
        }

        if (table?.StarterLoadout is null)
        {
            _logger.Warning("Starter loadout file {File} is empty; no starter loadout configured", filePath);

            return null;
        }

        return Normalize(table.StarterLoadout);
    }

    private static StarterLoadoutDefinition Normalize(StarterLoadoutDefinition definition)
    {
        definition.BackpackTemplate ??= "";
        definition.Base = NormalizeSection(definition.Base);
        definition.Races = NormalizeSections(definition.Races);
        definition.Professions = NormalizeSections(definition.Professions);

        return definition;
    }

    private static LoadoutSection NormalizeSection(LoadoutSection? section)
    {
        section ??= new LoadoutSection();
        section.BackpackItems ??= [];
        section.EquipItems ??= [];

        return section;
    }

    private static Dictionary<string, LoadoutSection> NormalizeSections(Dictionary<string, LoadoutSection>? sections)
    {
        var normalized = new Dictionary<string, LoadoutSection>(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, section) in sections ?? new Dictionary<string, LoadoutSection>())
        {
            normalized[key] = NormalizeSection(section);
        }

        return normalized;
    }
}

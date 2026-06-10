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

        return Normalize(table.StarterLoadout, filePath);
    }

    private static StarterLoadoutDefinition Normalize(StarterLoadoutDefinition definition, string filePath)
    {
        definition.BackpackTemplate ??= "";
        definition.Base = NormalizeSection(definition.Base, "base", filePath);
        definition.Races = NormalizeSections(definition.Races, "races", filePath);
        definition.Professions = NormalizeSections(definition.Professions, "professions", filePath);

        return definition;
    }

    private static LoadoutSection NormalizeSection(LoadoutSection? section, string sectionName, string filePath)
    {
        section ??= new();
        section.BackpackItems ??= [];
        section.EquipItems ??= [];

        // A bare "-" list entry deserializes to a null element; reject it with
        // context instead of letting the validator hit a NullReferenceException.
        if (section.BackpackItems.Any(static entry => entry is null) ||
            section.EquipItems.Any(static entry => entry is null))
        {
            throw new InvalidOperationException(
                $"Starter loadout '{filePath}' section '{sectionName}' contains an empty list entry."
            );
        }

        return section;
    }

    private static Dictionary<string, LoadoutSection> NormalizeSections(
        Dictionary<string, LoadoutSection>? sections,
        string groupName,
        string filePath
    )
    {
        var normalized = new Dictionary<string, LoadoutSection>(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, section) in sections ?? new Dictionary<string, LoadoutSection>())
        {
            // YamlDotNet deserializes into a case-sensitive dictionary, so case
            // duplicates would silently last-write-win here; fail fast instead.
            if (normalized.ContainsKey(key))
            {
                throw new InvalidOperationException(
                    $"Starter loadout '{filePath}' has case-duplicate {groupName} key '{key}'."
                );
            }

            normalized[key] = NormalizeSection(section, $"{groupName}/{key}", filePath);
        }

        return normalized;
    }
}

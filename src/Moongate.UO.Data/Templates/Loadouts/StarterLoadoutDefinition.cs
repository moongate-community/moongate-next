namespace Moongate.UO.Data.Templates.Loadouts;

/// <summary>
/// Declarative starter loadout: a universal base section plus additive per-race
/// and per-profession overlays, all referencing item templates by id.
/// </summary>
public sealed class StarterLoadoutDefinition
{
    public string BackpackTemplate { get; set; } = "";

    public LoadoutSection Base { get; set; } = new();

    public Dictionary<string, LoadoutSection> Races { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, LoadoutSection> Professions { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

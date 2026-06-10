namespace Moongate.UO.Data.Templates.Loadouts;

/// <summary>
/// A composable starter loadout section: items added to the backpack and items equipped.
/// </summary>
public sealed class LoadoutSection
{
    public List<LoadoutItemEntry> BackpackItems { get; set; } = [];

    public List<LoadoutItemEntry> EquipItems { get; set; } = [];
}

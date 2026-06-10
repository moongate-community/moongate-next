namespace Moongate.UO.Data.Data.Loadouts;

/// <summary>
/// The fully composed starter loadout for a new character.
/// </summary>
public sealed class StarterLoadout
{
    /// <summary>The backpack container to create and equip, or null when not configured.</summary>
    public StarterLoadoutItem? Backpack { get; set; }

    /// <summary>Items to equip on the character.</summary>
    public List<StarterLoadoutItem> Equip { get; } = [];

    /// <summary>Items to place inside the backpack.</summary>
    public List<StarterLoadoutItem> BackpackItems { get; } = [];

    /// <summary>True when the loadout grants nothing.</summary>
    public bool IsEmpty => Backpack is null && Equip.Count == 0 && BackpackItems.Count == 0;
}

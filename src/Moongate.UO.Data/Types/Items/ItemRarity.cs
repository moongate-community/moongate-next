namespace Moongate.UO.Data.Types.Items;

/// <summary>
///     Rarity tier of an item, from common loot to unique legendary pieces.
/// </summary>
public enum ItemRarity : byte
{
    /// <summary>No rarity assigned.</summary>
    None = 0,

    /// <summary>Common item.</summary>
    Common = 1,

    /// <summary>Uncommon item.</summary>
    Uncommon = 2,

    /// <summary>Rare item.</summary>
    Rare = 3,

    /// <summary>Epic item.</summary>
    Epic = 4,

    /// <summary>Legendary item.</summary>
    Legendary = 5
}

using Moongate.UO.Data.Data.Tiles;

namespace Moongate.UO.Data.Interfaces.Tiles;

/// <summary>
/// Provides access to the static land and item tile properties parsed from <c>tiledata.mul</c>.
/// </summary>
public interface ITileDataStore
{
    /// <summary>All land tile entries, indexed by land tile id (0..0x3FFF).</summary>
    IReadOnlyList<LandData> LandTable { get; }

    /// <summary>All item tile entries, indexed by item tile id.</summary>
    IReadOnlyList<ItemData> ItemTable { get; }

    /// <summary>Returns the item tile data for <paramref name="id" />.</summary>
    /// <param name="id">Item tile id.</param>
    ItemData GetItem(int id);

    /// <summary>Returns the land tile data for <paramref name="id" />.</summary>
    /// <param name="id">Land tile id.</param>
    LandData GetLand(int id);
}

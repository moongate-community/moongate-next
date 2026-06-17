using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;

namespace Moongate.Server.Interfaces.Services.World;

/// <summary>
/// In-memory spatial index of live in-world entities (mobiles, the player subset, and ground
/// items), bucketed by map sector. Entities are mutated in place; not persisted per change.
/// </summary>
public interface IWorldSpatialIndex
{
    /// <summary>Adds or replaces the live mobile keyed by its serial, bucketing it by sector.</summary>
    void AddMobile(MobileEntity mobile);

    /// <summary>Gets the live mobile by serial, or false when absent.</summary>
    bool TryGet(Serial id, out MobileEntity mobile);

    /// <summary>Removes the live mobile from the index; returns false when absent.</summary>
    bool RemoveMobile(Serial id);

    /// <summary>Snapshot of all live mobiles.</summary>
    IReadOnlyCollection<MobileEntity> All { get; }

    /// <summary>Moves a tracked mobile to a new location, re-bucketing it and setting its Location atomically.</summary>
    void MoveMobile(MobileEntity mobile, Point3D newLocation);

    /// <summary>Adds or updates a ground item's sector bucket.</summary>
    void AddOrUpdateItem(ItemEntity item);

    /// <summary>Removes an item from the index; returns false when absent.</summary>
    bool RemoveItem(Serial id);

    /// <summary>Mobiles within <paramref name="range" /> tiles (2D) of the center on the given map.</summary>
    IReadOnlyList<MobileEntity> GetMobilesInRange(int mapId, Point3D center, int range);

    /// <summary>Player-controlled mobiles within range (2D) of the center on the given map.</summary>
    IReadOnlyList<MobileEntity> GetPlayersInRange(int mapId, Point3D center, int range);

    /// <summary>Ground items within range (2D) of the center on the given map.</summary>
    IReadOnlyList<ItemEntity> GetItemsInRange(int mapId, Point3D center, int range);
}

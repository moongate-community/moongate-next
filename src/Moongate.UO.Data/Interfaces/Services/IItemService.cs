using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.UO.Data.Entities.Items;

namespace Moongate.UO.Data.Interfaces.Services;

/// <summary>
///     Provides item persistence plus tile-data-derived facts (container/door),
///     total-weight aggregation and container placement behavior.
/// </summary>
public interface IItemService
{
    /// <summary>
    ///     Places a child item into a container at the given position and persists both;
    ///     returns false when the target item is not a container.
    /// </summary>
    ValueTask<bool> AddItemAsync(
        ItemEntity container,
        ItemEntity child,
        Point2D position,
        CancellationToken cancellationToken = default
    );

    /// <summary>Returns the current number of persisted items.</summary>
    ValueTask<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists an item, allocating a serial when one is not already set.</summary>
    ValueTask<ItemEntity> CreateAsync(ItemEntity item, CancellationToken cancellationToken = default);

    /// <summary>Permanently removes an item; returns false when absent.</summary>
    ValueTask<bool> DeleteAsync(Serial id, CancellationToken cancellationToken = default);

    /// <summary>Gets an item by serial, or null when absent.</summary>
    ValueTask<ItemEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default);

    /// <summary>True when the item's art is a container (derived from tile data).</summary>
    bool IsContainer(ItemEntity item);

    /// <summary>True when the given art id is a container (derived from tile data).</summary>
    bool IsContainer(int itemId);

    /// <summary>True when the item's art is a door (derived from tile data).</summary>
    bool IsDoor(ItemEntity item);

    /// <summary>True when the given art id is a door (derived from tile data).</summary>
    bool IsDoor(int itemId);

    /// <summary>
    ///     Removes a child item from a container by serial and persists the change;
    ///     returns false when the child was not contained.
    /// </summary>
    ValueTask<bool> RemoveItemAsync(ItemEntity container, Serial itemId, CancellationToken cancellationToken = default);

    /// <summary>Effective weight including stack amount and any contained items.</summary>
    ValueTask<int> TotalWeightAsync(ItemEntity item, CancellationToken cancellationToken = default);
}

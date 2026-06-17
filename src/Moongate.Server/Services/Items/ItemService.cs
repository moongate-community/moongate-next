using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.Server.Interfaces.Services.World;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Interfaces.Tiles;
using Moongate.UO.Data.Types.Tiles;

namespace Moongate.Server.Services.Items;

/// <summary>
/// Default item service backed by auto-increment persistence, deriving
/// container/door facts and weight aggregation from the tile data store.
/// </summary>
public sealed class ItemService : IItemService
{
    private readonly IAutoDataAccess<ItemEntity, Serial> _items;
    private readonly ITileDataStore _tileData;
    private readonly IWorldSpatialIndex _index;

    public ItemService(IAutoDataAccess<ItemEntity, Serial> items, ITileDataStore tileData, IWorldSpatialIndex index)
    {
        ArgumentNullException.ThrowIfNull(index);

        _items = items;
        _tileData = tileData;
        _index = index;
    }

    public async ValueTask<bool> AddItemAsync(
        ItemEntity container,
        ItemEntity child,
        Point2D position,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(child);

        if (!IsContainer(container))
        {
            return false;
        }

        if (!container.ContainedItemIds.Contains(child.Id))
        {
            container.ContainedItemIds.Add(child.Id);
        }

        child.ParentContainerId = container.Id;
        child.ContainerPosition = position;
        child.EquippedMobileId = default;
        child.EquippedLayer = null;

        await _items.UpsertAsync(container, cancellationToken);
        await _items.UpsertAsync(child, cancellationToken);

        _index.RemoveItem(child.Id);

        return true;
    }

    public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
        => _items.CountAsync(cancellationToken);

    public async ValueTask<ItemEntity> CreateAsync(ItemEntity item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!item.Id.IsValid)
        {
            // Items live in the serial range >= ItemOffset (mobiles take the lower range).
            // The shared auto-increment counter starts at 1, so lift the first allocations
            // into the item range; once item serials are persisted the counter self-corrects.
            var allocated = await _items.NextIdAsync(cancellationToken);

            item.Id = allocated.Value < Serial.ItemOffset
                          ? new(allocated.Value + Serial.ItemOffset)
                          : allocated;
        }

        await _items.UpsertAsync(item, cancellationToken);

        if (item.ParentContainerId == Serial.Zero && item.EquippedMobileId == Serial.Zero)
        {
            _index.AddOrUpdateItem(item);
        }

        return item;
    }

    public async ValueTask<bool> DeleteAsync(Serial id, CancellationToken cancellationToken = default)
    {
        var removed = await _items.RemoveAsync(id, cancellationToken);

        if (removed)
        {
            _index.RemoveItem(id);
        }

        return removed;
    }

    public ValueTask<ItemEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
        => _items.GetByIdAsync(id, cancellationToken);

    public bool IsContainer(ItemEntity item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return IsContainer(item.ItemId);
    }

    public bool IsContainer(int itemId)
        => _tileData.GetItem(itemId).Flags.HasFlag(UoTileFlag.Container);

    public bool IsDoor(ItemEntity item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return IsDoor(item.ItemId);
    }

    public bool IsDoor(int itemId)
        => _tileData.GetItem(itemId).Door;

    public async ValueTask<bool> RemoveItemAsync(
        ItemEntity container,
        Serial itemId,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(container);

        if (!container.ContainedItemIds.Remove(itemId))
        {
            return false;
        }

        await _items.UpsertAsync(container, cancellationToken);

        var child = await _items.GetByIdAsync(itemId, cancellationToken);

        if (child is not null)
        {
            child.ParentContainerId = default;
            child.ContainerPosition = default;

            await _items.UpsertAsync(child, cancellationToken);
        }

        return true;
    }

    public async ValueTask<int> TotalWeightAsync(ItemEntity item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);

        var total = item.Weight * Math.Max(1, item.Amount);

        foreach (var childId in item.ContainedItemIds)
        {
            var child = await _items.GetByIdAsync(childId, cancellationToken);

            if (child is not null)
            {
                total += await TotalWeightAsync(child, cancellationToken);
            }
        }

        return total;
    }
}

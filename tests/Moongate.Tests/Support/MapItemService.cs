using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Interfaces.Services;

namespace Moongate.Tests.Support;

/// <summary>
/// IItemService stub that resolves items from a fixed dictionary by id.
/// All other members throw <see cref="NotSupportedException" />.
/// </summary>
public sealed class MapItemService : IItemService
{
    private readonly Dictionary<Serial, ItemEntity> _items;

    public MapItemService(IEnumerable<ItemEntity> items)
    {
        _items = items.ToDictionary(item => item.Id);
    }

    public ValueTask<bool> AddItemAsync(
        ItemEntity container,
        ItemEntity child,
        Point2D position,
        CancellationToken cancellationToken = default
    )
        => throw new NotSupportedException();

    public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public ValueTask<ItemEntity> CreateAsync(ItemEntity item, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public ValueTask<bool> DeleteAsync(Serial id, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public ValueTask<ItemEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
        => new(_items.GetValueOrDefault(id));

    public bool IsContainer(ItemEntity item)
        => throw new NotSupportedException();

    public bool IsContainer(int itemId)
        => throw new NotSupportedException();

    public bool IsDoor(ItemEntity item)
        => throw new NotSupportedException();

    public bool IsDoor(int itemId)
        => throw new NotSupportedException();

    public ValueTask<bool> RemoveItemAsync(
        ItemEntity container,
        Serial itemId,
        CancellationToken cancellationToken = default
    )
        => throw new NotSupportedException();

    public ValueTask<int> TotalWeightAsync(ItemEntity item, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}

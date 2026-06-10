using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Interfaces.Services;

namespace Moongate.Tests.Support;

/// <summary>
/// IItemService stub for tests that must not touch items.
/// </summary>
public sealed class ThrowingItemService : IItemService
{
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
        => throw new NotSupportedException();

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

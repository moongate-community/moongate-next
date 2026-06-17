using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Interfaces.Services;

namespace Moongate.Tests.Support;

/// <summary>
/// IItemService double whose <see cref="GetByIdAsync"/> returns null; every other member throws.
/// Suitable for interest tests where no equipment lookup is expected to resolve an item.
/// </summary>
public sealed class FakeItemService : IItemService
{
    public ValueTask<ItemEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
        => ValueTask.FromResult<ItemEntity?>(null);

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

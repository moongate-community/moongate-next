using Moongate.Core.Ids;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.UO.Data.Data;
using Moongate.UO.Data.Entities.Items;

namespace Moongate.Tests.Server.Items.Support;

public sealed class FakeItemAccess : IAutoDataAccess<ItemEntity, Serial>
{
    private readonly Dictionary<Serial, ItemEntity> _items = [];
    private uint _nextId = 1;

    public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(_items.Count);
    }

    public ValueTask<IReadOnlyCollection<ItemEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult<IReadOnlyCollection<ItemEntity>>(_items.Values.Select(Clone).ToArray());
    }

    public ValueTask<ItemEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(_items.TryGetValue(id, out var item) ? Clone(item) : null);
    }

    public ValueTask<Serial> NextIdAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(new Serial(_nextId++));
    }

    public IQueryable<ItemEntity> Query()
    {
        return _items.Values.Select(Clone).AsQueryable();
    }

    public ValueTask<bool> RemoveAsync(Serial id, CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(_items.Remove(id));
    }

    public ValueTask UpsertAsync(ItemEntity entity, CancellationToken cancellationToken = default)
    {
        _items[entity.Id] = Clone(entity);

        return ValueTask.CompletedTask;
    }

    private static ItemEntity Clone(ItemEntity item)
    {
        return new ItemEntity
        {
            Id = item.Id,
            Name = item.Name,
            ItemId = item.ItemId,
            Amount = item.Amount,
            Weight = item.Weight,
            MapId = item.MapId,
            Location = item.Location,
            ParentContainerId = item.ParentContainerId,
            EquippedMobileId = item.EquippedMobileId,
            ContainerPosition = item.ContainerPosition,
            ContainedItemIds = [.. item.ContainedItemIds],
            CustomProperties = new Dictionary<string, CustomProperty>(item.CustomProperties)
        };
    }
}

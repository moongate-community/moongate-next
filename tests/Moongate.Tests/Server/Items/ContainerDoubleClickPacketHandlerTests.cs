using Moongate.Abstractions.Data.Network;
using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Network.UO.Packets.Incoming.Interaction;
using Moongate.Server.Interfaces.Services.Items;
using Moongate.Server.Services.Items;
using Moongate.Tests.Support;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Interfaces.Services;

namespace Moongate.Tests.Server.Items;

public sealed class ContainerDoubleClickPacketHandlerTests
{
    [Fact]
    public async Task HandleAsync_ItemSerial_CallsContentService()
    {
        var item = new ItemEntity { Id = new(Serial.ItemOffset + 10), ItemId = 3651 };
        var items = new FakeItemService(item);
        var contents = new CapturingContainerContentService();
        var handler = new ContainerDoubleClickPacketHandler(items, contents);

        await handler.HandleAsync(Context(item.Id));

        Assert.Same(item, contents.Container);
    }

    [Fact]
    public async Task HandleAsync_UnknownSerial_DoesNotCallContentService()
    {
        var items = new FakeItemService();
        var contents = new CapturingContainerContentService();
        var handler = new ContainerDoubleClickPacketHandler(items, contents);

        await handler.HandleAsync(Context(new(Serial.ItemOffset + 99)));

        Assert.Null(contents.Container);
    }

    private static PacketContext<DoubleClickPacket> Context(Serial serial)
        => new(
            new FakeGameSession { SessionId = 42 },
            new() { TargetSerial = serial },
            DateTimeOffset.UtcNow,
            static (_, _, _) => Task.CompletedTask,
            static () => [42]
        );

    private sealed class CapturingContainerContentService : IContainerContentService
    {
        public ItemEntity? Container { get; private set; }

        public Task EnsureContentsAsync(ItemEntity container, CancellationToken cancellationToken = default)
        {
            Container = container;

            return Task.CompletedTask;
        }
    }

    private sealed class FakeItemService : IItemService
    {
        private readonly ItemEntity? _item;

        public FakeItemService(ItemEntity? item = null)
        {
            _item = item;
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
            => ValueTask.FromResult(_item is not null && _item.Id == id ? _item : null);

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
}

using Moongate.Abstractions.Interfaces.EventHandlers;
using Moongate.Abstractions.Interfaces.Events;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Network.UO.Packets.Outgoing.Entity;
using Moongate.Network.UO.Packets.Outgoing.Login;
using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Network;
using Moongate.Server.Interfaces.Services.Items;
using Moongate.Server.Services.World;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Interfaces.Maps;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Maps;
using Moongate.UO.Data.Types.Items;
using Xunit;

namespace Moongate.Tests.Server.World;

public sealed class WorldEntryServiceTests
{
    [Fact]
    public async Task EnterWorldAsync_SendsCoreSequence_AndPublishesEvent()
    {
        var torso = new ItemEntity { Id = new Serial(Serial.ItemOffset + 2), ItemId = 0x1F03 };
        var backpack = new ItemEntity { Id = new Serial(Serial.ItemOffset + 3) };
        var pack = new ItemEntity { Id = new Serial(Serial.ItemOffset + 4), ItemId = 0x0EED, Amount = 1 };
        backpack.ContainedItemIds.Add(pack.Id);

        var mobile = new MobileEntity
        {
            Id = new Serial(7), Name = "Tom", AccountId = new Serial(99),
            MapId = 0, Location = new Point3D(1, 2, 0), BackpackId = backpack.Id
        };
        mobile.EquippedItemIds[ItemLayerType.OuterTorso] = torso.Id;
        mobile.EquippedItemIds[ItemLayerType.Backpack] = backpack.Id;

        var outgoing = new RecordingOutgoingQueue();
        var items = new MapItemService(new[] { torso, backpack, pack });
        var contents = new NoopContainerContentService();
        var maps = new FakeMapService();
        var events = new RecordingEventBus();
        var service = new WorldEntryService(outgoing, items, contents, maps, events);

        await service.EnterWorldAsync(42, mobile, CancellationToken.None);

        var types = outgoing.Sent.Select(s => s.Packet.GetType()).ToList();
        Assert.Contains(typeof(LoginConfirmPacket), types);
        Assert.Contains(typeof(DrawPlayerPacket), types);
        Assert.Contains(typeof(PlayerStatusPacket), types);
        Assert.Contains(typeof(WornItemPacket), types);
        Assert.Contains(typeof(ContainerContentPacket), types);
        Assert.Contains(typeof(PaperdollPacket), types);
        Assert.Contains(typeof(LoginCompletePacket), types);
        Assert.Single(outgoing.Sent, s => s.Packet is WornItemPacket); // backpack layer excluded
        Assert.True(outgoing.Sent.All(s => s.SessionId == 42));
        Assert.Single(events.Published.OfType<PlayerCharacterLoggedInEvent>());
        Assert.True(types.IndexOf(typeof(LoginConfirmPacket)) < types.IndexOf(typeof(LoginCompletePacket)));
    }
}

/// <summary>
/// IOutgoingPacketQueue stub that records every enqueued packet with its session id.
/// </summary>
internal sealed class RecordingOutgoingQueue : IOutgoingPacketQueue
{
    public List<(long SessionId, IGameNetworkPacket Packet)> Sent { get; } = [];

    public int Count => Sent.Count;

    public int Clear(Action<Moongate.Server.Data.Network.OutgoingPacketEnvelope>? handler = null)
        => throw new NotSupportedException();

    public int Drain(int maxItems, Func<Moongate.Server.Data.Network.OutgoingPacketEnvelope, bool> handler)
        => throw new NotSupportedException();

    public void Enqueue<TPacket>(long sessionId, TPacket packet)
        where TPacket : IGameNetworkPacket
        => Sent.Add((sessionId, packet));
}

/// <summary>
/// IItemService stub that resolves items from a fixed map; all other members throw.
/// </summary>
internal sealed class MapItemService : IItemService
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

    public ValueTask<bool> RemoveItemAsync(ItemEntity container, Serial itemId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public ValueTask<int> TotalWeightAsync(ItemEntity item, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}

/// <summary>
/// IContainerContentService stub that performs no work.
/// </summary>
internal sealed class NoopContainerContentService : IContainerContentService
{
    public Task EnsureContentsAsync(ItemEntity container, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

/// <summary>
/// IMapService stub. Map has no usable public constructor, so GetMap returns null
/// and the service falls back to width/height 0.
/// </summary>
internal sealed class FakeMapService : IMapService
{
    public IReadOnlyList<Map> Maps => Array.Empty<Map>();

    public Map? GetMap(int mapId)
        => null;
}

/// <summary>
/// IEventBusService stub that records every published event; other members throw.
/// </summary>
internal sealed class RecordingEventBus : IEventBusService
{
    public List<object> Published { get; } = [];

    public Action<Type, Exception, IMoongateEvent>? OnEventError { get; set; }

    public int CurrentTickQueueDepth => throw new NotSupportedException();

    public int DrainTickEvents(int maxItems)
        => throw new NotSupportedException();

    public void Publish<TEvent>(TEvent evt)
        where TEvent : ITickEvent
        => Published.Add(evt!);

    public Task PublishAsync<TEvent>(TEvent evt, CancellationToken cancellationToken = default)
        where TEvent : IAsyncEvent
        => throw new NotSupportedException();

    public Task StartAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}

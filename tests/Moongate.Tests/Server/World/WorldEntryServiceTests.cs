using Moongate.Core.Ids;
using Moongate.Network.UO.Packets.Outgoing.Entity;
using Moongate.Network.UO.Packets.Outgoing.Login;
using Moongate.Server.Data.Events;
using Moongate.Server.Interfaces.Services.Items;
using Moongate.Server.Services.World;
using Moongate.Tests.Support;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Interfaces.Maps;
using Moongate.UO.Data.Maps;
using Moongate.UO.Data.Types.Items;

namespace Moongate.Tests.Server.World;

public sealed class WorldEntryServiceTests
{
    [Fact]
    public async Task EnterWorldAsync_SendsCoreSequence_AndPublishesEvent()
    {
        var torso = new ItemEntity { Id = new(Serial.ItemOffset + 2), ItemId = 0x1F03 };
        var backpack = new ItemEntity { Id = new(Serial.ItemOffset + 3) };
        var pack = new ItemEntity { Id = new(Serial.ItemOffset + 4), ItemId = 0x0EED, Amount = 1 };
        backpack.ContainedItemIds.Add(pack.Id);

        var mobile = new MobileEntity
        {
            Id = new(7), Name = "Tom", AccountId = new(99),
            MapId = 0, Location = new(1, 2, 0), BackpackId = backpack.Id
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

        var containerContent = outgoing.Sent
                                       .Select(s => s.Packet)
                                       .OfType<ContainerContentPacket>()
                                       .Single();
        Assert.Contains(containerContent.Items, i => i.Id == pack.Id);
    }
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

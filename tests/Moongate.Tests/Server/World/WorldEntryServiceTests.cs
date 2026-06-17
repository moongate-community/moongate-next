using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Network.UO.Packets.Outgoing.Entity;
using Moongate.Network.UO.Packets.Outgoing.Login;
using Moongate.Network.UO.Packets.Outgoing.World;
using Moongate.Network.UO.Types.Environment;
using Moongate.Server.Data.Events;
using Moongate.Server.Data.World;
using Moongate.Server.Interfaces.Network;
using Moongate.Server.Interfaces.Services.Items;
using Moongate.Server.Services.Player;
using Moongate.Server.Services.World;
using Moongate.Tests.Support;
using Moongate.UO.Data.Data.Maps;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Interfaces.Files;
using Moongate.UO.Data.Interfaces.Maps;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Maps;
using Moongate.UO.Data.Types.Items;
using Moongate.UO.Data.Types.Maps;

namespace Moongate.Tests.Server.World;

public sealed class WorldEntryServiceTests
{
    /// <summary>
    /// Builds a real InterestManagementService backed by an empty spatial index so existing
    /// world-entry tests construct the service without altering their assertions (the snapshot
    /// over an empty index sends nothing).
    /// </summary>
    private static InterestManagementService EmptyInterest(IOutgoingPacketQueue outgoing, IItemService items)
        => new(new WorldSpatialIndex(), outgoing, new PlayerSessionService(), items);

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
        var service = new WorldEntryService(outgoing, items, contents, maps, events, new FakeLightAndTimeService(), new StubRegionResolver(null), EmptyInterest(outgoing, items));

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

    [Fact]
    public async Task EnterWorldAsync_UnresolvedMap_FallsBackToSpring()
    {
        var mobile = new MobileEntity { Id = new(7), Name = "Tom", AccountId = new(99), MapId = 0 };

        var outgoing = new RecordingOutgoingQueue();
        var service = new WorldEntryService(
            outgoing,
            new MapItemService(Array.Empty<ItemEntity>()),
            new NoopContainerContentService(),
            new FakeMapService(),
            new RecordingEventBus(),
            new FakeLightAndTimeService(),
            new StubRegionResolver(null),
            EmptyInterest(outgoing, new MapItemService(Array.Empty<ItemEntity>()))
        );

        await service.EnterWorldAsync(42, mobile, CancellationToken.None);

        var season = outgoing.Sent.Select(s => s.Packet).OfType<SeasonPacket>().Single();
        Assert.Equal(SeasonType.Spring, season.Season);
    }

    [Fact]
    public async Task EnterWorldAsync_UsesLightAndTimeService()
    {
        var mobile = new MobileEntity { Id = new(7), Name = "Tom", AccountId = new(99), MapId = 0 };
        var outgoing = new RecordingOutgoingQueue();
        var service = new WorldEntryService(
            outgoing,
            new MapItemService(Array.Empty<ItemEntity>()),
            new NoopContainerContentService(),
            new FakeMapService(),
            new RecordingEventBus(),
            new FakeLightAndTimeService(),
            new StubRegionResolver(null),
            EmptyInterest(outgoing, new MapItemService(Array.Empty<ItemEntity>()))
        );

        await service.EnterWorldAsync(42, mobile, CancellationToken.None);

        var overall = outgoing.Sent.Select(s => s.Packet).OfType<OverallLightLevelPacket>().Single();
        Assert.Equal((LightLevelType)12, overall.LightLevel);

        var setTime = outgoing.Sent.Select(s => s.Packet).OfType<SetTimePacket>().Single();
        Assert.Equal(18, setTime.Time.Hour);
    }

    [Fact]
    public async Task EnterWorldAsync_UsesMapSeason()
    {
        var mobile = new MobileEntity { Id = new(7), Name = "Tom", AccountId = new(99), MapId = 0 };
        var definition = new MapDefinition(0, 0, 0, 7168, 4096, "Felucca", MapRulesType.FeluccaRules, SeasonType.Summer);
        var map = new Map(definition, new NoopFileResolver());

        var outgoing = new RecordingOutgoingQueue();
        var service = new WorldEntryService(
            outgoing,
            new MapItemService(Array.Empty<ItemEntity>()),
            new NoopContainerContentService(),
            new FakeMapService(map),
            new RecordingEventBus(),
            new FakeLightAndTimeService(),
            new StubRegionResolver(null),
            EmptyInterest(outgoing, new MapItemService(Array.Empty<ItemEntity>()))
        );

        await service.EnterWorldAsync(42, mobile, CancellationToken.None);

        var season = outgoing.Sent.Select(s => s.Packet).OfType<SeasonPacket>().Single();
        Assert.Equal(SeasonType.Summer, season.Season);
    }

    [Fact]
    public async Task EnterWorldAsync_SendsRegionMusic()
    {
        var mobile = new MobileEntity { Id = new Serial(7), Name = "Tom", AccountId = new Serial(99), MapId = 0 };
        var region = new RegionEntry("TownRegion", 0, "Felucca", "Britain", 1,
            new[] { new RegionAreaEntry(0, 0, 100, 100) }, "Britain1", null, null);

        var outgoing = new RecordingOutgoingQueue();
        var service = new WorldEntryService(
            outgoing,
            new MapItemService(Array.Empty<ItemEntity>()),
            new NoopContainerContentService(),
            new FakeMapService(),
            new RecordingEventBus(),
            new FakeLightAndTimeService(),
            new StubRegionResolver(region),
            EmptyInterest(outgoing, new MapItemService(Array.Empty<ItemEntity>())));

        await service.EnterWorldAsync(42, mobile, CancellationToken.None);

        var music = outgoing.Sent.Select(s => s.Packet).OfType<SetMusicPacket>().Single();
        Assert.Equal((int)MusicType.Britain1, music.MusicId);
    }

    [Fact]
    public async Task EnterWorldAsync_NoRegionMusic_SendsNoMusicPacket()
    {
        var mobile = new MobileEntity { Id = new Serial(7), Name = "Tom", AccountId = new Serial(99), MapId = 0 };

        var outgoing = new RecordingOutgoingQueue();
        var service = new WorldEntryService(
            outgoing,
            new MapItemService(Array.Empty<ItemEntity>()),
            new NoopContainerContentService(),
            new FakeMapService(),
            new RecordingEventBus(),
            new FakeLightAndTimeService(),
            new StubRegionResolver(null),
            EmptyInterest(outgoing, new MapItemService(Array.Empty<ItemEntity>())));

        await service.EnterWorldAsync(42, mobile, CancellationToken.None);

        Assert.Empty(outgoing.Sent.Select(s => s.Packet).OfType<SetMusicPacket>());
    }

    [Fact]
    public async Task EnterWorld_SendsNearbyMobileViaInterestSnapshot()
    {
        const long sid = 42;
        var now = DateTimeOffset.UnixEpoch;

        var outgoing = new RecordingOutgoingQueue();
        var items = new MapItemService(Array.Empty<ItemEntity>());
        var sessions = new PlayerSessionService();
        var index = new WorldSpatialIndex();
        var interest = new InterestManagementService(index, outgoing, sessions, items);

        var service = new WorldEntryService(
            outgoing,
            items,
            new NoopContainerContentService(),
            new FakeMapService(),
            new RecordingEventBus(),
            new FakeLightAndTimeService(),
            new StubRegionResolver(null),
            interest
        );

        var viewer = new MobileEntity
        {
            Id = new Serial(10), MapId = 0, Location = new Point3D(100, 100, 0), IsPlayer = true
        };
        index.AddMobile(viewer);

        var other = new MobileEntity { Id = new Serial(11), MapId = 0, Location = new Point3D(101, 100, 0) };
        index.AddMobile(other);

        sessions.GetOrCreateConnected(sid, null, now);
        sessions.EnterWorld(sid, new Serial(900), viewer.Id, now);

        await service.EnterWorldAsync(sid, viewer, CancellationToken.None);

        Assert.Contains(outgoing.Sent, s => s.Packet is MobileIncomingPacket p && p.Mobile.Id == other.Id);
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
/// IMapService stub. When constructed without a map, GetMap returns null
/// and the service falls back to width/height 0. Supply a map to test season forwarding.
/// </summary>
internal sealed class FakeMapService : IMapService
{
    private readonly Map? _map;

    public FakeMapService(Map? map = null)
    {
        _map = map;
    }

    public IReadOnlyList<Map> Maps => _map is null ? Array.Empty<Map>() : new[] { _map };

    public Map? GetMap(int mapId)
        => _map is not null && _map.MapId == mapId ? _map : null;
}

/// <summary>
/// IUoFileResolver stub. Tiles are never queried in unit tests so all members are no-ops.
/// </summary>
internal sealed class NoopFileResolver : IUoFileResolver
{
    public string RootDirectory => string.Empty;

    public bool Contains(string fileName)
        => false;

    public string? Resolve(string fileName)
        => null;
}

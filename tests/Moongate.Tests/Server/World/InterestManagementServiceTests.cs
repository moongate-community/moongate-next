using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Network.UO.Packets.Outgoing.Entity;
using Moongate.Server.Data.Events;
using Moongate.Server.Services.Player;
using Moongate.Server.Services.World;
using Moongate.Tests.Support;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;
using Xunit;

namespace Moongate.Tests.Server.World;

public sealed class InterestManagementServiceTests
{
    private const long ViewerSession = 1;

    private static (InterestManagementService svc, WorldSpatialIndex index, RecordingOutgoingPacketQueue outgoing, PlayerSessionService sessions, MobileEntity viewer)
        Build()
    {
        var index = new WorldSpatialIndex();
        var outgoing = new RecordingOutgoingPacketQueue();
        var sessions = new PlayerSessionService();
        var viewerSerial = new Serial(10);
        sessions.GetOrCreateConnected(ViewerSession, null, DateTimeOffset.UtcNow);
        sessions.EnterWorld(ViewerSession, new Serial(900), viewerSerial, DateTimeOffset.UtcNow);
        var viewer = new MobileEntity { Id = viewerSerial, MapId = 0, Location = new Point3D(100, 100, 0), IsPlayer = true };
        index.AddMobile(viewer);
        var svc = new InterestManagementService(index, outgoing, sessions, new FakeItems());
        return (svc, index, outgoing, sessions, viewer);
    }

    [Fact]
    public async Task SendInitialSnapshot_SendsNearbyMobilesAndItems_AndSeedsKnownSet()
    {
        var (svc, index, outgoing, _, viewer) = Build();
        var other = new MobileEntity { Id = new Serial(11), MapId = 0, Location = new Point3D(101, 100, 0) }; // dist 1, in range
        var far = new MobileEntity { Id = new Serial(12), MapId = 0, Location = new Point3D(100, 150, 0) };    // dist 50, out of range (>24)
        var item = new ItemEntity { Id = new Serial(0x40000001), MapId = 0, Location = new Point3D(100, 101, 0) };
        index.AddMobile(other);
        index.AddMobile(far);
        index.AddOrUpdateItem(item);

        await svc.SendInitialSnapshotAsync(ViewerSession, viewer);

        Assert.Contains(outgoing.Sent, s => s.Packet is MobileIncomingPacket p && p.Mobile.Id == other.Id);
        Assert.Contains(outgoing.Sent, s => s.Packet is ObjectInformationPacket p && p.Item.Id == item.Id);
        Assert.DoesNotContain(outgoing.Sent, s => s.Packet is MobileIncomingPacket p && p.Mobile.Id == far.Id);   // out of range
        Assert.DoesNotContain(outgoing.Sent, s => s.Packet is MobileIncomingPacket p && p.Mobile.Id == viewer.Id); // never self
    }

    [Fact]
    public async Task OnEntityRemoved_SendsDeleteToObserversThatKnewIt()
    {
        var (svc, index, outgoing, _, viewer) = Build();
        var other = new MobileEntity { Id = new Serial(11), MapId = 0, Location = new Point3D(101, 100, 0) };
        index.AddMobile(other);
        await svc.SendInitialSnapshotAsync(ViewerSession, viewer); // viewer now knows 'other'
        outgoing.Sent.Clear();

        svc.OnEntityRemoved(other.Id);

        Assert.Contains(outgoing.Sent, s => s.SessionId == ViewerSession && s.Packet is DeleteObjectPacket d && d.Serial == other.Id);
    }

    [Fact]
    public async Task OnMobileMoved_ObserverGainsThenLosesSight()
    {
        var (svc, index, outgoing, sessions, viewer) = Build(); // viewer at (100,100), session 1, range 24
        var mover = new MobileEntity { Id = new Serial(20), MapId = 0, Location = new Point3D(100, 130, 0) }; // out of range (30 > 24)
        index.AddMobile(mover);

        // mover steps to (100,124) -> within range 24 of viewer -> viewer gets MobileIncoming
        index.MoveMobile(mover, new Point3D(100, 124, 0));
        await svc.OnMobileMovedAsync(new MobileMovedEvent(mover.Id, 0, new Point3D(100, 130, 0), new Point3D(100, 124, 0), Moongate.Core.Types.DirectionType.North));
        Assert.Contains(outgoing.Sent, s => s.SessionId == ViewerSession && s.Packet is MobileIncomingPacket p && p.Mobile.Id == mover.Id);
        outgoing.Sent.Clear();

        // mover steps within view -> MobileMoving
        index.MoveMobile(mover, new Point3D(100, 123, 0));
        await svc.OnMobileMovedAsync(new MobileMovedEvent(mover.Id, 0, new Point3D(100, 124, 0), new Point3D(100, 123, 0), Moongate.Core.Types.DirectionType.North));
        Assert.Contains(outgoing.Sent, s => s.SessionId == ViewerSession && s.Packet is MobileMovingPacket p && p.Mobile.Id == mover.Id);
        outgoing.Sent.Clear();

        // mover steps back out of range -> DeleteObject
        index.MoveMobile(mover, new Point3D(100, 130, 0));
        await svc.OnMobileMovedAsync(new MobileMovedEvent(mover.Id, 0, new Point3D(100, 123, 0), new Point3D(100, 130, 0), Moongate.Core.Types.DirectionType.South));
        Assert.Contains(outgoing.Sent, s => s.SessionId == ViewerSession && s.Packet is DeleteObjectPacket d && d.Serial == mover.Id);
    }

    private sealed class FakeItems : Moongate.UO.Data.Interfaces.Services.IItemService
    {
        public ValueTask<ItemEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
            => ValueTask.FromResult<ItemEntity?>(null);
        // throw for everything else (unused):
        public ValueTask<bool> AddItemAsync(ItemEntity c, ItemEntity ch, Moongate.Core.Geometry.Point2D p, CancellationToken ct = default) => throw new NotSupportedException();
        public ValueTask<int> CountAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public ValueTask<ItemEntity> CreateAsync(ItemEntity i, CancellationToken ct = default) => throw new NotSupportedException();
        public ValueTask<bool> DeleteAsync(Serial id, CancellationToken ct = default) => throw new NotSupportedException();
        public bool IsContainer(ItemEntity i) => throw new NotSupportedException();
        public bool IsContainer(int i) => throw new NotSupportedException();
        public bool IsDoor(ItemEntity i) => throw new NotSupportedException();
        public bool IsDoor(int i) => throw new NotSupportedException();
        public ValueTask<bool> RemoveItemAsync(ItemEntity c, Serial id, CancellationToken ct = default) => throw new NotSupportedException();
        public ValueTask<int> TotalWeightAsync(ItemEntity i, CancellationToken ct = default) => throw new NotSupportedException();
    }
}

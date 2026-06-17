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
        var svc = new InterestManagementService(index, outgoing, sessions, new FakeItemService());
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

    [Fact]
    public async Task OnMobileMoved_MoverSide_SeesEntitiesEnterAndLeaveView()
    {
        var (svc, index, outgoing, _, viewer) = Build(); // viewer is a PLAYER at (100,100), session 1, range 24
        var target = new MobileEntity { Id = new Serial(30), MapId = 0, Location = new Point3D(100, 140, 0) }; // dist 40, out of range (non-player)
        var item = new ItemEntity { Id = new Serial(0x40000010), MapId = 0, Location = new Point3D(100, 141, 0) }; // dist 41, out of range
        index.AddMobile(target);
        index.AddOrUpdateItem(item);
        outgoing.Sent.Clear(); // start with an empty known-set / no prior packets

        // Step 1: viewer walks toward them to (100,120) -> target (dist 20) and item (dist 21) enter view (<= 24)
        index.MoveMobile(viewer, new Point3D(100, 120, 0));
        await svc.OnMobileMovedAsync(new MobileMovedEvent(viewer.Id, 0, new Point3D(100, 100, 0), new Point3D(100, 120, 0), Moongate.Core.Types.DirectionType.South));
        Assert.Contains(outgoing.Sent, s => s.SessionId == ViewerSession && s.Packet is MobileIncomingPacket p && p.Mobile.Id == target.Id);
        Assert.Contains(outgoing.Sent, s => s.SessionId == ViewerSession && s.Packet is ObjectInformationPacket p && p.Item.Id == item.Id);
        outgoing.Sent.Clear();

        // Step 2: viewer walks back to (100,100) -> target (dist 40) and item leave view -> DeleteObject for both
        index.MoveMobile(viewer, new Point3D(100, 100, 0));
        await svc.OnMobileMovedAsync(new MobileMovedEvent(viewer.Id, 0, new Point3D(100, 120, 0), new Point3D(100, 100, 0), Moongate.Core.Types.DirectionType.North));
        Assert.Contains(outgoing.Sent, s => s.SessionId == ViewerSession && s.Packet is DeleteObjectPacket d && d.Serial == target.Id);
        Assert.Contains(outgoing.Sent, s => s.SessionId == ViewerSession && s.Packet is DeleteObjectPacket d && d.Serial == item.Id);
    }
}

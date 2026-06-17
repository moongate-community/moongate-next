using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Network.UO.Packets.Outgoing.Entity;
using Moongate.Server.Data.Events;
using Moongate.Server.Handlers.World;
using Moongate.Server.Services.Player;
using Moongate.Server.Services.World;
using Moongate.Tests.Support;
using Moongate.UO.Data.Entities.Mobiles;
using Xunit;

namespace Moongate.Tests.Server.World;

public sealed class PlayerDisconnectedInterestHandlerTests
{
    [Fact]
    public async Task Handle_SendsDeleteOfLeaver_ToObserversThatKnewIt()
    {
        var index = new WorldSpatialIndex();
        var outgoing = new RecordingOutgoingPacketQueue();
        var sessions = new PlayerSessionService();

        var observerSerial = new Serial(10);
        sessions.GetOrCreateConnected(1, null, DateTimeOffset.UtcNow);
        sessions.EnterWorld(1, new Serial(900), observerSerial, DateTimeOffset.UtcNow);
        var observer = new MobileEntity { Id = observerSerial, MapId = 0, Location = new Point3D(100, 100, 0), IsPlayer = true };
        index.AddMobile(observer);

        var leaverSerial = new Serial(11);
        sessions.GetOrCreateConnected(2, null, DateTimeOffset.UtcNow);
        sessions.EnterWorld(2, new Serial(901), leaverSerial, DateTimeOffset.UtcNow);
        var leaver = new MobileEntity { Id = leaverSerial, MapId = 0, Location = new Point3D(101, 100, 0), IsPlayer = true };
        index.AddMobile(leaver);

        var interest = new InterestManagementService(index, outgoing, sessions, new FakeItemService());
        await interest.SendInitialSnapshotAsync(1, observer); // observer (session 1) now knows the leaver
        outgoing.Sent.Clear();

        var handler = new PlayerDisconnectedInterestHandler(sessions, interest);
        handler.Handle(new PlayerDisconnectedEvent(2, null, DateTimeOffset.UtcNow));

        Assert.Contains(outgoing.Sent, s => s.SessionId == 1 && s.Packet is DeleteObjectPacket d && d.Serial == leaverSerial);
    }
}

using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Data.Player;
using Moongate.Abstractions.Data.Version;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Core.Types;
using Moongate.Network.UO.Packets.Incoming.Movement;
using Moongate.Network.UO.Packets.Outgoing.Movement;
using Moongate.Server.Handlers.Movement;
using Moongate.Server.Interfaces.Services.Movement;
using Moongate.Server.Data.Events;
using Moongate.Server.Services.Player;
using Moongate.Server.Services.World;
using Moongate.Tests.Support;
using Moongate.UO.Data.Entities.Mobiles;

namespace Moongate.Tests.Server.Movement;

public sealed class MoveRequestHandlerTests
{
    private const long InWorldSessionId = 77;
    private static readonly Serial MobileId = new(123);

    private sealed class FakeMovementValidation : IMovementValidationService
    {
        private readonly bool _accept;

        public FakeMovementValidation(bool accept)
        {
            _accept = accept;
        }

        public bool TryResolveMove(MobileEntity mobile, DirectionType direction, out Point3D newLocation)
        {
            if (_accept)
            {
                newLocation = mobile.Location.Move(direction);

                return true;
            }

            newLocation = mobile.Location;

            return false;
        }
    }

    [Fact]
    public async Task ValidStep_UpdatesLiveMobile_AndConfirms()
    {
        var mobile = new MobileEntity
        {
            Id = MobileId,
            Direction = DirectionType.South,
            Location = new Point3D(50, 50, 0)
        };
        var registry = new WorldSpatialIndex();
        registry.AddMobile(mobile);

        var sent = new List<IGameNetworkPacket>();
        var handler = CreateHandler(registry, accept: true);

        await handler.HandleAsync(Context(DirectionType.South, sequence: 0, sent));

        Assert.Contains(sent, static p => p is MoveConfirmPacket);
        Assert.DoesNotContain(sent, static p => p is MoveDenyPacket);
        Assert.True(registry.TryGet(MobileId, out var live));
        Assert.Equal(new Point3D(50, 51, 0), live.Location);
    }

    [Fact]
    public async Task InvalidStep_Denies()
    {
        var mobile = new MobileEntity
        {
            Id = MobileId,
            Direction = DirectionType.South,
            Location = new Point3D(50, 50, 0)
        };
        var registry = new WorldSpatialIndex();
        registry.AddMobile(mobile);

        var sent = new List<IGameNetworkPacket>();
        var handler = CreateHandler(registry, accept: false);

        await handler.HandleAsync(Context(DirectionType.South, sequence: 0, sent));

        Assert.Contains(sent, static p => p is MoveDenyPacket);
        Assert.DoesNotContain(sent, static p => p is MoveConfirmPacket);
        Assert.True(registry.TryGet(MobileId, out var live));
        Assert.Equal(new Point3D(50, 50, 0), live.Location);
    }

    [Fact]
    public async Task TurnOnly_TurnsWithoutMoving()
    {
        var mobile = new MobileEntity
        {
            Id = MobileId,
            Direction = DirectionType.South,
            Location = new Point3D(50, 50, 0)
        };
        var registry = new WorldSpatialIndex();
        registry.AddMobile(mobile);

        var sent = new List<IGameNetworkPacket>();
        var handler = CreateHandler(registry, accept: true);

        await handler.HandleAsync(Context(DirectionType.East, sequence: 0, sent));

        Assert.Contains(sent, static p => p is MoveConfirmPacket);
        Assert.True(registry.TryGet(MobileId, out var live));
        Assert.Equal(new Point3D(50, 50, 0), live.Location);
        Assert.Equal(DirectionType.East, live.Direction);
    }

    [Fact]
    public async Task ValidStep_ConfirmEchoesRequestSequence_AndAdvancesSequence()
    {
        var mobile = new MobileEntity
        {
            Id = MobileId,
            Direction = DirectionType.South,
            Location = new Point3D(50, 50, 0)
        };
        var registry = new WorldSpatialIndex();
        registry.AddMobile(mobile);

        var sent = new List<IGameNetworkPacket>();
        var handler = CreateHandler(registry, accept: true, out var sessions);
        sessions.UpdateMovementState(InWorldSessionId, moveSequence: 0, moveCredit: 0, moveTime: 0);

        await handler.HandleAsync(Context(DirectionType.South, sequence: 0, sent));

        var confirm = Assert.IsType<MoveConfirmPacket>(Assert.Single(sent));
        Assert.Equal((byte)0, confirm.Sequence);
        Assert.True(sessions.TryGetBySessionId(InWorldSessionId, out var session));
        Assert.Equal((byte)1, session.MoveSequence);
    }

    [Fact]
    public async Task SequenceWrap_From255_ResetsToOne()
    {
        var mobile = new MobileEntity
        {
            Id = MobileId,
            Direction = DirectionType.South,
            Location = new Point3D(50, 50, 0)
        };
        var registry = new WorldSpatialIndex();
        registry.AddMobile(mobile);

        var sent = new List<IGameNetworkPacket>();
        var handler = CreateHandler(registry, accept: true, out var sessions);
        sessions.UpdateMovementState(InWorldSessionId, moveSequence: 255, moveCredit: 0, moveTime: 0);

        await handler.HandleAsync(Context(DirectionType.South, sequence: 255, sent));

        Assert.Contains(sent, static p => p is MoveConfirmPacket);
        Assert.True(sessions.TryGetBySessionId(InWorldSessionId, out var session));
        Assert.Equal((byte)1, session.MoveSequence);
    }

    [Fact]
    public async Task ResyncMismatch_DropsWithoutReply()
    {
        var mobile = new MobileEntity
        {
            Id = MobileId,
            Direction = DirectionType.South,
            Location = new Point3D(50, 50, 0)
        };
        var registry = new WorldSpatialIndex();
        registry.AddMobile(mobile);

        var sent = new List<IGameNetworkPacket>();
        var handler = CreateHandler(registry, accept: true, out var sessions);
        sessions.UpdateMovementState(InWorldSessionId, moveSequence: 0, moveCredit: 0, moveTime: 0);

        await handler.HandleAsync(Context(DirectionType.South, sequence: 5, sent));

        Assert.Empty(sent);
        Assert.True(registry.TryGet(MobileId, out var live));
        Assert.Equal(new Point3D(50, 50, 0), live.Location);
    }

    [Fact]
    public async Task Throttled_Denies_WithoutResettingSequence()
    {
        var mobile = new MobileEntity
        {
            Id = MobileId,
            Direction = DirectionType.South,
            Location = new Point3D(50, 50, 0)
        };
        var registry = new WorldSpatialIndex();
        registry.AddMobile(mobile);

        var sent = new List<IGameNetworkPacket>();
        var handler = CreateHandler(registry, accept: true, out var sessions);
        sessions.UpdateMovementState(
            InWorldSessionId,
            moveSequence: 1,
            moveCredit: 0,
            moveTime: Environment.TickCount64 + 10_000_000
        );

        await handler.HandleAsync(Context(DirectionType.South, sequence: 1, sent));

        Assert.Contains(sent, static p => p is MoveDenyPacket);
        Assert.DoesNotContain(sent, static p => p is MoveConfirmPacket);
        Assert.True(registry.TryGet(MobileId, out var live));
        Assert.Equal(new Point3D(50, 50, 0), live.Location);
        Assert.True(sessions.TryGetBySessionId(InWorldSessionId, out var session));
        Assert.Equal((byte)1, session.MoveSequence);
    }

    [Fact]
    public async Task TwoStepWalk_PersistsSequence_AcrossPackets()
    {
        var mobile = new MobileEntity
        {
            Id = MobileId,
            Direction = DirectionType.South,
            Location = new Point3D(50, 50, 0)
        };
        var registry = new WorldSpatialIndex();
        registry.AddMobile(mobile);

        var sent = new List<IGameNetworkPacket>();
        var handler = CreateHandler(registry, accept: true, out var sessions);

        // Seed fastwalk credit and a current move time so the throttle never denies the back-to-back
        // packets; this test isolates sequence persistence across packets.
        sessions.UpdateMovementState(
            InWorldSessionId,
            moveSequence: 0,
            moveCredit: 5000,
            moveTime: Environment.TickCount64
        );

        await handler.HandleAsync(Context(DirectionType.South, sequence: 0, sent));
        await handler.HandleAsync(Context(DirectionType.South, sequence: 1, sent));

        Assert.Equal(2, sent.Count(static p => p is MoveConfirmPacket));
        Assert.DoesNotContain(sent, static p => p is MoveDenyPacket);
        Assert.True(registry.TryGet(MobileId, out var live));
        Assert.Equal(new Point3D(50, 52, 0), live.Location);
    }

    [Fact]
    public async Task ValidStep_UpdatesIndexLocation_AndPublishesMobileMovedEvent()
    {
        var mobile = new MobileEntity
        {
            Id = MobileId,
            MapId = 0,
            Direction = DirectionType.South,
            Location = new Point3D(50, 50, 0)
        };
        var registry = new WorldSpatialIndex();
        registry.AddMobile(mobile);

        var sent = new List<IGameNetworkPacket>();
        var handler = CreateHandler(registry, accept: true, out _, out var events);

        await handler.HandleAsync(Context(DirectionType.South, sequence: 0, sent));

        Assert.Contains(sent, static p => p is MoveConfirmPacket);
        Assert.True(registry.TryGet(MobileId, out var live));
        Assert.Equal(new Point3D(50, 51, 0), live.Location);

        var moved = Assert.Single(events.Published.OfType<MobileMovedEvent>());
        Assert.Equal(MobileId, moved.MobileId);
        Assert.Equal(0, moved.MapId);
        Assert.Equal(new Point3D(50, 50, 0), moved.OldLocation);
        Assert.Equal(new Point3D(50, 51, 0), moved.NewLocation);
        Assert.Equal(DirectionType.South, moved.Direction);
    }

    [Fact]
    public async Task TurnOnly_DoesNotPublishMobileMovedEvent()
    {
        var mobile = new MobileEntity
        {
            Id = MobileId,
            MapId = 0,
            Direction = DirectionType.South,
            Location = new Point3D(50, 50, 0)
        };
        var registry = new WorldSpatialIndex();
        registry.AddMobile(mobile);

        var sent = new List<IGameNetworkPacket>();
        var handler = CreateHandler(registry, accept: true, out _, out var events);

        await handler.HandleAsync(Context(DirectionType.East, sequence: 0, sent));

        Assert.Contains(sent, static p => p is MoveConfirmPacket);
        Assert.DoesNotContain(events.Published, static e => e is MobileMovedEvent);
    }

    private static MoveRequestHandler CreateHandler(WorldSpatialIndex registry, bool accept)
        => CreateHandler(registry, accept, out _, out _);

    private static MoveRequestHandler CreateHandler(
        WorldSpatialIndex registry,
        bool accept,
        out PlayerSessionService sessions
    )
        => CreateHandler(registry, accept, out sessions, out _);

    private static MoveRequestHandler CreateHandler(
        WorldSpatialIndex registry,
        bool accept,
        out PlayerSessionService sessions,
        out RecordingEventBusService events
    )
    {
        sessions = new PlayerSessionService();
        sessions.GetOrCreateConnected(InWorldSessionId, null, DateTimeOffset.UtcNow);
        sessions.EnterWorld(InWorldSessionId, new Serial(456), MobileId, DateTimeOffset.UtcNow);
        events = new RecordingEventBusService();

        return new MoveRequestHandler(
            events,
            new NoopNetworkSessionManager(),
            sessions,
            registry,
            new FakeMovementValidation(accept)
        );
    }

    private static PacketContext<MoveRequestPacket> Context(
        DirectionType direction,
        byte sequence,
        List<IGameNetworkPacket> sent
    )
        => new(
            new FakeGameSession { SessionId = InWorldSessionId },
            new MoveRequestPacket { Direction = direction, Sequence = sequence },
            DateTimeOffset.UtcNow,
            (_, packet, _) =>
            {
                sent.Add(packet);

                return Task.CompletedTask;
            },
            static () => [InWorldSessionId]
        );
}

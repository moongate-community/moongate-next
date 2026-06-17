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
using Moongate.Server.Services.World;
using Moongate.Tests.Support;
using Moongate.UO.Data.Entities.Mobiles;
using ZLinq;
using ZLinq.Linq;

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

    private sealed class FakePlayerSessionService : IPlayerSessionService
    {
        private readonly PlayerSession _session;

        public FakePlayerSessionService(PlayerSession session)
        {
            _session = session;
        }

        public int Count => throw new NotSupportedException();

        public PlayerSession Authenticate(long sessionId, Serial userId, string username, DateTimeOffset authenticatedAt)
            => throw new NotSupportedException();

        public bool Disconnect(long sessionId, DateTimeOffset disconnectedAt)
            => throw new NotSupportedException();

        public PlayerSession EnterWorld(
            long sessionId,
            Serial characterSerial,
            Serial mobileSerial,
            DateTimeOffset enteredWorldAt
        )
            => throw new NotSupportedException();

        public IReadOnlyCollection<PlayerSession> GetAll()
            => throw new NotSupportedException();

        public PlayerSession GetOrCreateConnected(long sessionId, string? remoteEndPoint, DateTimeOffset connectedAt)
            => throw new NotSupportedException();

        public ValueEnumerable<FromArray<PlayerSession>, PlayerSession> Query()
            => throw new NotSupportedException();

        public bool Remove(long sessionId)
            => throw new NotSupportedException();

        public bool TryGetByMobileSerial(Serial mobileSerial, out PlayerSession session)
            => throw new NotSupportedException();

        public bool TryGetBySessionId(long sessionId, out PlayerSession session)
        {
            if (sessionId == InWorldSessionId)
            {
                session = _session;

                return true;
            }

            session = null!;

            return false;
        }

        public PlayerSession UpdateClient(long sessionId, ClientVersion? clientVersion = null, int? viewRange = null)
            => throw new NotSupportedException();
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
        var registry = new WorldMobileRegistry();
        registry.Add(mobile);

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
        var registry = new WorldMobileRegistry();
        registry.Add(mobile);

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
        var registry = new WorldMobileRegistry();
        registry.Add(mobile);

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
        var registry = new WorldMobileRegistry();
        registry.Add(mobile);

        var sent = new List<IGameNetworkPacket>();
        var handler = CreateHandler(registry, accept: true, out var session);
        session.MoveSequence = 0;

        await handler.HandleAsync(Context(DirectionType.South, sequence: 0, sent));

        var confirm = Assert.IsType<MoveConfirmPacket>(Assert.Single(sent));
        Assert.Equal((byte)0, confirm.Sequence);
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
        var registry = new WorldMobileRegistry();
        registry.Add(mobile);

        var sent = new List<IGameNetworkPacket>();
        var handler = CreateHandler(registry, accept: true, out var session);
        session.MoveSequence = 255;

        await handler.HandleAsync(Context(DirectionType.South, sequence: 255, sent));

        Assert.Contains(sent, static p => p is MoveConfirmPacket);
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
        var registry = new WorldMobileRegistry();
        registry.Add(mobile);

        var sent = new List<IGameNetworkPacket>();
        var handler = CreateHandler(registry, accept: true, out var session);
        session.MoveSequence = 0;

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
        var registry = new WorldMobileRegistry();
        registry.Add(mobile);

        var sent = new List<IGameNetworkPacket>();
        var handler = CreateHandler(registry, accept: true, out var session);
        session.MoveSequence = 1;
        session.MoveTime = Environment.TickCount64 + 10_000_000;
        session.MoveCredit = 0;

        await handler.HandleAsync(Context(DirectionType.South, sequence: 1, sent));

        Assert.Contains(sent, static p => p is MoveDenyPacket);
        Assert.DoesNotContain(sent, static p => p is MoveConfirmPacket);
        Assert.True(registry.TryGet(MobileId, out var live));
        Assert.Equal(new Point3D(50, 50, 0), live.Location);
        Assert.Equal((byte)1, session.MoveSequence);
    }

    private static MoveRequestHandler CreateHandler(WorldMobileRegistry registry, bool accept)
        => CreateHandler(registry, accept, out _);

    private static MoveRequestHandler CreateHandler(WorldMobileRegistry registry, bool accept, out PlayerSession session)
    {
        session = new PlayerSession
        {
            SessionId = InWorldSessionId,
            State = Abstractions.Types.Player.PlayerSessionStateType.InWorld,
            MobileSerial = MobileId
        };

        return new MoveRequestHandler(
            new NoopEventBusService(),
            new NoopNetworkSessionManager(),
            new FakePlayerSessionService(session),
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

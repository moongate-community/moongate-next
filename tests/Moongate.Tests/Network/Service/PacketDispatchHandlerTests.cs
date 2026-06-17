using System.Net.Sockets;
using DryIoc;
using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Abstractions.Interfaces.EventHandlers;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Network;
using Moongate.Network.Client;
using Moongate.Network.Spans;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Registry;
using Moongate.Server.Data.Events;
using Moongate.Server.Extensions.EventBus;
using Moongate.Server.Extensions.Network;
using Moongate.Server.Interfaces.Network;
using Moongate.Server.Services.Network;
using Moongate.Server.Services.Player;

namespace Moongate.Tests.Network.Service;

public class PacketDispatchHandlerTests
{
    [Fact]
    public void AddMoongateNetwork_RegistersPlayerSessionServiceAndEventHandlers()
    {
        var container = new Container();
        container.RegisterInstance(new PacketRegistry());

        container.AddMoongateNetwork();

        Assert.Same(
            container.Resolve<IPlayerSessionService>(),
            container.Resolve<PlayerSessionService>()
        );
        Assert.Contains(
            container.ResolveMany<ITickEventHandler<PlayerConnectedEvent>>(),
            static handler => handler is PlayerSessionService
        );
        Assert.Contains(
            container.ResolveMany<ITickEventHandler<PlayerDisconnectedEvent>>(),
            static handler => handler is PlayerSessionService
        );
    }

    [Fact]
    public void AddMoongatePacketHandlers_RegistersDispatcherAsTickHandler()
    {
        var container = new Container();
        container.Register<IOutgoingPacketQueue, OutgoingPacketQueue>(Reuse.Singleton);
        container.Register<SessionService>(Reuse.Singleton);
        container.RegisterMapping<ISessionService, SessionService>();
        container.RegisterMapping<INetworkSessionManager, SessionService>();

        container.AddMoongatePacketHandlers();

        var handlers = container.ResolveMany<ITickEventHandler<PacketReceivedEvent>>().ToArray();

        Assert.Contains(handlers, static handler => handler.GetType() == typeof(PacketDispatchHandler));
    }

    [Fact]
    public void AddPacketHandler_BaseClass_ReceivesEventBusAndSessions()
    {
        var container = new Container();
        container.RegisterInstance(new PacketRegistry());
        container.RegisterInstance(new BaseHandlerProbe());
        container.AddMoongateEventBus();
        container.AddMoongateNetwork();
        container.AddMoongatePacketHandlers();
        container.AddPacketHandler<BaseHandler, TestPacket>();

        var bus = container.Resolve<IEventBusService>();
        using var client = NewClient();
        var sessionId = container.Resolve<SessionService>().GetOrCreate(client).SessionId;
        bus.DrainTickEvents(10);

        bus.Publish(new PacketReceivedEvent(sessionId, 0xA1, new TestPacket(0xA1), DateTimeOffset.UtcNow));
        bus.DrainTickEvents(10);

        var probe = container.Resolve<BaseHandlerProbe>();
        Assert.Same(bus, probe.EventBus);
        Assert.Same(container.Resolve<INetworkSessionManager>(), probe.Sessions);
    }

    [Fact]
    public void AddPacketHandler_RegistersHandlerMapping()
    {
        var container = new Container();

        container.AddPacketHandler<CapturingHandler, TestPacket>();

        var handlers = container.ResolveMany<IPacketHandler<TestPacket>>().ToArray();

        Assert.Single(handlers);
        Assert.IsType<CapturingHandler>(handlers[0]);
    }

    [Fact]
    public void EventBus_DrainsPacketReceivedEvent_InvokesTypedHandler()
    {
        var container = new Container();
        container.RegisterInstance(new PacketRegistry());
        container.RegisterInstance(new IntegrationCapture());
        container.AddMoongateEventBus();
        container.AddMoongateNetwork();
        container.AddMoongatePacketHandlers();
        container.AddPacketHandler<IntegrationHandler, TestPacket>();

        var bus = container.Resolve<IEventBusService>();
        using var client = NewClient();
        var sessionId = container.Resolve<SessionService>().GetOrCreate(client).SessionId;
        bus.DrainTickEvents(10);

        bus.Publish(new PacketReceivedEvent(sessionId, 0xA1, new TestPacket(0xA1), DateTimeOffset.UtcNow));
        var processed = bus.DrainTickEvents(10);

        Assert.Equal(1, processed);
        Assert.Equal(new[] { sessionId }, container.Resolve<IntegrationCapture>().SessionIds);
    }

    [Fact]
    public void Handle_HandlerThrows_ContinuesWithRemainingHandlers()
    {
        using var client = NewClient();
        var sessions = new SessionService();
        var sessionId = sessions.GetOrCreate(client).SessionId;
        var capturing = new CapturingHandler();
        var dispatcher = NewDispatcher([new ThrowingHandler(), capturing], sessions);

        dispatcher.Handle(new PacketReceivedEvent(sessionId, 0xA1, new TestPacket(0xA1), DateTimeOffset.UtcNow));

        Assert.Equal(new[] { sessionId }, capturing.SessionIds);
    }

    [Fact]
    public void Handle_MatchingPacket_InvokesTypedHandler()
    {
        using var client = NewClient();
        var sessions = new SessionService();
        var sessionId = sessions.GetOrCreate(client).SessionId;
        var handler = new CapturingHandler();
        var dispatcher = NewDispatcher([handler], sessions);

        dispatcher.Handle(new PacketReceivedEvent(sessionId, 0xA1, new TestPacket(0xA1), DateTimeOffset.UtcNow));

        Assert.Equal(new[] { sessionId }, handler.SessionIds);
    }

    [Fact]
    public void Handle_MultipleMatchingHandlers_InvokesAll()
    {
        using var client = NewClient();
        var sessions = new SessionService();
        var sessionId = sessions.GetOrCreate(client).SessionId;
        var first = new CapturingHandler();
        var second = new CapturingHandler();
        var dispatcher = NewDispatcher([first, second], sessions);

        dispatcher.Handle(new PacketReceivedEvent(sessionId, 0xA1, new TestPacket(0xA1), DateTimeOffset.UtcNow));

        Assert.Equal(new[] { sessionId }, first.SessionIds);
        Assert.Equal(new[] { sessionId }, second.SessionIds);
    }

    [Fact]
    public void Handle_NonMatchingPacket_DoesNotInvokeUnrelatedHandler()
    {
        using var client = NewClient();
        var sessions = new SessionService();
        var sessionId = sessions.GetOrCreate(client).SessionId;
        var handler = new CapturingHandler();
        var dispatcher = NewDispatcher([handler], sessions);

        dispatcher.Handle(new PacketReceivedEvent(sessionId, 0xA2, new OtherPacket(), DateTimeOffset.UtcNow));

        Assert.Empty(handler.SessionIds);
    }

    [Fact]
    public void Handle_PassesGameSessionToContext()
    {
        using var client = NewClient();
        var sessions = new SessionService();
        var gameSession = sessions.GetOrCreate(client);
        var handler = new SessionCapturingHandler();
        var dispatcher = NewDispatcher([handler], sessions);

        dispatcher.Handle(new PacketReceivedEvent(gameSession.SessionId, 0xA1, new TestPacket(0xA1), DateTimeOffset.UtcNow));

        Assert.Same(gameSession, handler.Session);
    }

    [Fact]
    public void Handle_UnknownSession_SkipsDispatch()
    {
        var handler = new CapturingHandler();
        var dispatcher = NewDispatcher([handler], new SessionService());

        dispatcher.Handle(new PacketReceivedEvent(404, 0xA1, new TestPacket(0xA1), DateTimeOffset.UtcNow));

        Assert.Empty(handler.SessionIds);
    }

    [Fact]
    public void SessionService_ExposesNetworkSessionManager()
    {
        var container = new Container();
        container.RegisterInstance(new PacketRegistry());

        container.AddMoongateNetwork();

        Assert.Same(
            container.Resolve<ISessionService>(),
            container.Resolve<INetworkSessionManager>()
        );
    }

    private static MoongateTCPClient NewClient()
    {
        return new MoongateTCPClient(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
    }

    private static PacketDispatchHandler NewDispatcher(
        IReadOnlyList<IPacketHandler<TestPacket>> handlers,
        SessionService sessions
    )
    {
        return new PacketDispatchHandler(
            new FakeServiceProvider(handlers),
            new OutgoingPacketQueue(),
            sessions
        );
    }

    private sealed class CapturingHandler : IPacketHandler<TestPacket>
    {
        public List<long> SessionIds { get; } = [];

        public Task HandleAsync(PacketContext<TestPacket> context, CancellationToken cancellationToken = default)
        {
            SessionIds.Add(context.SessionId);

            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingHandler : IPacketHandler<TestPacket>
    {
        public Task HandleAsync(PacketContext<TestPacket> context, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("handler failed");
        }
    }

    private sealed class SessionCapturingHandler : IPacketHandler<TestPacket>
    {
        public IGameSession? Session { get; private set; }

        public Task HandleAsync(PacketContext<TestPacket> context, CancellationToken cancellationToken = default)
        {
            Session = context.Session;

            return Task.CompletedTask;
        }
    }

    private sealed class FakeServiceProvider : IServiceProvider
    {
        private readonly IReadOnlyList<IPacketHandler<TestPacket>> _handlers;

        public FakeServiceProvider(IReadOnlyList<IPacketHandler<TestPacket>> handlers)
        {
            _handlers = handlers;
        }

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IEnumerable<IPacketHandler<TestPacket>>))
            {
                return _handlers;
            }

            return null;
        }
    }

    private sealed class IntegrationCapture
    {
        public List<long> SessionIds { get; } = [];
    }

    private sealed class BaseHandlerProbe
    {
        public IEventBusService? EventBus { get; set; }
        public INetworkSessionManager? Sessions { get; set; }
    }

    private sealed class BaseHandler : PacketHandlerBase<TestPacket>
    {
        private readonly BaseHandlerProbe _probe;

        public BaseHandler(
            IEventBusService eventBus,
            INetworkSessionManager sessions,
            IPlayerSessionService playerSessions,
            BaseHandlerProbe probe
        )
            : base(eventBus, sessions, playerSessions)
        {
            _probe = probe;
        }

        public override Task HandleAsync(PacketContext<TestPacket> context, CancellationToken cancellationToken = default)
        {
            _probe.EventBus = EventBus;
            _probe.Sessions = Sessions;

            return Task.CompletedTask;
        }
    }

    private sealed class IntegrationHandler : IPacketHandler<TestPacket>
    {
        private readonly IntegrationCapture _capture;

        public IntegrationHandler(IntegrationCapture capture)
        {
            _capture = capture;
        }

        public Task HandleAsync(PacketContext<TestPacket> context, CancellationToken cancellationToken = default)
        {
            _capture.SessionIds.Add(context.SessionId);

            return Task.CompletedTask;
        }
    }

    private sealed class TestPacket : BaseGameNetworkPacket
    {
        public TestPacket(byte opCode)
            : base(opCode, 1)
        {
        }

        public override void Write(ref SpanWriter writer)
        {
            writer.Write(OpCode);
        }

        protected override bool ParsePayload(ref SpanReader reader)
        {
            return true;
        }
    }

    private sealed class OtherPacket : BaseGameNetworkPacket
    {
        public OtherPacket()
            : base(0xA2, 1)
        {
        }

        public override void Write(ref SpanWriter writer)
        {
            writer.Write(OpCode);
        }

        protected override bool ParsePayload(ref SpanReader reader)
        {
            return true;
        }
    }
}

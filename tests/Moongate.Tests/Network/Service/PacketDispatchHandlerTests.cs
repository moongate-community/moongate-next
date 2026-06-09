using DryIoc;
using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Abstractions.Interfaces.EventHandlers;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Network;
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
            => throw new InvalidOperationException("handler failed");
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
            : base(opCode, 1) { }

        public override void Write(ref SpanWriter writer)
            => writer.Write(OpCode);

        protected override bool ParsePayload(ref SpanReader reader)
            => true;
    }

    private sealed class OtherPacket : BaseGameNetworkPacket
    {
        public OtherPacket()
            : base(0xA2, 1) { }

        public override void Write(ref SpanWriter writer)
            => writer.Write(OpCode);

        protected override bool ParsePayload(ref SpanReader reader)
            => true;
    }

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

        bus.Publish(new PacketReceivedEvent(10, 0xA1, new TestPacket(0xA1), DateTimeOffset.UtcNow));
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

        bus.Publish(new PacketReceivedEvent(10, 0xA1, new TestPacket(0xA1), DateTimeOffset.UtcNow));
        var processed = bus.DrainTickEvents(10);

        Assert.Equal(1, processed);
        Assert.Equal(new long[] { 10 }, container.Resolve<IntegrationCapture>().SessionIds);
    }

    [Fact]
    public void Handle_HandlerThrows_ContinuesWithRemainingHandlers()
    {
        var capturing = new CapturingHandler();
        var dispatcher = NewDispatcher([new ThrowingHandler(), capturing]);

        dispatcher.Handle(new(10, 0xA1, new TestPacket(0xA1), DateTimeOffset.UtcNow));

        Assert.Equal(new long[] { 10 }, capturing.SessionIds);
    }

    [Fact]
    public void Handle_MatchingPacket_InvokesTypedHandler()
    {
        var handler = new CapturingHandler();
        var dispatcher = NewDispatcher([handler]);

        dispatcher.Handle(new(10, 0xA1, new TestPacket(0xA1), DateTimeOffset.UtcNow));

        Assert.Equal(new long[] { 10 }, handler.SessionIds);
    }

    [Fact]
    public void Handle_MultipleMatchingHandlers_InvokesAll()
    {
        var first = new CapturingHandler();
        var second = new CapturingHandler();
        var dispatcher = NewDispatcher([first, second]);

        dispatcher.Handle(new(10, 0xA1, new TestPacket(0xA1), DateTimeOffset.UtcNow));

        Assert.Equal(new long[] { 10 }, first.SessionIds);
        Assert.Equal(new long[] { 10 }, second.SessionIds);
    }

    [Fact]
    public void Handle_NonMatchingPacket_DoesNotInvokeUnrelatedHandler()
    {
        var handler = new CapturingHandler();
        var dispatcher = NewDispatcher([handler]);

        dispatcher.Handle(new(10, 0xA2, new OtherPacket(), DateTimeOffset.UtcNow));

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

    private static PacketDispatchHandler NewDispatcher(IReadOnlyList<IPacketHandler<TestPacket>> handlers)
        => new(
            new FakeServiceProvider(handlers),
            new OutgoingPacketQueue(),
            new SessionService()
        );
}

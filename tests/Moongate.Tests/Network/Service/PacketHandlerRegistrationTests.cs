using DryIoc;
using Moongate.Abstractions.Attributes;
using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Network.Spans;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Packets.Incoming.Speech;
using Moongate.Server.Services.Commands;

namespace Moongate.Tests.Network.Service;

public class PacketHandlerRegistrationTests
{
    private sealed class RegPacketA : BaseGameNetworkPacket
    {
        public RegPacketA()
            : base(0xB1, 1) { }

        public override void Write(ref SpanWriter writer)
            => writer.Write(OpCode);

        protected override bool ParsePayload(ref SpanReader reader)
            => true;
    }

    private sealed class RegPacketB : BaseGameNetworkPacket
    {
        public RegPacketB()
            : base(0xB2, 1) { }

        public override void Write(ref SpanWriter writer)
            => writer.Write(OpCode);

        protected override bool ParsePayload(ref SpanReader reader)
            => true;
    }

    [RegisterPacketHandler]
    private sealed class MarkedHandlerA : IPacketHandler<RegPacketA>
    {
        public Task HandleAsync(PacketContext<RegPacketA> context, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class UnmarkedHandlerA : IPacketHandler<RegPacketA>
    {
        public Task HandleAsync(PacketContext<RegPacketA> context, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    [RegisterPacketHandler]
    private sealed class SecondHandlerA : IPacketHandler<RegPacketA>
    {
        public Task HandleAsync(PacketContext<RegPacketA> context, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    [RegisterPacketHandler]
    private sealed class MultiHandler : IPacketHandler<RegPacketA>, IPacketHandler<RegPacketB>
    {
        public Task HandleAsync(PacketContext<RegPacketA> context, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task HandleAsync(PacketContext<RegPacketB> context, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    [RegisterPacketHandler]
    private sealed class MarkedNoInterfaceHandler { }

    [Fact]
    public void AddPacketHandlers_MarkedHandler_RegistersIt()
    {
        var container = new Container();

        container.AddPacketHandlers([typeof(MarkedHandlerA)]);

        var handlers = container.ResolveMany<IPacketHandler<RegPacketA>>().ToArray();
        Assert.Contains(handlers, static h => h.GetType() == typeof(MarkedHandlerA));
    }

    [Fact]
    public void AddPacketHandlers_MarkedWithoutInterface_Throws()
    {
        var container = new Container();

        Assert.Throws<InvalidOperationException>(() => container.AddPacketHandlers([typeof(MarkedNoInterfaceHandler)]));
    }

    [Fact]
    public void AddPacketHandlers_MultiInterfaceHandler_RegistersForAllAsSameSingleton()
    {
        var container = new Container();

        container.AddPacketHandlers([typeof(MultiHandler)]);

        var viaA = container.ResolveMany<IPacketHandler<RegPacketA>>().Single(static h => h is MultiHandler);
        var viaB = container.ResolveMany<IPacketHandler<RegPacketB>>().Single(static h => h is MultiHandler);
        Assert.Same(viaA, viaB);
    }

    [Fact]
    public void AddPacketHandlers_TwoHandlersSamePacket_BothRegistered()
    {
        var container = new Container();

        container.AddPacketHandlers([typeof(MarkedHandlerA), typeof(SecondHandlerA)]);

        var types = container.ResolveMany<IPacketHandler<RegPacketA>>().Select(static h => h.GetType()).ToArray();
        Assert.Contains(typeof(MarkedHandlerA), types);
        Assert.Contains(typeof(SecondHandlerA), types);
    }

    [Fact]
    public void AddPacketHandlers_UnmarkedHandler_IsSkipped()
    {
        var container = new Container();

        container.AddPacketHandlers([typeof(UnmarkedHandlerA)]);

        Assert.Empty(container.ResolveMany<IPacketHandler<RegPacketA>>());
    }

    [Fact]
    public void AddPacketHandlersFromAssembly_ServerAssembly_RegistersSpeechHandler()
    {
        var container = new Container();

        container.AddPacketHandlersFromAssembly(typeof(SpeechCommandPacketHandler).Assembly);

        Assert.True(container.IsRegistered<IPacketHandler<UnicodeSpeechPacket>>());
        Assert.True(container.IsRegistered<IPacketHandler<TalkRequestPacket>>());
    }
}

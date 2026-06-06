using DryIoc;
using Moongate.Abstractions.Interfaces.Commands;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Network.UO.Packets.Incoming.Speech;
using Moongate.Network.UO.Registry;
using Moongate.Server.Extensions.Commands;
using Moongate.Server.Extensions.Network;
using Moongate.Server.Interfaces.Commands;
using Moongate.Server.Services.Commands;

namespace Moongate.Tests.Server.Commands;

public sealed class CommandContainerExtensionsTests
{
    [Fact]
    public void AddMoongateCommands_RegistersRegistryBuiltinsAndSpeechHandlers()
    {
        var container = new Container();
        container.RegisterInstance(new PacketRegistry());
        container.AddMoongateNetwork();

        container.AddMoongateCommands();

        var registry = container.Resolve<ICommandRegistry>();
        Assert.True(registry.TryGetCommand("help", out _));
        Assert.True(registry.TryGetCommand("exit", out _));
        Assert.NotNull(container.Resolve<ICommandSystemService>());
        Assert.NotNull(container.Resolve<IConsoleCommandService>());
        Assert.Contains(
            container.ResolveMany<IPacketHandler<UnicodeSpeechPacket>>(),
            static handler => handler.GetType() == typeof(SpeechCommandPacketHandler)
        );
        Assert.Contains(
            container.ResolveMany<IPacketHandler<TalkRequestPacket>>(),
            static handler => handler.GetType() == typeof(SpeechCommandPacketHandler)
        );
    }
}

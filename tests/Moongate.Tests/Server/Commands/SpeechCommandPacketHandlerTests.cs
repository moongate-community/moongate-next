using Moongate.Abstractions.Data.Commands;
using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Data.Player;
using Moongate.Abstractions.Interfaces.Commands;
using Moongate.Abstractions.Types.Commands;
using Moongate.Network.UO.Packets.Incoming.Speech;
using Moongate.Server.Services.Commands;
using Moongate.Server.Services.Player;
using Moongate.Tests.Support;

namespace Moongate.Tests.Server.Commands;

public sealed class SpeechCommandPacketHandlerTests
{
    private sealed class CapturingCommandSystemService : ICommandSystemService
    {
        public string? CommandText { get; private set; }
        public CommandSourceType? Source { get; private set; }
        public long? SessionId { get; private set; }
        public PlayerSession? PlayerSession { get; private set; }

        public Task ExecuteCommandAsync(
            string commandWithArgs,
            CommandSourceType source = CommandSourceType.Console,
            long? sessionId = null,
            PlayerSession? playerSession = null,
            CancellationToken cancellationToken = default
        )
        {
            CommandText = commandWithArgs;
            Source = source;
            SessionId = sessionId;
            PlayerSession = playerSession;

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<string>> ExecuteCommandWithOutputAsync(
            string commandWithArgs,
            CommandSourceType source = CommandSourceType.Console,
            long? sessionId = null,
            PlayerSession? playerSession = null,
            CancellationToken cancellationToken = default
        )
            => Task.FromResult<IReadOnlyList<string>>([]);

        public IReadOnlyList<string> GetAutocompleteSuggestions(string commandWithArgs)
            => [];

        public IReadOnlyList<CommandDefinition> GetRegisteredCommands()
            => [];

        public Task StartAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    [Fact]
    public async Task HandleAsync_NormalSpeech_DoesNotDispatchCommand()
    {
        var commands = new CapturingCommandSystemService();
        var handler = new SpeechCommandPacketHandler(commands, new PlayerSessionService());
        var context = CreateUnicodeContext("hello");

        await handler.HandleAsync(context);

        Assert.Null(commands.CommandText);
    }

    [Fact]
    public async Task HandleAsync_SpeechCommand_DispatchesInGameCommandWithSession()
    {
        var commands = new CapturingCommandSystemService();
        var playerSessions = new PlayerSessionService();
        playerSessions.GetOrCreateConnected(42, "127.0.0.1:2593", DateTimeOffset.UtcNow);
        var handler = new SpeechCommandPacketHandler(commands, playerSessions);
        var context = CreateUnicodeContext(".echo hi");

        await handler.HandleAsync(context);

        Assert.Equal("echo hi", commands.CommandText);
        Assert.Equal(CommandSourceType.InGame, commands.Source);
        Assert.Equal(42, commands.SessionId);
        Assert.NotNull(commands.PlayerSession);
    }

    private static PacketContext<UnicodeSpeechPacket> CreateUnicodeContext(string text)
        => new(
            new FakeGameSession { SessionId = 42 },
            new() { Text = text },
            DateTimeOffset.UtcNow,
            static (_, _, _) => Task.CompletedTask,
            static () => [42]
        );
}

using Moongate.Abstractions.Data.Commands;
using Moongate.Abstractions.Data.Player;
using Moongate.Abstractions.Interfaces.Commands;
using Moongate.Abstractions.Types.Commands;
using Moongate.Server.Data.LiveConsole;
using Moongate.Server.Hubs;
using Moongate.Server.Interfaces.LiveConsole;
using Moongate.Server.Types.LiveConsole;

namespace Moongate.Tests.Server.LiveConsole;

public class LiveConsoleHubTests
{
    [Fact]
    public async Task ExecuteCommand_BlankInput_PublishesNothing()
    {
        var broadcaster = new RecordingBroadcaster();
        var commands = new FakeCommandSystem(Array.Empty<string>());
        var hub = new LiveConsoleHub(broadcaster, commands);

        await hub.ExecuteCommand("   ");

        Assert.Empty(broadcaster.Published);
        Assert.Null(commands.LastCommand);
    }

    [Fact]
    public async Task ExecuteCommand_EchoesThenStreamsOutputAsConsoleSource()
    {
        var broadcaster = new RecordingBroadcaster();
        var commands = new FakeCommandSystem(new[] { "admin (Administrator)", "bob (Player)" });
        var hub = new LiveConsoleHub(broadcaster, commands);

        await hub.ExecuteCommand("users list");

        Assert.Equal("users list", commands.LastCommand);
        Assert.Equal(CommandSourceType.Console, commands.LastSource);
        Assert.Equal(3, broadcaster.Published.Count);
        Assert.Equal(LiveConsoleEntryKind.CommandEcho, broadcaster.Published[0].Kind);
        Assert.Equal("> users list", broadcaster.Published[0].Message);
        Assert.Equal(LiveConsoleEntryKind.CommandOutput, broadcaster.Published[1].Kind);
        Assert.Equal("admin (Administrator)", broadcaster.Published[1].Message);
        Assert.Equal(LiveConsoleEntryKind.CommandOutput, broadcaster.Published[2].Kind);
        Assert.Equal("bob (Player)", broadcaster.Published[2].Message);
    }

    private sealed class RecordingBroadcaster : ILiveConsoleBroadcaster
    {
        public List<LiveConsoleEntry> Published { get; } = new();

        public event Action<LiveConsoleEntry>? EntryPublished;

        public IReadOnlyList<LiveConsoleEntry> GetBacklog()
        {
            return Published.ToList();
        }

        public void Publish(LiveConsoleEntry entry)
        {
            Published.Add(entry);
            EntryPublished?.Invoke(entry);
        }
    }

    private sealed class FakeCommandSystem : ICommandSystemService
    {
        private readonly IReadOnlyList<string> _output;

        public FakeCommandSystem(IReadOnlyList<string> output)
        {
            _output = output;
        }

        public string? LastCommand { get; private set; }

        public CommandSourceType? LastSource { get; private set; }

        public Task ExecuteCommandAsync(
            string commandWithArgs,
            CommandSourceType source = CommandSourceType.Console,
            long? sessionId = null,
            PlayerSession? playerSession = null,
            CancellationToken cancellationToken = default
        )
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<string>> ExecuteCommandWithOutputAsync(
            string commandWithArgs,
            CommandSourceType source = CommandSourceType.Console,
            long? sessionId = null,
            PlayerSession? playerSession = null,
            CancellationToken cancellationToken = default
        )
        {
            LastCommand = commandWithArgs;
            LastSource = source;

            return Task.FromResult(_output);
        }

        public IReadOnlyList<string> GetAutocompleteSuggestions(string commandWithArgs)
        {
            throw new NotSupportedException();
        }

        public IReadOnlyList<CommandDefinition> GetRegisteredCommands()
        {
            throw new NotSupportedException();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

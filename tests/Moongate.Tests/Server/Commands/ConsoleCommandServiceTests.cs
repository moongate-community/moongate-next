using Moongate.Abstractions.Data.Commands;
using Moongate.Abstractions.Data.Player;
using Moongate.Abstractions.Interfaces.Commands;
using Moongate.Abstractions.Types.Commands;
using Moongate.Server.Services.Commands;
using Moongate.Tests.Support;

namespace Moongate.Tests.Server.Commands;

public sealed class ConsoleCommandServiceTests
{
    private sealed class CapturingCommandSystemService : ICommandSystemService
    {
        private readonly TaskCompletionSource<string> _executedCommand = new(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        public Task<string> ExecutedCommand => _executedCommand.Task;

        public Task ExecuteCommandAsync(
            string commandWithArgs,
            CommandSourceType source = CommandSourceType.Console,
            long? sessionId = null,
            PlayerSession? playerSession = null,
            CancellationToken cancellationToken = default
        )
        {
            _executedCommand.TrySetResult(commandWithArgs);

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

    [Theory, InlineData("exit"), InlineData("exit now"), InlineData(" stop"), InlineData("QUIT")]
    public void IsLoopTerminatingCommand_ExitAliases_ReturnsTrue(string line)
        => Assert.True(ConsoleCommandService.IsLoopTerminatingCommand(line));

    [Theory, InlineData(""), InlineData(" "), InlineData("help"), InlineData("exitdoor"), InlineData(".exit")]
    public void IsLoopTerminatingCommand_OtherInput_ReturnsFalse(string line)
        => Assert.False(ConsoleCommandService.IsLoopTerminatingCommand(line));

    [Fact]
    public void Prompt_UsesMoongatePrefix()
        => Assert.Equal("MG> ", ConsoleCommandService.Prompt);

    [Fact]
    public async Task StartAsync_WaitsForApplicationStartedBeforePromptingAndReading()
    {
        var originalIn = Console.In;
        var originalOut = Console.Out;
        var output = new StringWriter();
        using var lifetime = new TestHostApplicationLifetime();
        var commands = new CapturingCommandSystemService();
        using var service = new ConsoleCommandService(commands, lifetime, static () => false);

        try
        {
            Console.SetIn(new StringReader("exit\n"));
            Console.SetOut(output);

            await service.StartAsync(CancellationToken.None);

            Assert.False(commands.ExecutedCommand.IsCompleted);
            Assert.DoesNotContain(ConsoleCommandService.Prompt, output.ToString(), StringComparison.Ordinal);

            lifetime.Start();

            var command = await commands.ExecutedCommand.WaitAsync(TimeSpan.FromSeconds(3));

            Assert.Equal("exit", command);
            Assert.Contains(ConsoleCommandService.Prompt, output.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            await service.StopAsync(CancellationToken.None);
            Console.SetIn(originalIn);
            Console.SetOut(originalOut);
        }
    }
}

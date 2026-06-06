using Moongate.Abstractions.Services.Commands;
using Moongate.Abstractions.Types.Commands;
using Moongate.Server.Services.Commands;

namespace Moongate.Tests.Server.Commands;

public sealed class CommandSystemServiceTests
{
    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType)
            => null;
    }

    [Fact]
    public async Task ExecuteCommandWithOutputAsync_ParsesQuotedArgumentsAndCapturesOutput()
    {
        var registry = new CommandRegistry();
        var service = new CommandSystemService(registry, new EmptyServiceProvider());
        registry.RegisterCommand(
            "say|s",
            context =>
            {
                context.Print(string.Join(",", context.Arguments));

                return Task.CompletedTask;
            },
            source: CommandSourceType.All
        );

        var output = await service.ExecuteCommandWithOutputAsync(
                         "s \"hello world\" Britannia",
                         CommandSourceType.InGame
                     );

        Assert.Equal(["hello world,Britannia"], output);
    }

    [Fact]
    public async Task ExecuteCommandWithOutputAsync_SourceNotAllowed_DoesNotInvokeHandler()
    {
        var invoked = false;
        var registry = new CommandRegistry();
        var service = new CommandSystemService(registry, new EmptyServiceProvider());
        registry.RegisterCommand(
            "stop",
            _ =>
            {
                invoked = true;

                return Task.CompletedTask;
            },
            source: CommandSourceType.Console
        );

        var output = await service.ExecuteCommandWithOutputAsync("stop", CommandSourceType.InGame);

        Assert.False(invoked);
        Assert.Contains("not available", output[0], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetAutocompleteSuggestions_UsesCommandAliasesAndProviders()
    {
        var registry = new CommandRegistry();
        var service = new CommandSystemService(registry, new EmptyServiceProvider());
        registry.RegisterCommand(
            "teleport|tp",
            static _ => Task.CompletedTask,
            source: CommandSourceType.All,
            autocompleteProvider: static context =>
                                  {
                                      if (context.Arguments.Count == 0 || context.EndsWithWhitespace)
                                      {
                                          return new[] { "britain", "moonglow" };
                                      }

                                      return new[] { "britain", "moonglow" }
                                             .Where(
                                                 value => value.StartsWith(
                                                     context.Arguments[^1],
                                                     StringComparison.OrdinalIgnoreCase
                                                 )
                                             )
                                             .ToArray();
                                  }
        );

        Assert.Equal(["teleport", "tp"], service.GetAutocompleteSuggestions("t"));
        Assert.Equal(["britain"], service.GetAutocompleteSuggestions("tp br"));
    }
}

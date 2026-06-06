using Moongate.Abstractions.Services.Commands;
using Moongate.Abstractions.Types.Commands;

namespace Moongate.Tests.Server.Commands;

public sealed class CommandRegistryTests
{
    [Fact]
    public void RegisterCommand_DuplicateAlias_Throws()
    {
        var registry = new CommandRegistry();
        registry.RegisterCommand("save", static _ => Task.CompletedTask);

        Assert.Throws<InvalidOperationException>(
            () => registry.RegisterCommand("persist|save", static _ => Task.CompletedTask)
        );
    }

    [Fact]
    public void RegisterCommand_WithAliases_ResolvesEveryAliasAsOneDefinition()
    {
        var registry = new CommandRegistry();

        registry.RegisterCommand(
            "save|persist",
            static _ => Task.CompletedTask,
            "Saves state.",
            CommandSourceType.All
        );

        Assert.True(registry.TryGetCommand("save", out var primary));
        Assert.True(registry.TryGetCommand("persist", out var alias));
        Assert.Same(primary, alias);
        Assert.Equal(["save", "persist"], primary.Aliases);
        Assert.Equal(CommandSourceType.All, primary.Source);
        Assert.Single(registry.GetRegisteredCommands());
    }
}

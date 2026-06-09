using DryIoc;
using Moongate.Abstractions.Interfaces.Commands;
using Moongate.Abstractions.Services.Commands;
using Moongate.Server.Commands;
using Moongate.Server.Extensions.Hosting;
using Moongate.Server.Interfaces.Commands;
using Moongate.Server.Services.Commands;

namespace Moongate.Server.Extensions.Commands;

/// <summary>
/// DryIoc-native registration helpers for server commands.
/// </summary>
public static class CommandContainerExtensions
{
    private const int CommandSystemPriority = 25;
    private const int ConsoleCommandPriority = 35;

    /// <summary>
    /// Registers command registry, command dispatch, built-in commands, console input, and in-game speech hooks.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    public static IContainer AddMoongateCommands(this IContainer container)
    {
        container.AddMoongateHosting();

        container.Register<ICommandRegistry, CommandRegistry>(
            Reuse.Singleton,
            ifAlreadyRegistered: IfAlreadyRegistered.Keep
        );
        container.AddMoongateService<ICommandSystemService, CommandSystemService>(CommandSystemPriority);
        container.AddMoongateService<IConsoleCommandService, ConsoleCommandService>(ConsoleCommandPriority);

        BuiltinCommandRegistration.Register(container.Resolve<ICommandRegistry>());

        return container;
    }
}

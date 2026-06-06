using Moongate.Abstractions.Data.Commands;
using Moongate.Abstractions.Interfaces.Commands;
using Moongate.Abstractions.Types.Commands;

namespace Moongate.Server.Commands;

internal static class BuiltinCommandRegistration
{
    public static void Register(ICommandRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        RegisterHelp(registry);
        RegisterEcho(registry);
        RegisterExit(registry);
    }

    private static string GetCurrentToken(CommandAutocompleteContext context)
    {
        if (context.EndsWithWhitespace || context.Arguments.Count == 0)
        {
            return "";
        }

        return context.Arguments[^1];
    }

    private static void PrintCommandHelp(ICommandRegistry registry, CommandSystemContext context, string commandName)
    {
        if (!registry.TryGetCommand(commandName, out var command))
        {
            context.PrintWarning("Command '{0}' is not registered.", commandName);

            return;
        }

        if ((command.Source & context.Source) == 0)
        {
            context.PrintWarning("Command '{0}' is not available from {1}.", command.Name, context.Source);

            return;
        }

        var description = string.IsNullOrWhiteSpace(command.Description) ? "No description." : command.Description;
        var aliases = command.Aliases.Count <= 1 ? "" : $" Aliases: {string.Join(", ", command.Aliases.Skip(1))}.";

        context.Print("{0}: {1}{2}", command.Name, description, aliases);
    }

    private static void RegisterEcho(ICommandRegistry registry)
    {
        if (registry.TryGetCommand("echo", out _))
        {
            return;
        }

        registry.RegisterCommand(
            "echo",
            context =>
            {
                context.Print(string.Join(" ", context.Arguments));

                return Task.CompletedTask;
            },
            "Prints command arguments.",
            CommandSourceType.All
        );
    }

    private static void RegisterExit(ICommandRegistry registry)
    {
        if (registry.TryGetCommand("exit", out _))
        {
            return;
        }

        registry.RegisterCommand(
            "exit|stop|quit",
            context =>
            {
                var lifetime = context.Services.GetService(typeof(IHostApplicationLifetime)) as IHostApplicationLifetime;

                if (lifetime is null)
                {
                    context.PrintWarning("Host lifetime service is not available.");

                    return Task.CompletedTask;
                }

                context.Print("Stopping server.");
                lifetime.StopApplication();

                return Task.CompletedTask;
            },
            "Stops the server."
        );
    }

    private static void RegisterHelp(ICommandRegistry registry)
    {
        if (registry.TryGetCommand("help", out _))
        {
            return;
        }

        registry.RegisterCommand(
            "help|?",
            context =>
            {
                if (context.Arguments.Count > 0)
                {
                    PrintCommandHelp(registry, context, context.Arguments[0]);

                    return Task.CompletedTask;
                }

                var commands = registry.GetRegisteredCommands()
                                       .Where(command => (command.Source & context.Source) != 0)
                                       .ToArray();

                if (commands.Length == 0)
                {
                    context.Print("No commands are registered for {0}.", context.Source);

                    return Task.CompletedTask;
                }

                context.Print(
                    "Commands: {0}",
                    string.Join(", ", commands.Select(static command => command.Name))
                );

                return Task.CompletedTask;
            },
            "Shows registered commands.",
            CommandSourceType.All,
            context => registry.GetRegisteredCommands()
                               .SelectMany(static command => command.Aliases)
                               .Where(
                                   alias => alias.StartsWith(GetCurrentToken(context), StringComparison.OrdinalIgnoreCase)
                               )
                               .Order(StringComparer.OrdinalIgnoreCase)
                               .ToArray()
        );
    }
}

using Moongate.Abstractions.Data.Commands;
using Moongate.Abstractions.Interfaces.Commands;
using Moongate.Abstractions.Types.Commands;

namespace Moongate.Abstractions.Services.Commands;

/// <summary>
/// Thread-safe command registry shared by built-in server code and trusted plugins.
/// </summary>
public sealed class CommandRegistry : ICommandRegistry
{
    private readonly Lock _sync = new();
    private readonly Dictionary<string, CommandDefinition> _commands = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<CommandDefinition> GetRegisteredCommands()
    {
        lock (_sync)
        {
            return _commands
                   .Values
                   .Distinct()
                   .OrderBy(static command => command.Name, StringComparer.OrdinalIgnoreCase)
                   .ToArray();
        }
    }

    public void RegisterCommand(
        string commandName,
        Func<CommandSystemContext, Task> handler,
        string description = "",
        CommandSourceType source = CommandSourceType.Console,
        Func<CommandAutocompleteContext, IReadOnlyList<string>>? autocompleteProvider = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandName);
        ArgumentNullException.ThrowIfNull(handler);

        var aliases = commandName
                      .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                      .Select(static alias => alias.ToLowerInvariant())
                      .Distinct(StringComparer.OrdinalIgnoreCase)
                      .ToArray();

        if (aliases.Length == 0)
        {
            throw new ArgumentException("Command name is required.", nameof(commandName));
        }

        var definition = new CommandDefinition
        {
            Name = aliases[0],
            Aliases = aliases,
            Description = description,
            Source = source,
            Handler = handler,
            AutocompleteProvider = autocompleteProvider
        };

        lock (_sync)
        {
            foreach (var alias in aliases)
            {
                if (_commands.ContainsKey(alias))
                {
                    throw new InvalidOperationException($"Command '{alias}' is already registered.");
                }
            }

            foreach (var alias in aliases)
            {
                _commands[alias] = definition;
            }
        }
    }

    public bool TryGetCommand(string commandName, out CommandDefinition definition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandName);

        lock (_sync)
        {
            return _commands.TryGetValue(commandName, out definition!);
        }
    }
}

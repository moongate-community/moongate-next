using Moongate.Abstractions.Data.Commands;
using Moongate.Abstractions.Types.Commands;

namespace Moongate.Abstractions.Interfaces.Commands;

/// <summary>
/// Registers command handlers from built-in server code and trusted plugins.
/// </summary>
public interface ICommandRegistry
{
    /// <summary>
    /// Gets all registered command definitions.
    /// </summary>
    /// <returns>Registered commands ordered by primary name.</returns>
    IReadOnlyList<CommandDefinition> GetRegisteredCommands();

    /// <summary>
    /// Registers one command or multiple aliases separated by <c>|</c>.
    /// </summary>
    /// <param name="commandName">Primary command name or alias list.</param>
    /// <param name="handler">Command handler.</param>
    /// <param name="description">Help description.</param>
    /// <param name="source">Allowed command sources.</param>
    /// <param name="autocompleteProvider">Optional autocomplete provider.</param>
    void RegisterCommand(
        string commandName,
        Func<CommandSystemContext, Task> handler,
        string description = "",
        CommandSourceType source = CommandSourceType.Console,
        Func<CommandAutocompleteContext, IReadOnlyList<string>>? autocompleteProvider = null
    );

    /// <summary>
    /// Attempts to resolve a command by primary name or alias.
    /// </summary>
    /// <param name="commandName">Command name or alias.</param>
    /// <param name="definition">Matching command definition.</param>
    /// <returns><c>true</c> when a command was found.</returns>
    bool TryGetCommand(string commandName, out CommandDefinition definition);
}

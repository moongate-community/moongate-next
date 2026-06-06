using Moongate.Abstractions.Data.Commands;
using Moongate.Abstractions.Data.Player;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Types.Commands;

namespace Moongate.Abstractions.Interfaces.Commands;

/// <summary>
/// Dispatches operator commands from console, in-game speech, plugins, and HTTP endpoints.
/// </summary>
public interface ICommandSystemService : IMoongateService
{
    /// <summary>
    /// Executes a raw command text.
    /// </summary>
    /// <param name="commandWithArgs">Raw command text including arguments.</param>
    /// <param name="source">Command source.</param>
    /// <param name="sessionId">Optional network session id for in-game commands.</param>
    /// <param name="playerSession">Optional logical player session for in-game commands.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ExecuteCommandAsync(
        string commandWithArgs,
        CommandSourceType source = CommandSourceType.Console,
        long? sessionId = null,
        PlayerSession? playerSession = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Executes a command and returns output produced through the command context.
    /// </summary>
    /// <param name="commandWithArgs">Raw command text including arguments.</param>
    /// <param name="source">Command source.</param>
    /// <param name="sessionId">Optional network session id for in-game commands.</param>
    /// <param name="playerSession">Optional logical player session for in-game commands.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Captured command output lines.</returns>
    Task<IReadOnlyList<string>> ExecuteCommandWithOutputAsync(
        string commandWithArgs,
        CommandSourceType source = CommandSourceType.Console,
        long? sessionId = null,
        PlayerSession? playerSession = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets autocomplete suggestions for the current command line.
    /// </summary>
    /// <param name="commandWithArgs">Current command line.</param>
    /// <returns>Autocomplete suggestions.</returns>
    IReadOnlyList<string> GetAutocompleteSuggestions(string commandWithArgs);

    /// <summary>
    /// Gets registered command definitions.
    /// </summary>
    /// <returns>Registered command definitions.</returns>
    IReadOnlyList<CommandDefinition> GetRegisteredCommands();
}

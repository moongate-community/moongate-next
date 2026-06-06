using Moongate.Abstractions.Types.Commands;

namespace Moongate.Abstractions.Data.Commands;

/// <summary>
/// Registered command metadata and handler.
/// </summary>
public sealed class CommandDefinition
{
    /// <summary>
    /// Primary command name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// All aliases that resolve to this command, including the primary name.
    /// </summary>
    public required IReadOnlyList<string> Aliases { get; init; }

    /// <summary>
    /// Human-readable command description.
    /// </summary>
    public string Description { get; init; } = "";

    /// <summary>
    /// Sources allowed to execute the command.
    /// </summary>
    public CommandSourceType Source { get; init; } = CommandSourceType.Console;

    /// <summary>
    /// Command handler.
    /// </summary>
    public required Func<CommandSystemContext, Task> Handler { get; init; }

    /// <summary>
    /// Optional autocomplete provider.
    /// </summary>
    public Func<CommandAutocompleteContext, IReadOnlyList<string>>? AutocompleteProvider { get; init; }
}

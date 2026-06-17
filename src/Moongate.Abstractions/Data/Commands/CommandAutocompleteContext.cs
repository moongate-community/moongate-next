namespace Moongate.Abstractions.Data.Commands;

/// <summary>
///     Context passed to command autocomplete providers.
/// </summary>
public sealed class CommandAutocompleteContext
{
    /// <summary>
    ///     Command being completed.
    /// </summary>
    public string CommandName { get; init; } = "";

    /// <summary>
    ///     Already parsed argument tokens.
    /// </summary>
    public IReadOnlyList<string> Arguments { get; init; } = [];

    /// <summary>
    ///     Whether the original input ends with whitespace.
    /// </summary>
    public bool EndsWithWhitespace { get; init; }
}

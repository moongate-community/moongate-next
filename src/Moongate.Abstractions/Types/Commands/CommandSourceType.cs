namespace Moongate.Abstractions.Types.Commands;

/// <summary>
///     Sources allowed to execute registered commands.
/// </summary>
[Flags]
public enum CommandSourceType
{
    None = 0,
    Console = 1,
    InGame = 2,
    All = Console | InGame
}

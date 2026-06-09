namespace Moongate.Server.Types.LiveConsole;

/// <summary>
/// Classifies a <c>LiveConsoleEntry</c> so the client can style it: server log line,
/// the echo of a command an admin typed, or a line of that command's output.
/// </summary>
public enum LiveConsoleEntryKind
{
    Log,
    CommandEcho,
    CommandOutput
}

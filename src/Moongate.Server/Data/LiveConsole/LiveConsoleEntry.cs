using Moongate.Server.Types.LiveConsole;

namespace Moongate.Server.Data.LiveConsole;

/// <summary>
/// A single line shown in the live admin console, sent over SignalR to connected admins.
/// </summary>
public sealed record LiveConsoleEntry
{
    /// <summary>What this line represents (log line, command echo, or command output).</summary>
    public required LiveConsoleEntryKind Kind { get; init; }

    /// <summary>Serilog level name for <see cref="LiveConsoleEntryKind.Log" /> entries; null otherwise.</summary>
    public string? Level { get; init; }

    /// <summary>Creation time in Unix milliseconds (UTC).</summary>
    public required long Timestamp { get; init; }

    /// <summary>The rendered text of the line.</summary>
    public required string Message { get; init; }
}

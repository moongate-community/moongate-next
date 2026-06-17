using Moongate.Server.Data.LiveConsole;

namespace Moongate.Server.Interfaces.LiveConsole;

/// <summary>
///     In-memory fan-out for the live admin console. Holds a bounded backlog of recent entries and
///     raises an event for every new entry, decoupling Serilog and command execution from SignalR.
/// </summary>
public interface ILiveConsoleBroadcaster
{
    /// <summary>Raised once for every published entry (after it is appended to the backlog).</summary>
    event Action<LiveConsoleEntry> EntryPublished;

    /// <summary>Returns a snapshot of the recent backlog (oldest first), capped to the buffer size.</summary>
    IReadOnlyList<LiveConsoleEntry> GetBacklog();

    /// <summary>Appends an entry to the backlog and notifies subscribers. Never throws.</summary>
    void Publish(LiveConsoleEntry entry);
}

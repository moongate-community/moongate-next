using Moongate.Server.Data.LiveConsole;
using Moongate.Server.Interfaces.LiveConsole;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.LiveConsole;

/// <summary>
///     Default <see cref="ILiveConsoleBroadcaster" />: a thread-safe ring buffer of the last
///     <see cref="BacklogCapacity" /> entries plus an event fan-out. Publishing never throws so a
///     misbehaving subscriber can never break logging or command execution.
/// </summary>
public sealed class LiveConsoleBroadcaster : ILiveConsoleBroadcaster
{
    private const int BacklogCapacity = 200;

    // Initial capacity hint only; the trim loop in Publish enforces the hard cap of BacklogCapacity.
    private readonly Queue<LiveConsoleEntry> _backlog = new(BacklogCapacity);
    private readonly object _lock = new();

    private readonly ILogger _logger = Log.ForContext<LiveConsoleBroadcaster>();

    public event Action<LiveConsoleEntry>? EntryPublished;

    public IReadOnlyList<LiveConsoleEntry> GetBacklog()
    {
        lock (_lock)
        {
            return _backlog.ToArray();
        }
    }

    public void Publish(LiveConsoleEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        lock (_lock)
        {
            _backlog.Enqueue(entry);

            while (_backlog.Count > BacklogCapacity)
            {
                _backlog.Dequeue();
            }
        }

        // Raise outside the lock so subscribers (e.g. the SignalR relay) can't deadlock the publisher.
        try
        {
            EntryPublished?.Invoke(entry);
        }
        catch (Exception ex)
        {
            // A faulty subscriber must not break the logging / command path.
            _logger.Error(ex, "Live console subscriber threw while handling a published entry");
        }
    }
}

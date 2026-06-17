using Moongate.Server.Data.LiveConsole;
using Moongate.Server.Interfaces.LiveConsole;
using Moongate.Server.Types.LiveConsole;
using Serilog.Core;
using Serilog.Events;

namespace Moongate.Server.Services.Logging;

/// <summary>
///     Serilog sink that forwards each log event to the <see cref="ILiveConsoleBroadcaster" /> so it
///     streams to connected admins. Skips SignalR's own framework logs to avoid a re-logging feedback loop.
/// </summary>
public sealed class LiveConsoleSink : ILogEventSink
{
    private readonly ILiveConsoleBroadcaster _broadcaster;

    public LiveConsoleSink(ILiveConsoleBroadcaster broadcaster)
    {
        ArgumentNullException.ThrowIfNull(broadcaster);

        _broadcaster = broadcaster;
    }

    public void Emit(LogEvent logEvent)
    {
        if (logEvent is null || IsSignalRNoise(logEvent))
        {
            return;
        }

        _broadcaster.Publish(
            new LiveConsoleEntry
            {
                Kind = LiveConsoleEntryKind.Log,
                Level = logEvent.Level.ToString(),
                Timestamp = logEvent.Timestamp.ToUnixTimeMilliseconds(),
                Message = logEvent.RenderMessage()
            }
        );
    }

    private static bool IsSignalRNoise(LogEvent logEvent)
    {
        if (!logEvent.Properties.TryGetValue("SourceContext", out var value) ||
            value is not ScalarValue { Value: string source })
        {
            return false;
        }

        return source.StartsWith("Microsoft.AspNetCore.SignalR", StringComparison.Ordinal) ||
               source.StartsWith("Microsoft.AspNetCore.Http.Connections", StringComparison.Ordinal);
    }
}

using Moongate.Abstractions.Interfaces.EventHandlers;
using Moongate.Server.Data.Events;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.Diagnostics;

/// <summary>
///     Diagnostic tick handler that logs the thread it runs on. Confirms that the
///     game-loop service really dispatches tick handlers on its dedicated thread.
/// </summary>
public sealed class ServerStartedHandler : ITickEventHandler<ServerStartedEvent>
{
    private readonly ILogger _logger = Log.ForContext<ServerStartedHandler>();

    public void Handle(ServerStartedEvent evt)
    {
        var thread = Thread.CurrentThread;

        _logger.Information(
            "ServerStartedEvent handled on thread Name={ThreadName} ManagedId={ThreadId} IsThreadPool={IsPool} IsBackground={IsBackground} (publishedAt={At:O})",
            thread.Name ?? "(null)",
            thread.ManagedThreadId,
            thread.IsThreadPoolThread,
            thread.IsBackground,
            evt.At
        );
    }
}

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Moongate.Server.Data.LiveConsole;
using Moongate.Server.Hubs;
using Moongate.Server.Interfaces.LiveConsole;

namespace Moongate.Server.Services.LiveConsole;

/// <summary>
/// Bridges the <see cref="ILiveConsoleBroadcaster" /> to SignalR: subscribes on start and forwards
/// every published entry to all connected console clients via <see cref="IHubContext{THub}" />.
/// Forwarding is fire-and-forget so it never blocks the logging or command-execution thread.
/// </summary>
public sealed class LiveConsoleRelay : IHostedService
{
    private readonly ILiveConsoleBroadcaster _broadcaster;
    private readonly IHubContext<LiveConsoleHub> _hub;

    public LiveConsoleRelay(ILiveConsoleBroadcaster broadcaster, IHubContext<LiveConsoleHub> hub)
    {
        ArgumentNullException.ThrowIfNull(broadcaster);
        ArgumentNullException.ThrowIfNull(hub);

        _broadcaster = broadcaster;
        _hub = hub;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _broadcaster.EntryPublished += OnEntryPublished;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _broadcaster.EntryPublished -= OnEntryPublished;

        return Task.CompletedTask;
    }

    private void OnEntryPublished(LiveConsoleEntry entry)
        => _ = _hub.Clients.All.SendAsync("line", entry);
}

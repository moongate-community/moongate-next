using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Moongate.Abstractions.Interfaces.Commands;
using Moongate.Abstractions.Types.Commands;
using Moongate.Core.Types;
using Moongate.Server.Data.LiveConsole;
using Moongate.Server.Interfaces.LiveConsole;
using Moongate.Server.Types.LiveConsole;

namespace Moongate.Server.Hubs;

/// <summary>
/// SignalR hub for the live admin console. On connect it sends the recent backlog to the caller;
/// <see cref="ExecuteCommand" /> runs a command as <see cref="CommandSourceType.Console" /> and
/// pushes the echo + output into the shared broadcaster (seen by all connected admins).
/// </summary>
[Authorize(Roles = nameof(UserLevelType.Administrator))]
public sealed class LiveConsoleHub : Hub
{
    private readonly ILiveConsoleBroadcaster _broadcaster;
    private readonly ICommandSystemService _commandSystem;

    public LiveConsoleHub(ILiveConsoleBroadcaster broadcaster, ICommandSystemService commandSystem)
    {
        ArgumentNullException.ThrowIfNull(broadcaster);
        ArgumentNullException.ThrowIfNull(commandSystem);

        _broadcaster = broadcaster;
        _commandSystem = commandSystem;
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        await Clients.Caller.SendAsync("backlog", _broadcaster.GetBacklog());
    }

    /// <summary>
    /// Runs a command as the Console source and streams its echo + output to all admins. A command
    /// that throws lets the exception surface to the calling admin (other clients are unaffected);
    /// this is intentional — do not swallow it into a generic failure line.
    /// </summary>
    public async Task ExecuteCommand(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return;
        }

        _broadcaster.Publish(
            new LiveConsoleEntry
            {
                Kind = LiveConsoleEntryKind.CommandEcho,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Message = "> " + line
            }
        );

        var output = await _commandSystem.ExecuteCommandWithOutputAsync(line, CommandSourceType.Console);

        foreach (var outputLine in output)
        {
            _broadcaster.Publish(
                new LiveConsoleEntry
                {
                    Kind = LiveConsoleEntryKind.CommandOutput,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    Message = outputLine
                }
            );
        }
    }
}

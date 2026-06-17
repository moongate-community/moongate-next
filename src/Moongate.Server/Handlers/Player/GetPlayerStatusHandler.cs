using Moongate.Abstractions.Attributes;
using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Network.UO.Packets.Incoming.Player;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Handlers.Player;

/// <summary>
///     Consumes the Get Player Status packet (0x34) so the parser can frame it correctly.
///     Sending the actual status/skills response is intentionally out of scope (no-op for now).
/// </summary>
[RegisterPacketHandler]
public sealed class GetPlayerStatusHandler : IPacketHandler<GetPlayerStatusPacket>
{
    private readonly ILogger _logger = Log.ForContext<GetPlayerStatusHandler>();

    public Task HandleAsync(
        PacketContext<GetPlayerStatusPacket> context,
        CancellationToken cancellationToken = default
    )
    {
        _logger.Debug(
            "Get-player-status {StatusType} for {MobileSerial} on session {SessionId} (not yet answered)",
            context.Packet.StatusType,
            context.Packet.MobileSerial,
            context.SessionId
        );

        return Task.CompletedTask;
    }
}

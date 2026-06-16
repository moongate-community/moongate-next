using Moongate.Abstractions.Attributes;
using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Network;
using Moongate.Network.UO.Packets.Incoming.Player;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Handlers.System;

[RegisterPacketHandler]
public class PingHandler : PacketHandlerBase<PingMessagePacket>
{
    private readonly ILogger _logger = Log.ForContext<PingHandler>();

    public PingHandler(
        IEventBusService eventBus,
        INetworkSessionManager sessions,
        IPlayerSessionService playerSessions
    ) : base(eventBus, sessions, playerSessions) { }

    public override async Task HandleAsync(
        PacketContext<PingMessagePacket> context,
        CancellationToken cancellationToken = default
    )
    {
        if (PlayerSessions.TryGetBySessionId(context.SessionId, out var playerSession))
        {
            _logger.Information(
                "Ping from session {PlayerSessionId} sequence: {Sequence}",
                context.SessionId,
                context.Packet.Sequence
            );

            playerSession.PingSequence += 1;

            var pongPacket = new PingMessagePacket
            {
                Sequence = playerSession.PingSequence
            };

            await context.SendAsync(pongPacket, cancellationToken);
        }
    }
}

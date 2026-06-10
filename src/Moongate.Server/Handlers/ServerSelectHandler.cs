using Moongate.Abstractions.Attributes;
using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Network;
using Moongate.Network.UO.Packets.Incoming.Login;
using Moongate.Network.UO.Packets.Outgoing.Login;
using Moongate.Network.UO.Types.Login;
using Moongate.Server.Interfaces.Sessions;

namespace Moongate.Server.Handlers;

[RegisterPacketHandler]
public class ServerSelectHandler : PacketHandlerBase<ServerSelectPacket>
{
    private readonly IGameLoginHandoffService _gameLoginHandoffService;

    public ServerSelectHandler(
        IEventBusService eventBus,
        INetworkSessionManager sessions,
        IPlayerSessionService playerSessions,
        IGameLoginHandoffService gameLoginHandoffService
    ) : base(eventBus, sessions, playerSessions)
    {
        _gameLoginHandoffService = gameLoginHandoffService;
    }

    public override async Task HandleAsync(
        PacketContext<ServerSelectPacket> context,
        CancellationToken cancellationToken = default
    )
    {
        if (context.Session.ServerEndPoint is null ||
            !PlayerSessions.TryGetBySessionId(context.SessionId, out var playerSession))
        {
            return;
        }

        var sessionKey = (uint)Random.Shared.Next();

        var serverRedirectPacket = new ServerRedirectPacket()
        {
            IpAddress = context.Session.ServerEndPoint.Address,
            Port = context.Session.ServerEndPoint.Port,
            SessionKey = sessionKey
        };

        _gameLoginHandoffService.Store(sessionKey, ClientType.StygianAbyss, playerSession.ClientVersion);

        await context.SendAsync(serverRedirectPacket, cancellationToken);
    }
}

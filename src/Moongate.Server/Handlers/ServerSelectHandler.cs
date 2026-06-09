using Moongate.Abstractions.Attributes;
using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Network;
using Moongate.Network.UO.Packets.Incoming.Login;
using Moongate.Network.UO.Packets.Outgoing.Login;
using Moongate.Network.UO.Types.Login;
using Moongate.Server.Interfaces.Network;
using Moongate.Server.Interfaces.Sessions;

namespace Moongate.Server.Handlers;

[RegisterPacketHandler]
public class ServerSelectHandler : PacketHandlerBase<ServerSelectPacket>
{
    private readonly ISessionService _sessionService;

    private readonly IPlayerSessionService _playerSessionService;
    private readonly IGameLoginHandoffService _gameLoginHandoffService;

    public ServerSelectHandler(
        IEventBusService eventBus,
        INetworkSessionManager sessions,
        IPlayerSessionService playerSessions,
        ISessionService sessionService,
        IGameLoginHandoffService gameLoginHandoffService,
        IPlayerSessionService playerSessionService
    ) : base(eventBus, sessions, playerSessions)
    {
        _sessionService = sessionService;
        _gameLoginHandoffService = gameLoginHandoffService;
        _playerSessionService = playerSessionService;
    }

    public override async Task HandleAsync(
        PacketContext<ServerSelectPacket> context,
        CancellationToken cancellationToken = default
    )
    {
        if (_sessionService.TryGet(context.SessionId, out var session))
        {
            if (_playerSessionService.TryGetBySessionId(context.SessionId, out var playerSession))
            {
                var sessionKey = (uint)Random.Shared.Next();

                var serverRedirectPacket = new ServerRedirectPacket()
                {
                    IpAddress = session.ServerEndPoint.Address,
                    Port = session.ServerEndPoint.Port,
                    SessionKey = sessionKey
                };




                _gameLoginHandoffService.Store(sessionKey, ClientType.StygianAbyss, playerSession.ClientVersion);

                await context.SendAsync(serverRedirectPacket, cancellationToken);
            }
        }
    }
}

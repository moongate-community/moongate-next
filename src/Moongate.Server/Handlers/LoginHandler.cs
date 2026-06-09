using Moongate.Abstractions.Attributes;
using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Network;
using Moongate.Network.UO.Data.Login;
using Moongate.Network.UO.Packets.Incoming.Login;
using Moongate.Network.UO.Packets.Outgoing.Login;
using Moongate.Network.UO.Types.Login;
using Moongate.Server.Interfaces.Network;
using Moongate.UO.Domain.Interfaces.Services;

namespace Moongate.Server.Handlers;

[RegisterPacketHandler]
public class LoginHandler : PacketHandlerBase<LoginSeedPacket>, IPacketHandler<AccountLoginPacket>
{
    private readonly IUserService _userService;

    private readonly ISessionService _sessionService;

    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<LoginHandler>();

    public LoginHandler(
        IEventBusService eventBus,
        INetworkSessionManager sessions,
        IPlayerSessionService playerSessionService,
        IUserService userService,
        ISessionService sessionService
    ) : base(eventBus, sessions, playerSessionService)
    {
        _userService = userService;
        _sessionService = sessionService;
    }

    public override Task HandleAsync(PacketContext<LoginSeedPacket> context, CancellationToken cancellationToken = default)
    {
        if (PlayerSessions.TryGetBySessionId(context.SessionId, out var playerSession))
        {
            PlayerSessions.UpdateClient(context.SessionId, context.Packet.ClientVersion);

            _logger.Information(
                "Player session {@PlayerSession} - Version {Version}",
                playerSession.SessionId,
                context.Packet.ClientVersion
            );

            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    public async Task HandleAsync(PacketContext<AccountLoginPacket> context, CancellationToken cancellationToken = default)
    {
        if (PlayerSessions.TryGetBySessionId(context.SessionId, out var _))
        {
            if (_sessionService.TryGet(context.SessionId, out var gameSession))
            {
                var userEntity = await _userService.LoginAsync(
                                     context.Packet.Account,
                                     context.Packet.Password,
                                     cancellationToken
                                 );

                if (userEntity == null)
                {
                    _logger.Warning("Failed login attempt for account {Account}", context.Packet.Account);
                    await context.SendAsync(new LoginDeniedPacket(LoginDeniedReasonType.BadPassword), cancellationToken);

                    return;
                }

                if (!userEntity.IsActive)
                {
                    await context.SendAsync(new LoginDeniedPacket(LoginDeniedReasonType.AccountBlocked), cancellationToken);

                    return;
                }

                _logger.Information("User {@User} autheticated, level: {@Level}", userEntity.Username, userEntity.Level);

                var serverListPacket = new ServerListPacket(
                    new GameServerEntry()
                    {
                        Index = 0,
                        ServerName = "Moongate",
                        IpAddress = gameSession.ServerEndPoint.Address,
                    }
                );

                await context.SendAsync(serverListPacket, cancellationToken);
            }
        }
    }
}

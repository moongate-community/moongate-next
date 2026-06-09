using Moongate.Abstractions.Attributes;
using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Network;
using Moongate.Network.UO.Packets.Incoming.Login;
using Moongate.UO.Domain.Interfaces.Services;

namespace Moongate.Server.Handlers;

[RegisterPacketHandler]
public class LoginHandler : PacketHandlerBase<LoginSeedPacket>, IPacketHandler<AccountLoginPacket>
{
    private readonly IPlayerSessionService _playerSessionService;
    private readonly IUserService  _userService;

    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<LoginHandler>();
    public LoginHandler(
        IEventBusService eventBus,
        INetworkSessionManager sessions,
        IPlayerSessionService playerSessionService,
        IUserService userService
    ) : base(eventBus, sessions)
    {
        _playerSessionService = playerSessionService;
        _userService = userService;
    }

    public override Task HandleAsync(PacketContext<LoginSeedPacket> context, CancellationToken cancellationToken = default)
    {
        if (_playerSessionService.TryGetBySessionId(context.SessionId, out var playerSession))
        {
            // Player has already sent a seed packet for this session, ignore subsequent ones.

            playerSession.ClientVersion = context.Packet.ClientVersion;

            _logger.Information("Player session {@PlayerSession} - Version {Version}", playerSession.SessionId,  context.Packet.ClientVersion);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    public Task HandleAsync(PacketContext<AccountLoginPacket> context, CancellationToken cancellationToken = default)
    {

        return Task.CompletedTask;
    }
}

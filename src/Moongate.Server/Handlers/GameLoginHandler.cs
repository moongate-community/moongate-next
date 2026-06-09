using Moongate.Abstractions.Attributes;
using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Network;
using Moongate.Network.UO.Packets.Incoming.Login;
using Moongate.Server.Interfaces.Sessions;
using Moongate.UO.Domain.Interfaces.Services;

namespace Moongate.Server.Handlers;

[RegisterPacketHandler]
public class GameLoginHandler : PacketHandlerBase<GameLoginPacket>, IPacketHandler<LoginSeedPacket>
{
    private readonly IGameLoginHandoffService _gameLoginHandoffService;

    private readonly IUserService _userService;

    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<GameLoginHandler>();

    public GameLoginHandler(
        IEventBusService eventBus,
        INetworkSessionManager sessions,
        IPlayerSessionService playerSessions,
        IGameLoginHandoffService gameLoginHandoffService,
        IUserService userService
    ) : base(eventBus, sessions, playerSessions)
    {
        _gameLoginHandoffService = gameLoginHandoffService;
        _userService = userService;
    }

    public override async Task HandleAsync(
        PacketContext<GameLoginPacket> context,
        CancellationToken cancellationToken = default
    )
    {
        if (_gameLoginHandoffService.TryConsume(context.Packet.SessionKey, out var handoffInfo))
        {
            var userEntity = await _userService.GetByUsernameAsync(context.Packet.AccountName, cancellationToken);
            PlayerSessions.UpdateClient(context.SessionId, handoffInfo.ClientVersion);
            PlayerSessions.Authenticate(context.SessionId, userEntity.Id, userEntity.Username, DateTimeOffset.Now);
            _logger.Information(
                "User {AccountName} level: {Level} [{Version}] authenticated!",
                userEntity.Username,
                userEntity.Level,
                handoffInfo.ClientVersion
            );
        }
    }

    public Task HandleAsync(PacketContext<LoginSeedPacket> context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

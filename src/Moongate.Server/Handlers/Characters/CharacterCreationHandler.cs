using Moongate.Abstractions.Attributes;
using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Network;
using Moongate.Network.UO.Packets.Incoming.Login;
using Moongate.Server.Services.Mobiles;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Handlers.Characters;

[RegisterPacketHandler]
public class CharacterCreationHandler : PacketHandlerBase<CharacterCreationPacket>
{
    private readonly ILogger _logger = Log.ForContext<CharacterCreationHandler>();
    private readonly MobileFactoryService _mobileFactory;

    public CharacterCreationHandler(
        IEventBusService eventBus,
        INetworkSessionManager sessions,
        IPlayerSessionService playerSessions,
        MobileFactoryService mobileFactory
    ) : base(eventBus, sessions, playerSessions)
    {
        ArgumentNullException.ThrowIfNull(mobileFactory);
        _mobileFactory = mobileFactory;
    }

    public override async Task HandleAsync(
        PacketContext<CharacterCreationPacket> context,
        CancellationToken cancellationToken = default
    )
    {
        if (!PlayerSessions.TryGetBySessionId(context.SessionId, out var session) || session.UserId is not { } accountId)
        {
            _logger.Warning("Character creation packet without an authenticated session ({SessionId})", context.SessionId);

            return;
        }

        var mobile = await _mobileFactory.CreatePlayerMobile(context.Packet, accountId, cancellationToken);

        _logger.Information(
            "Created player mobile {Name} ({MobileId}) for account {AccountId}",
            mobile.Name,
            mobile.Id,
            accountId
        );
    }
}

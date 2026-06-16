using Moongate.Abstractions.Attributes;
using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Network;
using Moongate.Network.UO.Packets.Incoming.Login;
using Moongate.Server.Interfaces.Services;
using Moongate.UO.Data.Interfaces.Services;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Handlers.Characters;

[RegisterPacketHandler]
public class PlayCharacterHandler : PacketHandlerBase<PlayCharacterPacket>
{
    private readonly ILogger _logger = Log.ForContext<PlayCharacterHandler>();
    private readonly IMobileService _mobiles;
    private readonly IWorldEntryService _worldEntry;

    public PlayCharacterHandler(
        IEventBusService eventBus,
        INetworkSessionManager sessions,
        IPlayerSessionService playerSessions,
        IMobileService mobiles,
        IWorldEntryService worldEntry
    ) : base(eventBus, sessions, playerSessions)
    {
        ArgumentNullException.ThrowIfNull(mobiles);
        ArgumentNullException.ThrowIfNull(worldEntry);

        _mobiles = mobiles;
        _worldEntry = worldEntry;
    }

    public override async Task HandleAsync(
        PacketContext<PlayCharacterPacket> context,
        CancellationToken cancellationToken = default
    )
    {
        if (!PlayerSessions.TryGetBySessionId(context.SessionId, out var session) || session.UserId is not { } accountId)
        {
            _logger.Warning("Play character packet without an authenticated session ({SessionId})", context.SessionId);

            return;
        }

        var characters = await _mobiles.GetByAccountIdAsync(accountId, cancellationToken);

        var slot = context.Packet.Slot;

        if (slot < 0 || slot >= characters.Count)
        {
            _logger.Warning(
                "Play character packet with out-of-range slot {Slot} for account {AccountId} ({Count} characters)",
                slot,
                accountId,
                characters.Count
            );

            return;
        }

        var mobile = characters[slot];

        PlayerSessions.EnterWorld(context.SessionId, mobile.Id, mobile.Id, DateTimeOffset.UtcNow);

        _logger.Information(
            "Player {Name} ({MobileId}) entering world on session {SessionId}",
            mobile.Name,
            mobile.Id,
            context.SessionId
        );

        await _worldEntry.EnterWorldAsync(context.SessionId, mobile, cancellationToken);
    }
}

using Moongate.Abstractions.Attributes;
using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Network;
using Moongate.Abstractions.Types.Expansions;
using Moongate.Network.UO.Data.Login;
using Moongate.Network.UO.Packets.Incoming.Login;
using Moongate.Network.UO.Packets.Outgoing.Login;
using Moongate.Server.Interfaces.Sessions;
using Moongate.UO.Data.Interfaces.Expansions;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Domain.Interfaces.Services;

namespace Moongate.Server.Handlers;

[RegisterPacketHandler]
public class GameLoginHandler : PacketHandlerBase<GameLoginPacket>, IPacketHandler<LoginSeedPacket>
{
    private readonly IGameLoginHandoffService _gameLoginHandoffService;

    private readonly IUserService _userService;

    private readonly IExpansionStore _expansionStore;

    private readonly IMobileService _mobileService;

    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<GameLoginHandler>();

    public GameLoginHandler(
        IEventBusService eventBus,
        INetworkSessionManager sessions,
        IPlayerSessionService playerSessions,
        IGameLoginHandoffService gameLoginHandoffService,
        IUserService userService,
        IExpansionStore expansionStore,
        IMobileService mobileService
    ) : base(eventBus, sessions, playerSessions)
    {
        _gameLoginHandoffService = gameLoginHandoffService;
        _userService = userService;
        _expansionStore = expansionStore;
        _mobileService = mobileService;
    }

    public override async Task HandleAsync(
        PacketContext<GameLoginPacket> context,
        CancellationToken cancellationToken = default
    )
    {
        if (!_gameLoginHandoffService.TryConsume(context.Packet.SessionKey, out var handoffInfo))
        {
            return;
        }

        var userEntity = await _userService.GetByUsernameAsync(context.Packet.AccountName, cancellationToken);

        if (userEntity is null)
        {
            _logger.Warning("Game login for unknown account {AccountName}", context.Packet.AccountName);

            return;
        }

        PlayerSessions.UpdateClient(context.SessionId, handoffInfo.ClientVersion);
        PlayerSessions.Authenticate(context.SessionId, userEntity.Id, userEntity.Username, DateTimeOffset.Now);

        _logger.Information(
            "User {AccountName} level: {Level} [{Version}] authenticated",
            userEntity.Username,
            userEntity.Level,
            handoffInfo.ClientVersion
        );

        context.Session.EnableCompression();

        var supportedFeatures = _expansionStore.Core?.SupportedFeatures ?? FeatureFlags.ExpansionEj;
        var characterListFlags = _expansionStore.Core?.CharacterListFlags;

        var characters = await _mobileService.GetByAccountIdAsync(userEntity.Id, cancellationToken);

        var characterEntries = characters
            .Select(m => new CharacterEntry(m.Name ?? string.Empty))
            .ToList();

        var charListPacket = new CharactersStartingLocationsPacket();
        charListPacket.Cities.AddRange(StartingCities.AvailableStartingCities);

        if (characterListFlags.HasValue)
        {
            charListPacket.Flags = characterListFlags.Value;
        }

        charListPacket.FillCharacters(characterEntries);

        await context.SendAsync(new SupportFeaturesPacket(supportedFeatures, true), cancellationToken);
        await context.SendAsync(charListPacket, cancellationToken);
    }

    public Task HandleAsync(PacketContext<LoginSeedPacket> context, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

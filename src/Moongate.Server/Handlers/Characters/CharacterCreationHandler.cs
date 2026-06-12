using Moongate.Abstractions.Attributes;
using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Network;
using Moongate.Network.UO.Packets.Incoming.Login;

namespace Moongate.Server.Handlers.Characters;

[RegisterPacketHandler]
public class CharacterCreationHandler : PacketHandlerBase<CharacterCreationPacket>
{
    public CharacterCreationHandler(
        IEventBusService eventBus,
        INetworkSessionManager sessions,
        IPlayerSessionService playerSessions
    ) : base(eventBus, sessions, playerSessions) { }

    public override Task HandleAsync(
        PacketContext<CharacterCreationPacket> context,
        CancellationToken cancellationToken = default
    )
        => Task.CompletedTask;
}

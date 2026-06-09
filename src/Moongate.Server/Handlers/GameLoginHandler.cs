using Moongate.Abstractions.Attributes;
using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Network;
using Moongate.Network.UO.Packets.Incoming.Login;

namespace Moongate.Server.Handlers;

[RegisterPacketHandler]
public class GameLoginHandler : PacketHandlerBase<GameLoginPacket>, IPacketHandler<LoginSeedPacket>
{
    public GameLoginHandler(
        IEventBusService eventBus,
        INetworkSessionManager sessions,
        IPlayerSessionService playerSessions
    ) : base(eventBus, sessions, playerSessions) { }

    public override Task HandleAsync(
        PacketContext<GameLoginPacket> context,
        CancellationToken cancellationToken = default
    )
    {
        return Task.CompletedTask;
    }

    public Task HandleAsync(PacketContext<LoginSeedPacket> context, CancellationToken cancellationToken = default)
    {

        return Task.CompletedTask;
    }
}

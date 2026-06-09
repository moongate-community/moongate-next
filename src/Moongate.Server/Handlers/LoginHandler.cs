using Moongate.Abstractions.Attributes;
using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Network;
using Moongate.Network.UO.Packets.Incoming.Login;

namespace Moongate.Server.Handlers;

[RegisterPacketHandler]
public class LoginHandler : PacketHandlerBase<LoginSeedPacket>, IPacketHandler<AccountLoginPacket>
{


    public LoginHandler(IEventBusService eventBus, INetworkSessionManager sessions) : base(eventBus, sessions) { }

    public override Task HandleAsync(PacketContext<LoginSeedPacket> context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;



    }

    public Task HandleAsync(PacketContext<AccountLoginPacket> context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

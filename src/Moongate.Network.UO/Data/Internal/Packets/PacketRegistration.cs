using Moongate.Network.UO.Data.Packets;
using Moongate.Abstractions.Interfaces.Network;

namespace Moongate.Network.UO.Data.Internal.Packets;

internal readonly record struct PacketRegistration
{
    public PacketDescriptor Descriptor { get; }
    public Func<IGameNetworkPacket> Factory { get; }

    public PacketRegistration(PacketDescriptor descriptor, Func<IGameNetworkPacket> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        Descriptor = descriptor;
        Factory = factory;
    }
}

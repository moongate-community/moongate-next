using Moongate.Abstractions.Interfaces.Network;
using Moongate.Network.UO.Data.Packets;

namespace Moongate.Network.UO.Data.Internal.Packets;

internal readonly record struct PacketRegistration
{
    public PacketRegistration(PacketDescriptor descriptor, Func<IGameNetworkPacket> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        Descriptor = descriptor;
        Factory = factory;
    }

    public PacketDescriptor Descriptor { get; }
    public Func<IGameNetworkPacket> Factory { get; }
}

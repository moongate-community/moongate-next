using Moongate.Core.Data.Directories;
using Moongate.Network.UO.Registry;

namespace Moongate.Server.Bootstrap;

public sealed class MoongateBootstrapContext
{
    public MoongateBootstrapContext(
        DirectoriesConfig directories,
        PacketRegistry packetRegistry,
        int registeredPacketCount
    )
    {
        Directories = directories;
        PacketRegistry = packetRegistry;
        RegisteredPacketCount = registeredPacketCount;
    }

    public DirectoriesConfig Directories { get; }

    public PacketRegistry PacketRegistry { get; }

    public int RegisteredPacketCount { get; }
}

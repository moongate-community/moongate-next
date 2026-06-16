using Moongate.Network.UO.Packets.Outgoing.World;
using Moongate.Tests.Support;

namespace Moongate.Tests.Network.Packets;

public class WarModePacketTests
{
    [Fact]
    public void Write_ProducesFixed5ByteLayout()
    {
        var bytes = PacketSerializer.Serialize(new WarModePacket());
        Assert.Equal(5, bytes.Length);
        Assert.Equal(0x72, bytes[0]);
        Assert.Equal(0x00, bytes[1]);
        Assert.Equal(0x32, bytes[3]);
    }
}

using Moongate.Network.UO.Packets.Outgoing.World;
using Moongate.Tests.Support;

namespace Moongate.Tests.Network.Packets;

public class SetMapPacketTests
{
    [Fact]
    public void Write_ProducesBfSetMapSubcommand()
    {
        var bytes = PacketSerializer.Serialize(new SetMapPacket(1));

        Assert.Equal(0xBF, bytes[0]);
        Assert.Equal(6, (bytes[1] << 8) | bytes[2]);
        Assert.Equal(0x0008, (bytes[3] << 8) | bytes[4]);
        Assert.Equal(1, bytes[5]);
        Assert.Equal(6, bytes.Length);
    }
}

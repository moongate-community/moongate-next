using System.Text;
using Moongate.Network.UO.Packets.Outgoing.Entity;
using Moongate.Tests.Support;
using Moongate.UO.Data.Entities.Mobiles;

namespace Moongate.Tests.Network.Packets;

public sealed class PaperdollPacketTests
{
    [Fact]
    public void Write_ProducesFixed66ByteLayout()
    {
        var mobile = new MobileEntity { Id = new(0x55), Name = "Tom the Brave" };

        var bytes = PacketSerializer.Serialize(new PaperdollPacket(mobile, mobile.Name!));

        Assert.Equal(66, bytes.Length);
        Assert.Equal(0x88, bytes[0]);
        Assert.Equal(0x55u, ((uint)bytes[1] << 24) | ((uint)bytes[2] << 16) | ((uint)bytes[3] << 8) | bytes[4]); // serial
        Assert.Equal(0x02, bytes[65]); // allow-lift flag
        Assert.Equal("Tom the Brave", Encoding.ASCII.GetString(bytes, 5, 13)); // display name at bytes[5..64], null-padded
    }
}

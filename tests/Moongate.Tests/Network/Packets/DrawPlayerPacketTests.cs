using Moongate.Core.Ids;
using Moongate.Core.Types;
using Moongate.Network.UO.Packets.Outgoing.Entity;
using Moongate.Tests.Support;
using Moongate.UO.Data.Entities.Mobiles;

namespace Moongate.Tests.Network.Packets;

public class DrawPlayerPacketTests
{
    [Fact]
    public void Write_ProducesFixed19ByteLayout()
    {
        var mobile = new MobileEntity
        {
            Id = new(0x55), BodyId = 401, SkinHue = (Hue)0x83EA,
            Location = new(10, 20, -5), Direction = DirectionType.East
        };
        var bytes = PacketSerializer.Serialize(new DrawPlayerPacket(mobile));
        Assert.Equal(19, bytes.Length);
        Assert.Equal(0x20, bytes[0]);
        Assert.Equal(401, (bytes[5] << 8) | bytes[6]);    // body @5..6
        Assert.Equal(0x83EA, (bytes[8] << 8) | bytes[9]); // hue @8..9
    }
}

using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Core.Types;
using Moongate.Network.UO.Packets.Outgoing.Login;
using Moongate.Tests.Support;
using Moongate.UO.Data.Entities.Mobiles;

namespace Moongate.Tests.Network.Packets;

public class LoginConfirmPacketTests
{
    [Fact]
    public void Write_ProducesFixed37ByteLayout()
    {
        var mobile = new MobileEntity
        {
            Id = new Serial(0x1234),
            BodyId = 400,
            Location = new Point3D(100, 200, 5),
            Direction = DirectionType.South
        };

        var bytes = PacketSerializer.Serialize(new LoginConfirmPacket(mobile, 6144, 4096));

        Assert.Equal(37, bytes.Length);
        Assert.Equal(0x1B, bytes[0]);
        Assert.Equal(400, (bytes[9] << 8) | bytes[10]);           // body short at offset 9..10
        Assert.Equal(0x1234, (bytes[3] << 8) | bytes[4]);         // serial low bytes (uint @1..4)
        Assert.Equal(100, (short)((bytes[11] << 8) | bytes[12])); // X
        Assert.Equal(200, (short)((bytes[13] << 8) | bytes[14])); // Y
        Assert.Equal(5, (short)((bytes[15] << 8) | bytes[16]));   // Z
        Assert.Equal(
            unchecked((int)0xFFFFFFFF),
            (bytes[19] << 24) | (bytes[20] << 16) | (bytes[21] << 8) | bytes[22]
        );                                                // -1 sentinel
        Assert.Equal(6144, (bytes[27] << 8) | bytes[28]); // map width
        Assert.Equal(4096, (bytes[29] << 8) | bytes[30]); // map height
    }
}

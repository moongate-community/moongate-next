using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Core.Types;
using Moongate.Network.UO.Packets.Outgoing.Login;
using Moongate.UO.Data.Entities.Mobiles;
using Xunit;

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

        var bytes = Serialize(new LoginConfirmPacket(mobile, mapWidth: 6144, mapHeight: 4096));

        Assert.Equal(37, bytes.Length);
        Assert.Equal(0x1B, bytes[0]);
        Assert.Equal(400, (bytes[9] << 8) | bytes[10]); // body short at offset 9..10
    }

    private static byte[] Serialize(Moongate.Network.UO.Base.BaseGameNetworkPacket packet)
    {
        var writer = new Moongate.Network.Spans.SpanWriter(256, true);
        packet.Write(ref writer);
        return writer.Span.ToArray();
    }
}

using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Core.Types;
using Moongate.Network.UO.Packets.Outgoing.Entity;
using Moongate.UO.Data.Entities.Mobiles;
using Xunit;

namespace Moongate.Tests.Network.Packets;

public class DrawPlayerPacketTests
{
    [Fact]
    public void Write_ProducesFixed19ByteLayout()
    {
        var mobile = new MobileEntity
        {
            Id = new Serial(0x55), BodyId = 401, SkinHue = (Hue)0x83EA,
            Location = new Point3D(10, 20, -5), Direction = DirectionType.East
        };
        var bytes = Serialize(new DrawPlayerPacket(mobile));
        Assert.Equal(19, bytes.Length);
        Assert.Equal(0x20, bytes[0]);
        Assert.Equal(401, (bytes[5] << 8) | bytes[6]);     // body @5..6
        Assert.Equal(0x83EA, (bytes[8] << 8) | bytes[9]);  // hue @8..9
    }

    private static byte[] Serialize(Moongate.Network.UO.Base.BaseGameNetworkPacket packet)
    {
        var writer = new Moongate.Network.Spans.SpanWriter(256, true);
        packet.Write(ref writer);
        return writer.Span.ToArray();
    }
}

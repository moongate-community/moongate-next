using Moongate.Core.Ids;
using Moongate.Network.UO.Packets.Outgoing.World;
using Moongate.UO.Data.Entities.Mobiles;
using Xunit;

namespace Moongate.Tests.Network.Packets;

public class WarModePacketTests
{
    [Fact]
    public void Write_ProducesFixed5ByteLayout()
    {
        var mobile = new MobileEntity { Id = new Serial(1) };
        var bytes = Serialize(new WarModePacket(mobile));
        Assert.Equal(5, bytes.Length);
        Assert.Equal(0x72, bytes[0]);
        Assert.Equal(0x00, bytes[1]);
        Assert.Equal(0x32, bytes[3]);
    }

    private static byte[] Serialize(Moongate.Network.UO.Base.BaseGameNetworkPacket packet)
    {
        var writer = new Moongate.Network.Spans.SpanWriter(256, true);
        packet.Write(ref writer);
        return writer.Span.ToArray();
    }
}

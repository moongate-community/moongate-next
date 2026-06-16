using Moongate.Core.Ids;
using Moongate.Network.UO.Packets.Outgoing.Entity;
using Moongate.Tests.Support;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Types.Items;
using Xunit;

namespace Moongate.Tests.Network.Packets;

public sealed class WornItemPacketTests
{
    [Fact]
    public void Write_ProducesFixed15ByteLayout()
    {
        var mobile = new MobileEntity { Id = new Serial(0x55) };
        var item = new ItemEntity { Id = new Serial(Serial.ItemOffset + 9), ItemId = 0x1F03, Hue = (Hue)0x44 };

        var bytes = PacketSerializer.Serialize(new WornItemPacket(mobile, item, ItemLayerType.OuterTorso));

        Assert.Equal(15, bytes.Length);
        Assert.Equal(0x2E, bytes[0]);
        Assert.Equal(0x1F03, (bytes[5] << 8) | bytes[6]);
        Assert.Equal((byte)ItemLayerType.OuterTorso, bytes[8]);
        Assert.Equal(0x44, (bytes[13] << 8) | bytes[14]);
    }
}

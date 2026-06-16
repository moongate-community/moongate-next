using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Network.UO.Packets.Outgoing.Entity;
using Moongate.Tests.Support;
using Moongate.UO.Data.Entities.Items;
using Xunit;

namespace Moongate.Tests.Network.Packets;

public sealed class ContainerContentPacketTests
{
    [Fact]
    public void Write_WritesHeaderAndPerItemEntries()
    {
        var containerId = new Serial(Serial.ItemOffset + 1);
        var items = new List<ItemEntity>
        {
            new()
            {
                Id = new Serial(Serial.ItemOffset + 2), ItemId = 0x0EED, Amount = 5,
                ContainerPosition = new Point2D(10, 20), Hue = (Hue)0
            }
        };

        var bytes = PacketSerializer.Serialize(new ContainerContentPacket(containerId, items));

        Assert.Equal(0x3C, bytes[0]);
        Assert.Equal(5 + 20, (bytes[1] << 8) | bytes[2]); // length 5 + 1*20
        Assert.Equal(1, (bytes[3] << 8) | bytes[4]);       // item count
        Assert.Equal(0x0EED, (bytes[9] << 8) | bytes[10]); // first item's graphic @9..10
        Assert.Equal(25, bytes.Length);
    }
}

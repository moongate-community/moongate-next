using Moongate.Core.Ids;
using Moongate.Network.UO.Packets.Outgoing.Entity;
using Moongate.Tests.Support;
using Moongate.UO.Data.Entities.Items;

namespace Moongate.Tests.Network.Packets;

public sealed class ContainerContentPacketTests
{
    [Fact]
    public void Write_ClampsAmountToAtLeastOne()
    {
        var items = new List<ItemEntity>
        {
            new()
            {
                Id = new(Serial.ItemOffset + 2), ItemId = 0x0EED, Amount = 0,
                ContainerPosition = new(0, 0), Hue = (Hue)0
            }
        };

        var bytes = PacketSerializer.Serialize(new ContainerContentPacket(new(Serial.ItemOffset + 1), items));

        Assert.Equal(1, (short)((bytes[12] << 8) | bytes[13])); // amount 0 -> clamped to 1
    }

    [Fact]
    public void Write_WritesHeaderAndPerItemEntries()
    {
        var containerId = new Serial(Serial.ItemOffset + 1);
        var items = new List<ItemEntity>
        {
            new()
            {
                Id = new(Serial.ItemOffset + 2), ItemId = 0x0EED, Amount = 5,
                ContainerPosition = new(10, 20), Hue = (Hue)0
            }
        };

        var bytes = PacketSerializer.Serialize(new ContainerContentPacket(containerId, items));

        Assert.Equal(0x3C, bytes[0]);
        Assert.Equal(5 + 20, (bytes[1] << 8) | bytes[2]);  // length 5 + 1*20
        Assert.Equal(1, (bytes[3] << 8) | bytes[4]);       // item count
        Assert.Equal(0x0EED, (bytes[9] << 8) | bytes[10]); // first item's graphic @9..10
        Assert.Equal(25, bytes.Length);
        Assert.Equal(5, (short)((bytes[12] << 8) | bytes[13]));  // amount
        Assert.Equal(10, (short)((bytes[14] << 8) | bytes[15])); // X
        Assert.Equal(20, (short)((bytes[16] << 8) | bytes[17])); // Y
        Assert.Equal(0, bytes[18]);                              // slot index of first item
        Assert.Equal(
            0x40000001u,
            ((uint)bytes[19] << 24) | ((uint)bytes[20] << 16) | ((uint)bytes[21] << 8) | bytes[22]
        ); // container id
    }
}

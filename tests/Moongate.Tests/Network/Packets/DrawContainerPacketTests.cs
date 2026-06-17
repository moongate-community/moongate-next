using Moongate.Core.Ids;
using Moongate.Network.UO.Packets.Outgoing.Entity;
using Moongate.Tests.Support;
using Moongate.UO.Data.Entities.Items;

namespace Moongate.Tests.Network.Packets;

public sealed class DrawContainerPacketTests
{
    [Fact]
    public void Write_ProducesFixed9ByteLayout_WithBackpackGumpFallback()
    {
        var backpack = new ItemEntity { Id = new Serial(Serial.ItemOffset + 1) }; // GumpId null -> 0x3C

        var bytes = PacketSerializer.Serialize(new DrawContainerPacket(backpack));

        Assert.Equal(9, bytes.Length);
        Assert.Equal(0x24, bytes[0]);
        Assert.Equal(0x3C, (bytes[5] << 8) | bytes[6]); // gump fallback @5..6
    }

    [Fact]
    public void Write_UsesExplicitGumpId_WhenSet()
    {
        var container = new ItemEntity { Id = new Serial(Serial.ItemOffset + 1), GumpId = 0x49 };

        var bytes = PacketSerializer.Serialize(new DrawContainerPacket(container));

        Assert.Equal(0x49, (bytes[5] << 8) | bytes[6]); // explicit gump id, not the 0x3C fallback
    }
}

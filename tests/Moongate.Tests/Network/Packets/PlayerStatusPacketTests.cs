using Moongate.Network.UO.Packets.Outgoing.Entity;
using Moongate.Tests.Support;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Types.Mobiles;

namespace Moongate.Tests.Network.Packets;

public class PlayerStatusPacketTests
{
    [Fact]
    public void Write_WritesNameAndVitalsWithLengthPrefix()
    {
        var mobile = new MobileEntity
        {
            Id = new(0x55), Name = "Tom", Gender = GenderType.Male,
            BaseStats = new() { Strength = 50, Dexterity = 40, Intelligence = 30 },
            Resources = new() { Hits = 45, MaxHits = 50, Mana = 30, MaxMana = 30, Stamina = 40, MaxStamina = 40 }
        };
        var bytes = PacketSerializer.Serialize(new PlayerStatusPacket(mobile));
        Assert.Equal(0x11, bytes[0]);
        Assert.Equal(bytes.Length, (bytes[1] << 8) | bytes[2]); // length prefix == actual length
        Assert.Equal(
            66,
            bytes.Length
        ); // opcode(1)+len(2)+id(4)+name(30)+attr(4)+canRename(1)+version(1)+isFemale(1)+7*ushort(14)+gold(4)+resist(2)+weight(2)
    }
}

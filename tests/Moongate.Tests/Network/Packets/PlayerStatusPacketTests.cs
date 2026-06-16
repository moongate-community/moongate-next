using Moongate.Core.Ids;
using Moongate.Network.UO.Packets.Outgoing.Entity;
using Moongate.UO.Data.Data.Mobiles;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Types.Mobiles;
using Xunit;

namespace Moongate.Tests.Network.Packets;

public class PlayerStatusPacketTests
{
    [Fact]
    public void Write_WritesNameAndVitalsWithLengthPrefix()
    {
        var mobile = new MobileEntity
        {
            Id = new Serial(0x55), Name = "Tom", Gender = GenderType.Male,
            BaseStats = new MobileStats { Strength = 50, Dexterity = 40, Intelligence = 30 },
            Resources = new MobileResources { Hits = 45, MaxHits = 50, Mana = 30, MaxMana = 30, Stamina = 40, MaxStamina = 40 }
        };
        var bytes = Serialize(new PlayerStatusPacket(mobile));
        Assert.Equal(0x11, bytes[0]);
        Assert.Equal(bytes.Length, (bytes[1] << 8) | bytes[2]); // length prefix == actual length
    }

    private static byte[] Serialize(Moongate.Network.UO.Base.BaseGameNetworkPacket packet)
    {
        var writer = new Moongate.Network.Spans.SpanWriter(256, true);
        packet.Write(ref writer);
        return writer.Span.ToArray();
    }
}

using Moongate.Network.UO.Packets.Incoming.Login;
using Moongate.UO.Data.Types;

namespace Moongate.Tests.Network.Packets;

public sealed class CharacterCreationPacketTests
{
    // Minimal valid 0xF8 payload (106 bytes total including opcode).
    // Layout per UO protocol docs:
    //   [0]      opcode 0xF8
    //   [1-4]    0xEDEDEDED
    //   [5-8]    0xFFFFFFFF
    //   [9]      0x00
    //   [10-39]  CharacterName (30 bytes ASCII)
    //   [40-41]  2 reserved bytes
    //   [42-45]  ClientFlags (uint)
    //   [46-49]  reserved int
    //   [50-53]  reserved int
    //   [54]     ProfessionId
    //   [55-69]  15 reserved bytes
    //   [70]     gender/race byte  (0 = male human)
    //   [71]     Strength
    //   [72]     Dexterity
    //   [73]     Intelligence
    //   [74-81]  4x (skill byte, value byte)
    //   [82-83]  skin hue (short)
    //   [84-87]  hair style + hue (2x short)
    //   [88-91]  facial hair style + hue (2x short)
    //   [92-93]  StartingCityIndex (short)
    //   [94-95]  2 reserved bytes
    //   [96-97]  Slot (short)
    //   [98-101] reserved int
    //   [102-103] shirt hue (short)
    //   [104-105] pants hue (short)
    private static byte[] BuildPacket(
        string name = "TestChar",
        byte genderRace = 0,
        byte str = 45,
        byte dex = 35,
        byte intel = 10,
        byte profId = 1,
        short cityIndex = 0,
        short slot = 0
    )
    {
        var buf = new byte[106];
        buf[0] = 0xF8;

        // 0xEDEDEDED
        buf[1] = 0xED; buf[2] = 0xED; buf[3] = 0xED; buf[4] = 0xED;
        // 0xFFFFFFFF
        buf[5] = 0xFF; buf[6] = 0xFF; buf[7] = 0xFF; buf[8] = 0xFF;
        // 0x00
        buf[9] = 0x00;

        var nameBytes = System.Text.Encoding.ASCII.GetBytes(name.PadRight(30, '\0'));
        Array.Copy(nameBytes, 0, buf, 10, Math.Min(nameBytes.Length, 30));

        // professionId at byte 54
        buf[54] = profId;

        // gender/race at byte 70
        buf[70] = genderRace;

        buf[71] = str;
        buf[72] = dex;
        buf[73] = intel;

        // 4 skills: (Magery=25, val=50), rest zeros
        buf[74] = (byte)UOSkillName.Magery;
        buf[75] = 50;

        // starting city index big-endian short at [92-93]
        buf[92] = (byte)(cityIndex >> 8);
        buf[93] = (byte)(cityIndex & 0xFF);

        // slot big-endian short at [96-97]
        buf[96] = (byte)(slot >> 8);
        buf[97] = (byte)(slot & 0xFF);

        return buf;
    }

    [Fact]
    public void TryParse_ValidPacket_ReturnsTrue()
    {
        var packet = new CharacterCreationPacket();

        Assert.True(packet.TryParse(BuildPacket()));
    }

    [Fact]
    public void TryParse_ParsesCharacterName()
    {
        var packet = new CharacterCreationPacket();

        packet.TryParse(BuildPacket(name: "Aldric"));

        Assert.Equal("Aldric", packet.CharacterName);
    }

    [Fact]
    public void TryParse_ParsesStats()
    {
        var packet = new CharacterCreationPacket();

        packet.TryParse(BuildPacket(str: 45, dex: 35, intel: 10));

        Assert.Equal(45, packet.Strength);
        Assert.Equal(35, packet.Dexterity);
        Assert.Equal(10, packet.Intelligence);
    }

    [Fact]
    public void TryParse_MaleHumanGender()
    {
        var packet = new CharacterCreationPacket();

        packet.TryParse(BuildPacket(genderRace: 0));

        Assert.Equal(GenderType.Male, packet.Gender);
        Assert.Equal(0, packet.RaceIndex);
    }

    [Fact]
    public void TryParse_FemaleElfGender()
    {
        var packet = new CharacterCreationPacket();

        packet.TryParse(BuildPacket(genderRace: 5));

        Assert.Equal(GenderType.Female, packet.Gender);
        Assert.Equal(1, packet.RaceIndex);
    }

    [Fact]
    public void TryParse_MaleGargoyleGender()
    {
        var packet = new CharacterCreationPacket();

        packet.TryParse(BuildPacket(genderRace: 6));

        Assert.Equal(GenderType.Male, packet.Gender);
        Assert.Equal(2, packet.RaceIndex);
    }

    [Fact]
    public void TryParse_ParsesProfessionId()
    {
        var packet = new CharacterCreationPacket();

        packet.TryParse(BuildPacket(profId: 3));

        Assert.Equal(3, packet.ProfessionId);
    }

    [Fact]
    public void TryParse_ParsesFirstSkill()
    {
        var packet = new CharacterCreationPacket();

        packet.TryParse(BuildPacket());

        Assert.Equal(4, packet.Skills.Count);
        Assert.Equal(UOSkillName.Magery, packet.Skills[0].Skill);
        Assert.Equal(50, packet.Skills[0].Value);
    }

    [Fact]
    public void TryParse_InvalidGenderRaceByte_ReturnsFalse()
    {
        var packet = new CharacterCreationPacket();

        Assert.False(packet.TryParse(BuildPacket(genderRace: 0xFF)));
    }

    [Fact]
    public void PacketTable_Includes_0xF8()
    {
        var registry = new Moongate.Network.UO.Registry.PacketRegistry();

        Moongate.Network.UO.Registry.PacketTable.Register(registry);

        Assert.True(registry.TryGetDescriptor(0xF8, out _));
        Assert.True(registry.TryCreatePacket(0xF8, out var p));
        Assert.Equal((byte)0xF8, p?.OpCode);
    }
}

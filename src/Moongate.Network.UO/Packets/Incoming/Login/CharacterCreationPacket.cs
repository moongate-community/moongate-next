using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Data.Login;
using Moongate.Network.UO.Types.Packets;
using Moongate.UO.Data.Races.Base;
using Moongate.UO.Data.Types;
using Moongate.UO.Data.Types.Expansions;
using Moongate.UO.Data.Types.Mobiles;
using Moongate.UO.Data.Types.Skills;

namespace Moongate.Network.UO.Packets.Incoming.Login;

/// <summary>
/// Character creation packet sent by clients 7.0.16.0 and later (opcode 0xF8, 106 bytes).
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = 106, Description = "Character Creation (7.0.16.0+)")]
public class CharacterCreationPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0xF8;

    public string CharacterName { get; private set; } = "";

    public ClientFlags ClientFlags { get; private set; }

    public int ProfessionId { get; private set; }

    public List<SkillKeyValue> Skills { get; } = [];

    public int Strength { get; private set; }

    public int Dexterity { get; private set; }

    public int Intelligence { get; private set; }

    public GenderType Gender { get; private set; }

    public int RaceIndex { get; private set; }

    public Race? Race { get; private set; }

    public short StartingCityIndex { get; private set; }

    public CityInfo? StartingCity { get; private set; }

    public int Slot { get; private set; }

    public HueStyle Skin { get; private set; }

    public HueStyle Hair { get; private set; }

    public HueStyle FacialHair { get; private set; }

    public HueStyle Shirt { get; private set; }

    public HueStyle Pants { get; private set; }

    public CharacterCreationPacket()
        : base(OpCodeValue, 106) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        reader.ReadInt32(); // 0xEDEDEDED
        reader.ReadInt32(); // 0xFFFFFFFF
        reader.ReadByte();  // 0x00
        CharacterName = reader.ReadAscii(30);
        reader.ReadBytes(2);

        ClientFlags = (ClientFlags)reader.ReadUInt32();
        reader.ReadInt32();
        reader.ReadInt32(); // reserved

        ProfessionId = reader.ReadByte();

        reader.ReadBytes(15);

        if (!TryParseGenderAndRace(reader.ReadByte(), out var gender, out var raceIndex))
        {
            return false;
        }

        Gender = gender;
        RaceIndex = raceIndex;
        Race = raceIndex >= 0 && raceIndex < Race.Races.Length ? Race.Races[raceIndex] : null;

        Strength = reader.ReadByte();
        Dexterity = reader.ReadByte();
        Intelligence = reader.ReadByte();

        Skills.Clear();
        Skills.Add(new((UOSkillName)reader.ReadByte(), reader.ReadByte()));
        Skills.Add(new((UOSkillName)reader.ReadByte(), reader.ReadByte()));
        Skills.Add(new((UOSkillName)reader.ReadByte(), reader.ReadByte()));
        Skills.Add(new((UOSkillName)reader.ReadByte(), reader.ReadByte()));

        Skin = new(0, reader.ReadInt16());
        Hair = new(reader.ReadInt16(), reader.ReadInt16());
        FacialHair = new(reader.ReadInt16(), reader.ReadInt16());

        StartingCityIndex = reader.ReadInt16();
        var cities = StartingCities.AvailableStartingCities;
        StartingCity = StartingCityIndex >= 0 && StartingCityIndex < cities.Length
            ? cities[StartingCityIndex]
            : null;

        reader.ReadBytes(2);
        Slot = reader.ReadInt16();

        reader.ReadInt32(); // reserved

        Shirt = new(0, reader.ReadInt16());
        Pants = new(0, reader.ReadInt16());

        return true;
    }

    private static bool TryParseGenderAndRace(byte value, out GenderType gender, out int raceIndex)
    {
        switch (value)
        {
            case 0:
            case 2:
                gender = GenderType.Male;
                raceIndex = 0; // Human
                return true;
            case 1:
            case 3:
                gender = GenderType.Female;
                raceIndex = 0; // Human
                return true;
            case 4:
                gender = GenderType.Male;
                raceIndex = 1; // Elf
                return true;
            case 5:
                gender = GenderType.Female;
                raceIndex = 1; // Elf
                return true;
            case 6:
                gender = GenderType.Male;
                raceIndex = 2; // Gargoyle
                return true;
            case 7:
                gender = GenderType.Female;
                raceIndex = 2; // Gargoyle
                return true;
            default:
                gender = default;
                raceIndex = -1;
                return false;
        }
    }
}

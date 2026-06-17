using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Data.Login;
using Moongate.Network.UO.Types.Packets;
using Moongate.UO.Data.Types.Expansions;

namespace Moongate.Network.UO.Packets.Outgoing.Login;

/// <summary>
///     Represents the Characters / Starting Locations packet (0xA9).
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Variable, Description = "Characters / Starting Locations")]
public class CharactersStartingLocationsPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0xA9;

    public CharactersStartingLocationsPacket()
        : base(OpCodeValue)
    {
    }

    public List<CityInfo> Cities { get; } = [];

    public List<CharacterEntry?> Characters { get; } = [];

    public CharacterListFlags Flags { get; set; } =
        CharacterListFlags.ExpansionEj | CharacterListFlags.SixthCharacterSlot | CharacterListFlags.SeventhCharacterSlot;

    public void FillCharacters(IReadOnlyList<CharacterEntry>? characters = null, int size = 7)
    {
        Characters.Clear();

        if (size < 1)
        {
            size = 1;
        }

        if (characters is not null)
        {
            for (var i = 0; i < characters.Count; i++)
            {
                Characters.Add(characters[i]);
            }
        }

        while (Characters.Count < size)
        {
            Characters.Add(null);
        }
    }

    public override void Write(ref SpanWriter writer)
    {
        var highestSlot = -1;

        for (var i = Characters.Count - 1; i >= 0; i--)
        {
            if (Characters[i] is not null)
            {
                highestSlot = i;

                break;
            }
        }

        // Supported slot counts: 1, 5, 6 or 7.
        var count = Math.Max(highestSlot + 1, 7);

        if (count is > 1 and < 5)
        {
            count = 5;
        }

        var length = 11 + CityInfo.Length * Cities.Count + count * CharacterEntry.Length;

        writer.Write(OpCode);
        writer.Write((ushort)length);
        writer.Write((byte)count);

        for (var i = 0; i < count; i++)
        {
            var character = i < Characters.Count ? Characters[i] : null;

            if (character is null)
            {
                writer.Clear(CharacterEntry.Length);

                continue;
            }

            writer.WriteAscii(character.Name, 30);
            writer.Clear(30);
        }

        writer.Write((byte)Cities.Count);

        for (var i = 0; i < Cities.Count; i++)
        {
            writer.Write(Cities[i].ToArray(i));
        }

        writer.Write((int)Flags);
        writer.Write((short)-1);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return true;
    }
}

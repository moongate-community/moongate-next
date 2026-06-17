using System.Text;
using Moongate.Core.Ids;
using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Internal.Packets;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.UI;

/// <summary>
///     Represents a gump menu selection packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Variable, Description = "Gump Menu Selection")]
public class GumpMenuSelectionPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0xB1;

    public GumpMenuSelectionPacket()
        : base(OpCodeValue)
    {
    }

    public int ButtonId { get; private set; }
    public int GumpId { get; private set; }
    public Serial Serial { get; private set; }
    public IReadOnlyList<int> Switches { get; private set; } = [];
    public IReadOnlyDictionary<ushort, string> TextEntries { get; private set; } = new Dictionary<ushort, string>();

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (!PacketLengthValidator.TryReadVariableLength(ref reader) || reader.Remaining < 16)
        {
            return false;
        }

        Serial = (Serial)reader.ReadUInt32();
        GumpId = reader.ReadInt32();
        ButtonId = reader.ReadInt32();

        var switchCount = reader.ReadInt32();

        if (switchCount < 0 || reader.Remaining < switchCount * 4 + 4)
        {
            return false;
        }

        Switches = ReadSwitches(ref reader, switchCount);

        var textCount = reader.ReadInt32();

        if (textCount < 0)
        {
            return false;
        }

        var textEntries = new Dictionary<ushort, string>(textCount);

        for (var i = 0; i < textCount; i++)
        {
            if (reader.Remaining < 4)
            {
                return false;
            }

            var entryId = reader.ReadUInt16();
            var charLength = reader.ReadUInt16();
            var byteLength = checked(charLength * 2);

            if (reader.Remaining < byteLength)
            {
                return false;
            }

            var rawText = reader.ReadBytes(byteLength);
            textEntries[entryId] = Encoding.BigEndianUnicode.GetString(rawText);
        }

        TextEntries = textEntries;

        return reader.Remaining == 0;
    }

    private static IReadOnlyList<int> ReadSwitches(ref SpanReader reader, int switchCount)
    {
        if (switchCount == 0)
        {
            return [];
        }

        var switches = new int[switchCount];

        for (var i = 0; i < switchCount; i++)
        {
            switches[i] = reader.ReadInt32();
        }

        return switches;
    }
}

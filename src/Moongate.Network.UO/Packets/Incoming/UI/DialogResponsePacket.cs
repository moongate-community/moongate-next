using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.UI;

/// <summary>
///     Represents a response to a dialog box packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Response To Dialog Box")]
public class DialogResponsePacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x7D;
    private const int LengthValue = 13;

    public DialogResponsePacket()
        : base(OpCodeValue, LengthValue)
    {
    }

    public uint DialogId { get; private set; }
    public uint ButtonId { get; private set; }
    public uint Unknown { get; private set; }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 12)
        {
            return false;
        }

        DialogId = reader.ReadUInt32();
        ButtonId = reader.ReadUInt32();
        Unknown = reader.ReadUInt32();

        return reader.Remaining == 0;
    }
}

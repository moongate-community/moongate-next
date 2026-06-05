using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.UI;

/// <summary>
/// Represents a help or tip request packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Send Help/Tip Request")]
public class SendHelpTipRequestPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0xB6;
    private const int LengthValue = 9;

    public uint LastTipId { get; private set; }
    public uint RequestFlag { get; private set; }

    public SendHelpTipRequestPacket()
        : base(OpCodeValue, LengthValue) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 8)
        {
            return false;
        }

        LastTipId = reader.ReadUInt32();
        RequestFlag = reader.ReadUInt32();

        return reader.Remaining == 0;
    }
}

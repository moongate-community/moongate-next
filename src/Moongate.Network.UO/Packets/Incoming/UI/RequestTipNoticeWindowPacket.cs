using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.UI;

/// <summary>
/// Represents a tip or notice window request packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Request Tip/Notice Window")]
public class RequestTipNoticeWindowPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0xA7;
    private const int LengthValue = 4;

    public ushort LastTipId { get; private set; }
    public byte RequestFlag { get; private set; }

    public RequestTipNoticeWindowPacket()
        : base(OpCodeValue, LengthValue) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 3)
        {
            return false;
        }

        LastTipId = reader.ReadUInt16();
        RequestFlag = reader.ReadByte();

        return reader.Remaining == 0;
    }
}

using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.UI;

/// <summary>
///     Represents a help request packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Request Help")]
public class RequestHelpPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x9B;
    private const int LengthValue = 258;

    public RequestHelpPacket()
        : base(OpCodeValue, LengthValue)
    {
    }

    public byte[] Payload { get; private set; } = [];

    protected override bool ParsePayload(ref SpanReader reader)
    {
        Payload = reader.ReadBytes(reader.Remaining);

        return reader.Remaining == 0;
    }
}

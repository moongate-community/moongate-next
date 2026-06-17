using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.Movement;

/// <summary>
///     Represents a war mode request packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Request War Mode")]
public class RequestWarModePacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x72;
    private const int LengthValue = 5;

    public RequestWarModePacket()
        : base(OpCodeValue, LengthValue)
    {
    }

    public bool IsWarMode { get; private set; }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 4)
        {
            return false;
        }

        IsWarMode = reader.ReadByte() != 0;
        _ = reader.ReadByte();
        _ = reader.ReadByte();
        _ = reader.ReadByte();

        return reader.Remaining == 0;
    }
}

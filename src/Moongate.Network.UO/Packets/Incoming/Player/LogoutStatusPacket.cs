using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.Player;

/// <summary>
///     Represents a logout status packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Logout Status")]
public class LogoutStatusPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0xD1;
    private const int LengthValue = 2;

    public LogoutStatusPacket()
        : base(OpCodeValue, LengthValue)
    {
    }

    public byte Status { get; private set; }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 1)
        {
            return false;
        }

        Status = reader.ReadByte();

        return reader.Remaining == 0;
    }
}

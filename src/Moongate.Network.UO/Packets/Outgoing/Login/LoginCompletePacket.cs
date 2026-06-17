using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Outgoing.Login;

/// <summary>
///     Represents a login complete packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Login Complete")]
public class LoginCompletePacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x55;
    private const int LengthValue = 1;

    public LoginCompletePacket()
        : base(OpCodeValue, LengthValue)
    {
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        return reader.Remaining == 0;
    }
}

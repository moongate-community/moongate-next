using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.Login;

[PacketHandler(0x00, PacketSizing.Fixed, Length = 104, Description = "Create Character")]

/// <summary>
/// Represents CreateCharacterPacket.
/// </summary>
public class CreateCharacterPacket : BaseGameNetworkPacket
{
    public CreateCharacterPacket()
        : base(0x00, 104) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}

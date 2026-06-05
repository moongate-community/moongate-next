using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.Login;

[PacketHandler(0x83, PacketSizing.Fixed, Length = 39, Description = "Delete Character")]

/// <summary>
/// Represents DeleteCharacterPacket.
/// </summary>
public class DeleteCharacterPacket : BaseGameNetworkPacket
{
    public DeleteCharacterPacket()
        : base(0x83, 39) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}

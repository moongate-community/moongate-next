using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.Login;

[PacketHandler(0xA0, PacketSizing.Fixed, Length = 3, Description = "Select Server")]
/// <summary>
/// Represents ServerSelectPacket.
/// </summary>
public class ServerSelectPacket : BaseGameNetworkPacket
{
    public ServerSelectPacket()
        : base(0xA0, 3)
    {
    }

    public int SelectedServerIndex { get; set; }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        SelectedServerIndex = reader.ReadInt16();

        return true;
    }
}

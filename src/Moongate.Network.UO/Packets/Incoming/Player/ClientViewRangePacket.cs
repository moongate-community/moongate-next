using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.Player;

/// <summary>
/// Represents a client view range packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Client View Range")]
public class ClientViewRangePacket : BaseGameNetworkPacket
{
    public const byte MinRange = 5;
    public const byte MaxRange = 18;

    private const byte OpCodeValue = 0xC8;
    private const int LengthValue = 2;

    public byte Range { get; set; } = MaxRange;

    public ClientViewRangePacket()
        : base(OpCodeValue, LengthValue) { }

    public ClientViewRangePacket(byte range)
        : this()
    {
        Range = range;
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write(Range);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 1)
        {
            return false;
        }

        Range = reader.ReadByte();

        return reader.Remaining == 0;
    }
}

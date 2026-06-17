using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Outgoing.World;

/// <summary>
///     Represents a world time packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Set Time")]
public class SetTimePacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x5B;
    private const int LengthValue = 4;

    public SetTimePacket()
        : base(OpCodeValue, LengthValue)
    {
        Time = DateTime.UtcNow;
    }

    public SetTimePacket(DateTime time)
        : this()
    {
        Time = time;
    }

    public DateTime Time { get; set; }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write((byte)Time.Hour);
        writer.Write((byte)Time.Minute);
        writer.Write((byte)Time.Second);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 3)
        {
            return false;
        }

        var hour = reader.ReadByte();
        var minute = reader.ReadByte();
        var second = reader.ReadByte();
        Time = DateTime.UtcNow.Date.AddHours(hour).AddMinutes(minute).AddSeconds(second);

        return reader.Remaining == 0;
    }
}

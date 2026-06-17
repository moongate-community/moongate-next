using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Outgoing.World;

/// <summary>
///     Represents a music change packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Set Music")]
public class SetMusicPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x6D;
    private const int LengthValue = 3;

    public SetMusicPacket()
        : base(OpCodeValue, LengthValue)
    {
    }

    public SetMusicPacket(int musicId)
        : this()
    {
        MusicId = musicId;
    }

    public int MusicId { get; set; }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write((ushort)MusicId);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 2)
        {
            return false;
        }

        MusicId = reader.ReadUInt16();

        return reader.Remaining == 0;
    }
}

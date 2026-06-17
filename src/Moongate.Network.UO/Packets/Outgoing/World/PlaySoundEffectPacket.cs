using Moongate.Core.Geometry;
using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Outgoing.World;

/// <summary>
///     Represents a sound effect packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Play Sound Effect")]
public class PlaySoundEffectPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x54;
    private const int LengthValue = 12;

    public PlaySoundEffectPacket()
        : base(OpCodeValue, LengthValue)
    {
    }

    public PlaySoundEffectPacket(byte mode, ushort soundModel, ushort unknown3, Point3D location)
        : this()
    {
        Mode = mode;
        SoundModel = soundModel;
        Unknown3 = unknown3;
        Location = location;
    }

    public byte Mode { get; set; } = 0x01;
    public ushort SoundModel { get; set; }
    public ushort Unknown3 { get; set; }
    public Point3D Location { get; set; }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write(Mode);
        writer.Write(SoundModel);
        writer.Write(Unknown3);
        writer.Write((ushort)Location.X);
        writer.Write((ushort)Location.Y);
        writer.Write((short)Location.Z);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 11)
        {
            return false;
        }

        Mode = reader.ReadByte();
        SoundModel = reader.ReadUInt16();
        Unknown3 = reader.ReadUInt16();
        var x = reader.ReadUInt16();
        var y = reader.ReadUInt16();
        var z = reader.ReadInt16();
        Location = new Point3D(x, y, z);

        return reader.Remaining == 0;
    }
}

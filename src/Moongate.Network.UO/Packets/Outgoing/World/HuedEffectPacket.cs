using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Effects;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Outgoing.World;

/// <summary>
/// Represents a hued graphical effect packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Hued Effect")]
public class HuedEffectPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0xC0;
    private const int LengthValue = 36;

    public EffectDirectionType DirectionType { get; set; }
    public Serial SourceSerial { get; set; }
    public Serial TargetSerial { get; set; }
    public ushort ItemId { get; set; }
    public Point3D SourceLocation { get; set; }
    public Point3D TargetLocation { get; set; }
    public byte Speed { get; set; }
    public byte Duration { get; set; }
    public byte Unknown1 { get; set; }
    public byte Unknown2 { get; set; }
    public bool FixedDirection { get; set; }
    public bool Explode { get; set; }
    public int Hue { get; set; }
    public int RenderMode { get; set; }

    public HuedEffectPacket()
        : base(OpCodeValue, LengthValue) { }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write((byte)DirectionType);
        writer.Write((uint)SourceSerial);
        writer.Write((uint)TargetSerial);
        writer.Write(ItemId);
        WriteLocations(ref writer);
        writer.Write(Speed);
        writer.Write(Duration);
        writer.Write(Unknown1);
        writer.Write(Unknown2);
        writer.Write(FixedDirection);
        writer.Write(Explode);
        writer.Write(Hue);
        writer.Write(RenderMode);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != LengthValue - 1)
        {
            return false;
        }

        DirectionType = (EffectDirectionType)reader.ReadByte();
        SourceSerial = (Serial)reader.ReadUInt32();
        TargetSerial = (Serial)reader.ReadUInt32();
        ItemId = reader.ReadUInt16();
        ReadLocations(ref reader);
        Speed = reader.ReadByte();
        Duration = reader.ReadByte();
        Unknown1 = reader.ReadByte();
        Unknown2 = reader.ReadByte();
        FixedDirection = reader.ReadBoolean();
        Explode = reader.ReadBoolean();
        Hue = reader.ReadInt32();
        RenderMode = reader.ReadInt32();

        return reader.Remaining == 0;
    }

    private void ReadLocations(ref SpanReader reader)
    {
        var sourceX = reader.ReadInt16();
        var sourceY = reader.ReadInt16();
        var sourceZ = reader.ReadSByte();
        var targetX = reader.ReadInt16();
        var targetY = reader.ReadInt16();
        var targetZ = reader.ReadSByte();

        SourceLocation = new(sourceX, sourceY, sourceZ);
        TargetLocation = new(targetX, targetY, targetZ);
    }

    private void WriteLocations(ref SpanWriter writer)
    {
        writer.Write((short)SourceLocation.X);
        writer.Write((short)SourceLocation.Y);
        writer.Write((sbyte)SourceLocation.Z);
        writer.Write((short)TargetLocation.X);
        writer.Write((short)TargetLocation.Y);
        writer.Write((sbyte)TargetLocation.Z);
    }
}

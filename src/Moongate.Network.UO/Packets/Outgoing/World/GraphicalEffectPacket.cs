using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Effects;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Outgoing.World;

/// <summary>
/// Represents a graphical effect packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Graphical Effect")]
public class GraphicalEffectPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x70;
    private const int LengthValue = 28;

    public EffectDirectionType DirectionType { get; set; }
    public Serial SourceSerial { get; set; }
    public Serial TargetSerial { get; set; }
    public ushort ItemId { get; set; }
    public Point3D SourceLocation { get; set; }
    public Point3D TargetLocation { get; set; }
    public byte Speed { get; set; }
    public byte Duration { get; set; }
    public ushort Unknown2 { get; set; }
    public bool AdjustDirectionDuringAnimation { get; set; } = true;
    public bool ExplodeOnImpact { get; set; }

    public GraphicalEffectPacket()
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
        writer.Write(Unknown2);
        writer.Write(AdjustDirectionDuringAnimation);
        writer.Write(ExplodeOnImpact);
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
        Unknown2 = reader.ReadUInt16();
        AdjustDirectionDuringAnimation = reader.ReadBoolean();
        ExplodeOnImpact = reader.ReadBoolean();

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

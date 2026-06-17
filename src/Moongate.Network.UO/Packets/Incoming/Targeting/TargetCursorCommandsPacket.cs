using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;
using Moongate.Network.UO.Types.Targeting;

namespace Moongate.Network.UO.Packets.Incoming.Targeting;

/// <summary>
///     Represents a target cursor command packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Target Cursor Commands")]
public class TargetCursorCommandsPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x6C;
    private const int LengthValue = 19;

    public TargetCursorCommandsPacket()
        : base(OpCodeValue, LengthValue)
    {
    }

    public TargetCursorCommandsPacket(
        TargetCursorSelectionType cursorTarget,
        Serial cursorId,
        TargetCursorType cursorType
    )
        : this()
    {
        CursorTarget = cursorTarget;
        CursorId = cursorId;
        CursorType = cursorType;
    }

    public TargetCursorSelectionType CursorTarget { get; set; }
    public Serial CursorId { get; set; }
    public TargetCursorType CursorType { get; set; }
    public Serial ClickedOnId { get; set; }
    public Point3D Location { get; set; }
    public byte Unknown { get; set; }
    public ushort Graphic { get; set; }

    public static TargetCursorCommandsPacket CreateCancelCurrentTarget()
    {
        return new TargetCursorCommandsPacket(
            TargetCursorSelectionType.SelectObject,
            (Serial)0u,
            TargetCursorType.CancelCurrentTargeting
        );
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write((byte)CursorTarget);
        writer.Write((uint)CursorId);
        writer.Write((byte)CursorType);
        writer.Write((uint)ClickedOnId);
        writer.Write((ushort)Location.X);
        writer.Write((ushort)Location.Y);
        writer.Write(Unknown);
        writer.Write((byte)Location.Z);
        writer.Write(Graphic);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 18)
        {
            return false;
        }

        CursorTarget = (TargetCursorSelectionType)reader.ReadByte();
        CursorId = (Serial)reader.ReadUInt32();
        CursorType = (TargetCursorType)reader.ReadByte();
        ClickedOnId = (Serial)reader.ReadUInt32();
        var x = reader.ReadUInt16();
        var y = reader.ReadUInt16();
        Unknown = reader.ReadByte();
        var z = unchecked((sbyte)reader.ReadByte());
        Graphic = reader.ReadUInt16();
        Location = new Point3D(x, y, z);

        return reader.Remaining == 0;
    }
}

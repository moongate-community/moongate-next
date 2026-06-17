using Moongate.Core.Ids;
using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Outgoing.Entity;

/// <summary>Outgoing "Delete Object" (0x1D): removes an entity from the client's view.</summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Delete Object")]
public class DeleteObjectPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x1D;
    private const int LengthValue = 5;

    public Serial Serial { get; }

    public DeleteObjectPacket(Serial serial)
        : base(OpCodeValue, LengthValue)
    {
        Serial = serial;
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write(Serial.Value);
    }

    protected override bool ParsePayload(ref SpanReader reader)
        => false;
}

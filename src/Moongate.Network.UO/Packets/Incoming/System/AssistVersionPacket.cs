using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Internal.Packets;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.System;

/// <summary>
/// Represents an assistant version packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Variable, Description = "Assist Version")]
public class AssistVersionPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0xBE;

    public byte[] Payload { get; private set; } = [];

    public AssistVersionPacket()
        : base(OpCodeValue) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (!PacketLengthValidator.TryReadVariableLength(ref reader))
        {
            return false;
        }

        Payload = reader.ReadBytes(reader.Remaining);

        return reader.Remaining == 0;
    }
}

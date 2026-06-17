using Moongate.Network.Spans;
using Moongate.Network.UO.Attributes;
using Moongate.Network.UO.Base;
using Moongate.Network.UO.Internal.Packets;
using Moongate.Network.UO.Types.Packets;

namespace Moongate.Network.UO.Packets.Incoming.System;

/// <summary>
///     Represents a freeshard list packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Variable, Description = "Freeshard List")]
public class FreeshardListPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0xF1;

    public FreeshardListPacket()
        : base(OpCodeValue)
    {
    }

    public byte[] Payload { get; private set; } = [];

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

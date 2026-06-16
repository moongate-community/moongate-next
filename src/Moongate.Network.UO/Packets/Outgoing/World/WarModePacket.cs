using Moongate.Network.Spans;
using Moongate.Network.UO.Base;

namespace Moongate.Network.UO.Packets.Outgoing.World;

/// <summary>
/// Outgoing "War Mode" (0x72): reports the player's combat stance (MVP: always peace).
/// Outgoing-only: carries no <c>[PacketHandler]</c> because opcode 0x72 is already registered
/// inbound by <c>RequestWarModePacket</c> (the packet registry rejects duplicate opcodes).
/// </summary>
public class WarModePacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x72;
    private const int LengthValue = 5;

    public bool WarMode { get; }

    public WarModePacket(bool warMode = false)
        : base(OpCodeValue, LengthValue)
    {
        WarMode = warMode;
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write(WarMode);
        writer.Write((byte)0);
        writer.Write((byte)0x32); // UO protocol constant (war-mode trailer)
        writer.Write((byte)0);
    }

    protected override bool ParsePayload(ref SpanReader reader)
        => false;
}

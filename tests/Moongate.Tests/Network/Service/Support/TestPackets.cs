using Moongate.Network.Spans;
using Moongate.Network.UO.Base;

namespace Moongate.Tests.Network.Service.Support;

/// <summary>
/// Fixed-length (5 bytes) packet whose payload always parses. Opcode 0x06.
/// </summary>
internal sealed class TestFixedPacket : BaseGameNetworkPacket
{
    public TestFixedPacket()
        : base(0x06, 5) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}

/// <summary>
/// Fixed-length (5 bytes) packet whose payload always fails to parse. Opcode 0x07.
/// </summary>
internal sealed class TestFailingPacket : BaseGameNetworkPacket
{
    public TestFailingPacket()
        : base(0x07, 5) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => false;
}

/// <summary>
/// Variable-length packet (length read from bytes 1-2). Opcode 0x12.
/// </summary>
internal sealed class TestVariablePacket : BaseGameNetworkPacket
{
    public TestVariablePacket()
        : base(0x12) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}

using Moongate.Abstractions.Interfaces.Network;
using Moongate.Network.UO.Registry;
using Moongate.Server.Services.Network.Internal;
using Moongate.Tests.Network.Service.Support;

namespace Moongate.Tests.Network.Service;

public class PacketParserTests
{
    private const int MaxPendingBufferBytes = 64 * 1024;
    private const int MaxDeclaredPacketLength = 16 * 1024;

    [Fact]
    public void Append_CompletingPartialPacketAcrossCalls_InvokesCallback()
    {
        var (parser, metrics, captured, pending) = Setup();

        parser.Append(pending, [0x06, 0, 0, 0], metrics, Capture(captured));
        parser.Append(pending, [0x2A], metrics, Capture(captured));

        Assert.Single(captured);
        Assert.Empty(pending);
        Assert.Equal(5, metrics.ReceivedBytes);
    }

    [Fact]
    public void Append_ExceedsPendingBufferLimit_DropsBufferAndCountsOverflow()
    {
        var (parser, metrics, captured, pending) = Setup(4);

        parser.Append(pending, [0x06, 0, 0, 0, 0], metrics, Capture(captured));

        Assert.Empty(captured);
        Assert.Empty(pending);
        Assert.Equal(1, metrics.PendingBufferOverflows);
    }

    [Fact]
    public void Append_LoginSeedOpcode_RecognizedAsPacketNotRawSeed()
    {
        var (parser, metrics, captured, pending) = Setup();
        var state = new PacketStreamState();

        // A lone 0xEF is recognized as the login-seed packet start: marked seed-consumed and left
        // for normal framing (a known opcode awaiting more bytes), not eaten as a raw 4-byte seed.
        parser.Append(pending, [0xEF], metrics, Capture(captured), state);

        Assert.True(state.SeedConsumed);
        Assert.Null(state.Seed);
        Assert.Equal(1, pending.Count);
        Assert.Empty(captured);
    }

    [Fact]
    public void Append_PartialFixedPacket_RetainsBytesAndDoesNotInvoke()
    {
        var (parser, metrics, captured, pending) = Setup();

        parser.Append(pending, [0x06, 0, 0, 0], metrics, Capture(captured));

        Assert.Empty(captured);
        Assert.Equal(4, pending.Count);
        Assert.Equal(0, metrics.ParsedPackets);
    }

    [Fact]
    public void Append_PayloadParseFailure_CountsFailureAndConsumesPacket()
    {
        var (parser, metrics, captured, pending) = Setup();

        // opcode 0x07 is registered as fixed length 5 but its payload always fails to parse.
        parser.Append(pending, [0x07, 0, 0, 0, 0], metrics, Capture(captured));

        Assert.Empty(captured);
        Assert.Empty(pending);
        Assert.Equal(1, metrics.ParseFailures);
        Assert.Equal(0, metrics.ParsedPackets);
    }

    [Fact]
    public void Append_RawSeed_ConsumesFourBytesThenParsesFollowingPacket()
    {
        var (parser, metrics, captured, pending) = Setup();
        var state = new PacketStreamState();

        // Game-server reconnect: 4 raw seed bytes (0xDEADBEEF big-endian) then a fixed 0x06 packet.
        parser.Append(pending, [0xDE, 0xAD, 0xBE, 0xEF, 0x06, 0, 0, 0, 0x2A], metrics, Capture(captured), state);

        Assert.True(state.SeedConsumed);
        Assert.Equal(0xDEADBEEFu, state.Seed);
        Assert.Single(captured);
        Assert.Empty(pending);
    }

    [Fact]
    public void Append_RawSeedSplitAcrossCalls_WaitsThenConsumes()
    {
        var (parser, metrics, captured, pending) = Setup();
        var state = new PacketStreamState();

        parser.Append(pending, [0xDE, 0xAD], metrics, Capture(captured), state);

        Assert.False(state.SeedConsumed);
        Assert.Equal(2, pending.Count);

        parser.Append(pending, [0xBE, 0xEF, 0x06, 0, 0, 0, 0x2A], metrics, Capture(captured), state);

        Assert.True(state.SeedConsumed);
        Assert.Equal(0xDEADBEEFu, state.Seed);
        Assert.Single(captured);
    }

    [Fact]
    public void Append_SingleFixedPacket_InvokesCallbackOnceAndEmptiesBuffer()
    {
        var (parser, metrics, captured, pending) = Setup();

        parser.Append(pending, [0x06, 0, 0, 0, 0x2A], metrics, Capture(captured));

        Assert.Single(captured);
        Assert.Equal(0x06, captured[0].OpCode);
        Assert.Empty(pending);
        Assert.Equal(1, metrics.ParsedPackets);
        Assert.Equal(5, metrics.ReceivedBytes);
    }

    [Fact]
    public void Append_SingleFixedPacket_PassesRawPacketToCallback()
    {
        var (parser, metrics, captured, pending) = Setup();
        byte[]? raw = null;

        parser.Append(
            pending,
            [0x06, 0, 0, 0, 0x2A],
            metrics,
            (_, packet, rawPacket) =>
            {
                captured.Add(packet);
                raw = rawPacket;
            }
        );

        Assert.Single(captured);
        Assert.Equal(new byte[] { 0x06, 0, 0, 0, 0x2A }, raw);
    }

    [Fact]
    public void Append_TwoFixedPacketsInOneBuffer_InvokesCallbackTwice()
    {
        var (parser, metrics, captured, pending) = Setup();

        parser.Append(pending, [0x06, 0, 0, 0, 1, 0x06, 0, 0, 0, 2], metrics, Capture(captured));

        Assert.Equal(2, captured.Count);
        Assert.Empty(pending);
        Assert.Equal(2, metrics.ParsedPackets);
    }

    [Fact]
    public void Append_UnknownOpcode_DropsBufferAndCountsDrop()
    {
        var (parser, metrics, captured, pending) = Setup();

        parser.Append(pending, [0xFE, 1, 2, 3], metrics, Capture(captured));

        Assert.Empty(captured);
        Assert.Empty(pending);
        Assert.Equal(1, metrics.UnknownOpcodeDrops);
    }

    [Fact]
    public void Append_VariablePacket_ReadsLengthFromHeaderAndParses()
    {
        var (parser, metrics, captured, pending) = Setup();

        // opcode 0x12, declared length 6 (0x0006), then 3 payload bytes.
        parser.Append(pending, [0x12, 0x00, 0x06, 0xAA, 0xBB, 0xCC], metrics, Capture(captured));

        Assert.Single(captured);
        Assert.Equal(0x12, captured[0].OpCode);
        Assert.Empty(pending);
    }

    [Fact]
    public void Append_VariablePacketExceedingMaxDeclaredLength_DropsBuffer()
    {
        var (parser, metrics, captured, pending) = Setup(maxDeclaredPacketLength: 8);

        // Declared length 0x00FF (255) > max 8.
        parser.Append(pending, [0x12, 0x00, 0xFF, 0x01], metrics, Capture(captured));

        Assert.Empty(captured);
        Assert.Empty(pending);
        Assert.Equal(1, metrics.InvalidLengthDrops);
    }

    [Fact]
    public void Append_VariablePacketWithInvalidLength_DropsBufferAndCountsInvalid()
    {
        var (parser, metrics, captured, pending) = Setup();

        // opcode 0x12, declared length 0 -> invalid.
        parser.Append(pending, [0x12, 0x00, 0x00], metrics, Capture(captured));

        Assert.Empty(captured);
        Assert.Empty(pending);
        Assert.Equal(1, metrics.InvalidLengthDrops);
    }

    [Fact]
    public void Append_VariablePacketWithoutLengthHeader_RetainsBytes()
    {
        var (parser, metrics, captured, pending) = Setup();

        // Only 2 bytes: cannot read the 2-byte length yet.
        parser.Append(pending, [0x12, 0x00], metrics, Capture(captured));

        Assert.Empty(captured);
        Assert.Equal(2, pending.Count);
    }

    private static Action<byte, IGameNetworkPacket, byte[]> Capture(List<IGameNetworkPacket> sink)
    {
        return (_, packet, _) => sink.Add(packet);
    }

    private static (PacketParser parser, NetworkParserSessionMetrics metrics, List<IGameNetworkPacket> captured, List<byte>
        pending) Setup(
            int maxPendingBufferBytes = MaxPendingBufferBytes,
            int maxDeclaredPacketLength = MaxDeclaredPacketLength
        )
    {
        var registry = new PacketRegistry();
        registry.RegisterFixed<TestFixedPacket>(0x06, 5);
        registry.RegisterFixed<TestFailingPacket>(0x07, 5);
        registry.RegisterVariable<TestVariablePacket>(0x12);
        registry.RegisterFixed<TestFixedPacket>(0xEF, 5);

        var parser = new PacketParser(registry, maxPendingBufferBytes, maxDeclaredPacketLength);

        return (parser, new NetworkParserSessionMetrics(), [], []);
    }
}

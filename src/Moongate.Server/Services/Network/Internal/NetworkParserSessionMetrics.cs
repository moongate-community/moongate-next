namespace Moongate.Server.Services.Network.Internal;

/// <summary>
/// Per-session parser counters updated with interlocked operations from the ingress thread.
/// </summary>
internal sealed class NetworkParserSessionMetrics
{
    private long _receivedBytes;
    private long _parsedPackets;
    private long _unknownOpcodeDrops;
    private long _invalidLengthDrops;
    private long _parseFailures;
    private long _pendingBufferOverflows;

    public long ReceivedBytes => Interlocked.Read(ref _receivedBytes);
    public long ParsedPackets => Interlocked.Read(ref _parsedPackets);
    public long UnknownOpcodeDrops => Interlocked.Read(ref _unknownOpcodeDrops);
    public long InvalidLengthDrops => Interlocked.Read(ref _invalidLengthDrops);
    public long ParseFailures => Interlocked.Read(ref _parseFailures);
    public long PendingBufferOverflows => Interlocked.Read(ref _pendingBufferOverflows);

    public void AddReceivedBytes(int bytes)
        => Interlocked.Add(ref _receivedBytes, bytes);

    public void IncrementInvalidLengthDrops()
        => Interlocked.Increment(ref _invalidLengthDrops);

    public void IncrementParsedPackets()
        => Interlocked.Increment(ref _parsedPackets);

    public void IncrementParseFailures()
        => Interlocked.Increment(ref _parseFailures);

    public void IncrementPendingBufferOverflows()
        => Interlocked.Increment(ref _pendingBufferOverflows);

    public void IncrementUnknownOpcodeDrops()
        => Interlocked.Increment(ref _unknownOpcodeDrops);
}

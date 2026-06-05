using Moongate.Server.Services.Network.Internal;

namespace Moongate.Tests.Network.Service;

public class NetworkParserSessionMetricsTests
{
    [Fact]
    public void AddReceivedBytes_Accumulates()
    {
        var metrics = new NetworkParserSessionMetrics();

        metrics.AddReceivedBytes(10);
        metrics.AddReceivedBytes(5);

        Assert.Equal(15, metrics.ReceivedBytes);
    }

    [Fact]
    public void Counters_AreThreadSafeUnderConcurrentIncrements()
    {
        var metrics = new NetworkParserSessionMetrics();
        const int threads = 8;
        const int perThread = 1000;

        var workers = new Thread[threads];

        for (var t = 0; t < threads; t++)
        {
            workers[t] = new(
                () =>
                {
                    for (var i = 0; i < perThread; i++)
                    {
                        metrics.IncrementParsedPackets();
                        metrics.AddReceivedBytes(2);
                    }
                }
            );
        }

        foreach (var worker in workers)
        {
            worker.Start();
        }

        foreach (var worker in workers)
        {
            worker.Join();
        }

        Assert.Equal(threads * perThread, metrics.ParsedPackets);
        Assert.Equal(threads * perThread * 2, metrics.ReceivedBytes);
    }

    [Fact]
    public void Increment_EachCounter_IncreasesByOne()
    {
        var metrics = new NetworkParserSessionMetrics();

        metrics.IncrementParsedPackets();
        metrics.IncrementUnknownOpcodeDrops();
        metrics.IncrementInvalidLengthDrops();
        metrics.IncrementParseFailures();
        metrics.IncrementPendingBufferOverflows();

        Assert.Equal(1, metrics.ParsedPackets);
        Assert.Equal(1, metrics.UnknownOpcodeDrops);
        Assert.Equal(1, metrics.InvalidLengthDrops);
        Assert.Equal(1, metrics.ParseFailures);
        Assert.Equal(1, metrics.PendingBufferOverflows);
    }

    [Fact]
    public void NewInstance_AllCountersZero()
    {
        var metrics = new NetworkParserSessionMetrics();

        Assert.Equal(0, metrics.ReceivedBytes);
        Assert.Equal(0, metrics.ParsedPackets);
        Assert.Equal(0, metrics.UnknownOpcodeDrops);
        Assert.Equal(0, metrics.InvalidLengthDrops);
        Assert.Equal(0, metrics.ParseFailures);
        Assert.Equal(0, metrics.PendingBufferOverflows);
    }
}

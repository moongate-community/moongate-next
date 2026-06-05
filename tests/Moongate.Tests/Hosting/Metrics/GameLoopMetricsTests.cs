using Microsoft.Extensions.DependencyInjection;
using Moongate.Abstractions.Types.Metrics;
using Moongate.Server.Services.EventBus;
using Moongate.Server.Services.GameLoop;

namespace Moongate.Tests.Hosting.Metrics;

public class GameLoopMetricsTests
{
    [Fact]
    public async Task Collect_AfterStartStop_TickCountIsPositive()
    {
        var (_, loop) = Build();
        await loop.StartAsync(CancellationToken.None);
        await Task.Delay(50);
        await loop.StopAsync(CancellationToken.None);

        Assert.True(loop.Collect().Single(s => s.Name == "tick_count").Value > 0);
    }

    [Fact]
    public void Collect_ReturnsAllFourCanonicalSamples()
    {
        var (_, loop) = Build();
        var names = loop.Collect().Select(s => s.Name).ToHashSet();

        Assert.Contains("tick_count", names);
        Assert.Contains("tick_avg_ms", names);
        Assert.Contains("tick_max_ms", names);
        Assert.Contains("idle_sleeps_total", names);
    }

    [Fact]
    public void Collect_TypeAssignmentMatchesSpec()
    {
        var (_, loop) = Build();
        var byName = loop.Collect().ToDictionary(s => s.Name, s => s);

        Assert.Equal(MetricType.Counter, byName["tick_count"].Type);
        Assert.Equal(MetricType.Counter, byName["idle_sleeps_total"].Type);
        Assert.Equal(MetricType.Gauge, byName["tick_avg_ms"].Type);
        Assert.Equal(MetricType.Gauge, byName["tick_max_ms"].Type);
    }

    [Fact]
    public void Prefix_IsGameloop()
    {
        var (_, loop) = Build();
        Assert.Equal("gameloop", loop.Prefix);
    }

    private static (EventBusService bus, GameLoopService loop) Build()
    {
        var bus = new EventBusService(new ServiceCollection().BuildServiceProvider());
        var loop = new GameLoopService(bus, new() { IdleSleepMs = 1 });

        return (bus, loop);
    }
}

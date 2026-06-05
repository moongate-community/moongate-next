using Moongate.Abstractions.Types.Metrics;
using Moongate.Server.Services.Timing;

namespace Moongate.Tests.Hosting.Timing;

public class TimerWheelServiceMetricsTests
{
    [Fact]
    public void Collect_ActiveGauge_ReflectsRegistrationCount()
    {
        var svc = NewService();
        svc.RegisterTimer("a", TimeSpan.FromMilliseconds(8), () => { });
        svc.RegisterTimer("b", TimeSpan.FromMilliseconds(8), () => { });

        var active = svc.Collect().Single(s => s.Name == "active").Value;
        Assert.Equal(2, active);
    }

    [Fact]
    public void Collect_AfterCallbackThrows_ErrorsCounterIncreases()
    {
        var svc = NewService(8, 8);
        svc.RegisterTimer("bad", TimeSpan.FromMilliseconds(8), () => throw new InvalidOperationException("boom"));
        svc.UpdateTicksDelta(0);
        svc.UpdateTicksDelta(8);

        var errors = svc.Collect().Single(s => s.Name == "callback_errors_total").Value;
        Assert.Equal(1, errors);
    }

    [Fact]
    public void Collect_AfterExecutions_CountersIncrease()
    {
        var svc = NewService(8, 8);
        svc.RegisterTimer("x", TimeSpan.FromMilliseconds(8), () => { });
        svc.UpdateTicksDelta(0);
        svc.UpdateTicksDelta(8);

        var byName = svc.Collect().ToDictionary(s => s.Name, s => s);
        Assert.Equal(1, byName["registered_total"].Value);
        Assert.Equal(1, byName["executed_total"].Value);
        Assert.Equal(1, byName["processed_ticks_total"].Value);
        Assert.Equal(0, byName["callback_errors_total"].Value);
    }

    [Fact]
    public void Collect_CounterTotals_HaveCorrectMetricType()
    {
        var svc = NewService();
        var byName = svc.Collect().ToDictionary(s => s.Name, s => s);

        Assert.Equal(MetricType.Counter, byName["registered_total"].Type);
        Assert.Equal(MetricType.Counter, byName["executed_total"].Type);
        Assert.Equal(MetricType.Counter, byName["callback_errors_total"].Type);
        Assert.Equal(MetricType.Counter, byName["processed_ticks_total"].Type);
        Assert.Equal(MetricType.Gauge, byName["active"].Type);
        Assert.Equal(MetricType.Gauge, byName["callback_avg_ms"].Type);
    }

    [Fact]
    public void Collect_ReturnsAllSixCanonicalSamples()
    {
        var svc = NewService();
        var samples = svc.Collect();
        var names = samples.Select(s => s.Name).ToHashSet();

        Assert.Contains("active", names);
        Assert.Contains("registered_total", names);
        Assert.Contains("executed_total", names);
        Assert.Contains("callback_errors_total", names);
        Assert.Contains("callback_avg_ms", names);
        Assert.Contains("processed_ticks_total", names);
    }

    [Fact]
    public void Prefix_IsTimer()
    {
        var svc = NewService();
        Assert.Equal("timer", svc.Prefix);
    }

    private static TimerWheelService NewService(int tickDurationMs = 8, int wheelSize = 16)
        => new(
            new()
            {
                TickDuration = TimeSpan.FromMilliseconds(tickDurationMs),
                WheelSize = wheelSize
            }
        );
}

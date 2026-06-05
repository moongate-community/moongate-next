using Microsoft.Extensions.DependencyInjection;
using Moongate.Abstractions.Interfaces.EventHandlers;
using Moongate.Abstractions.Types.Metrics;
using Moongate.Server.Services.EventBus;
using Moongate.Tests.Hosting.EventBus.Support;

namespace Moongate.Tests.Hosting.Metrics;

public class EventBusMetricsTests
{
    [Fact]
    public async Task Collect_AfterAsyncHandlerThrows_ErrorCounterIncrements()
    {
        var bus = BuildBus(
            services =>
            {
                services.AddSingleton<IAsyncEventHandler<TestAsyncEvent>>(_ => new ThrowingAsyncHandler());
            }
        );

        await bus.PublishAsync(new TestAsyncEvent("x"));

        Assert.Equal(1, ValueOf(bus, "handler_errors_total"));
    }

    [Fact]
    public async Task Collect_AfterAsyncPublish_AsyncCounterIncrements()
    {
        var bus = BuildBus(_ => { });

        await bus.PublishAsync(new TestAsyncEvent("x"));

        var v = ValueOf(bus, "async_events_total");
        Assert.Equal(1, v);
    }

    [Fact]
    public void Collect_AfterTickHandlerThrows_ErrorCounterIncrements()
    {
        var bus = BuildBus(
            services =>
            {
                services.AddSingleton<ITickEventHandler<TestTickEvent>>(_ => new ThrowingTickHandler());
            }
        );

        bus.Publish(new TestTickEvent(1));
        bus.DrainTickEvents(10);

        Assert.Equal(1, ValueOf(bus, "handler_errors_total"));
    }

    [Fact]
    public void Collect_AfterTickPublish_TickCounterIncrementsAndQueueDepthShown()
    {
        var bus = BuildBus(_ => { });

        bus.Publish(new TestTickEvent(1));
        bus.Publish(new TestTickEvent(2));

        Assert.Equal(2, ValueOf(bus, "tick_events_total"));
        Assert.Equal(2, ValueOf(bus, "tick_queue_depth"));
    }

    [Fact]
    public void Collect_CountersHaveCorrectMetricType()
    {
        var bus = BuildBus(_ => { });
        var byName = bus.Collect().ToDictionary(s => s.Name, s => s);

        Assert.Equal(MetricType.Counter, byName["async_events_total"].Type);
        Assert.Equal(MetricType.Counter, byName["tick_events_total"].Type);
        Assert.Equal(MetricType.Counter, byName["handler_errors_total"].Type);
        Assert.Equal(MetricType.Gauge, byName["tick_queue_depth"].Type);
    }

    [Fact]
    public void Prefix_IsBus()
    {
        var bus = BuildBus(_ => { });
        Assert.Equal("bus", bus.Prefix);
    }

    private static EventBusService BuildBus(Action<ServiceCollection> configure)
    {
        var services = new ServiceCollection();
        configure(services);

        return new(services.BuildServiceProvider());
    }

    private static double ValueOf(EventBusService bus, string name)
        => bus.Collect().Single(s => s.Name == name).Value;
}

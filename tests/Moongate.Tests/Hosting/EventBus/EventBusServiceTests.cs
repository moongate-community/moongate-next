using Microsoft.Extensions.DependencyInjection;
using Moongate.Abstractions.Interfaces.EventHandlers;
using Moongate.Abstractions.Interfaces.Events;
using Moongate.Server.Services.EventBus;
using Moongate.Tests.Hosting.EventBus.Support;

namespace Moongate.Tests.Hosting.EventBus;

public class EventBusServiceTests
{
    [Fact]
    public void DrainTickEvents_BoundedByMaxItems()
    {
        var timeline = new List<string>();
        var bus = BuildBus(services =>
            {
                services.AddSingleton<ITickEventHandler<TestTickEvent>>(_ => new TimelineTickHandler("A", timeline));
            }
        );

        for (var i = 0; i < 5; i++)
        {
            bus.Publish(new TestTickEvent(i));
        }

        var processed = bus.DrainTickEvents(2);

        Assert.Equal(2, processed);
        Assert.Equal(3, bus.CurrentTickQueueDepth);
        Assert.Equal(new[] { "tick:A:0", "tick:A:1" }, timeline);
    }

    [Fact]
    public void DrainTickEvents_DispatchesQueuedEvents()
    {
        var timeline = new List<string>();
        var bus = BuildBus(services =>
            {
                services.AddSingleton<ITickEventHandler<TestTickEvent>>(_ => new TimelineTickHandler("A", timeline));
            }
        );

        bus.Publish(new TestTickEvent(1));
        bus.Publish(new TestTickEvent(2));
        bus.Publish(new TestTickEvent(3));

        var processed = bus.DrainTickEvents(100);

        Assert.Equal(3, processed);
        Assert.Equal(new[] { "tick:A:1", "tick:A:2", "tick:A:3" }, timeline);
        Assert.Equal(0, bus.CurrentTickQueueDepth);
    }

    [Fact]
    public void DrainTickEvents_HandlerThrows_FiresOnEventErrorAndContinues()
    {
        var timeline = new List<string>();
        var bus = BuildBus(services =>
            {
                services.AddSingleton<ITickEventHandler<TestTickEvent>>(_ => new ThrowingTickHandler());
                services.AddSingleton<ITickEventHandler<TestTickEvent>>(_ => new TimelineTickHandler("survivor", timeline));
            }
        );

        Type? errorHandler = null;
        bus.OnEventError = (h, _, _) => errorHandler = h;

        bus.Publish(new TestTickEvent(7));
        var processed = bus.DrainTickEvents(10);

        Assert.Equal(1, processed);
        Assert.Equal(new[] { "tick:survivor:7" }, timeline);
        Assert.Equal(typeof(ThrowingTickHandler), errorHandler);
    }

    [Fact]
    public void Publish_ConcurrentFromManyThreads_EnqueuesEveryEvent()
    {
        var bus = BuildBus(_ => { });

        const int threads = 8;
        const int publishesPerThread = 1000;
        var workers = new Thread[threads];

        for (var t = 0; t < threads; t++)
        {
            var local = t;
            workers[t] = new Thread(() =>
                {
                    for (var i = 0; i < publishesPerThread; i++)
                    {
                        bus.Publish(new TestTickEvent(local * 10_000 + i));
                    }
                }
            );
        }

        foreach (var w in workers)
        {
            w.Start();
        }

        foreach (var w in workers)
        {
            w.Join();
        }

        Assert.Equal(threads * publishesPerThread, bus.CurrentTickQueueDepth);
    }

    [Fact]
    public void Publish_Tick_EnqueuesAndDoesNotInvokeSynchronously()
    {
        var timeline = new List<string>();
        var bus = BuildBus(services =>
            {
                services.AddSingleton<ITickEventHandler<TestTickEvent>>(_ => new TimelineTickHandler("A", timeline));
            }
        );

        bus.Publish(new TestTickEvent(42));

        Assert.Empty(timeline);
        Assert.Equal(1, bus.CurrentTickQueueDepth);
    }

    [Fact]
    public async Task PublishAsync_HandlerThrows_ContinuesChainAndFiresOnEventError()
    {
        var timeline = new List<string>();
        var bus = BuildBus(services =>
            {
                services.AddSingleton<IAsyncEventHandler<TestAsyncEvent>>(_ => new ThrowingAsyncHandler());
                services.AddSingleton<IAsyncEventHandler<TestAsyncEvent>>(_ => new TimelineAsyncHandler("survivor", timeline)
                );
            }
        );

        Type? errorHandler = null;
        Exception? errorException = null;
        IMoongateEvent? errorEvent = null;
        bus.OnEventError = (handlerType, ex, evt) =>
        {
            errorHandler = handlerType;
            errorException = ex;
            errorEvent = evt;
        };

        await bus.PublishAsync(new TestAsyncEvent("x"));

        Assert.Equal(new[] { "async:survivor:x" }, timeline);
        Assert.Equal(typeof(ThrowingAsyncHandler), errorHandler);
        Assert.IsType<InvalidOperationException>(errorException);
        Assert.NotNull(errorEvent);
        Assert.Equal("x", Assert.IsType<TestAsyncEvent>(errorEvent).Payload);
    }

    [Fact]
    public async Task PublishAsync_MultipleHandlers_InvokedInRegistrationOrder()
    {
        var timeline = new List<string>();
        var bus = BuildBus(services =>
            {
                services.AddSingleton<IAsyncEventHandler<TestAsyncEvent>>(_ => new TimelineAsyncHandler("first", timeline));
                services.AddSingleton<IAsyncEventHandler<TestAsyncEvent>>(_ => new TimelineAsyncHandler("second", timeline));
            }
        );

        await bus.PublishAsync(new TestAsyncEvent("x"));

        Assert.Equal(new[] { "async:first:x", "async:second:x" }, timeline);
    }

    [Fact]
    public async Task PublishAsync_NoHandlers_CompletesWithoutThrowing()
    {
        var bus = BuildBus(_ => { });

        await bus.PublishAsync(new TestAsyncEvent("hello"));
    }

    [Fact]
    public async Task PublishAsync_SingleHandler_InvokesHandler()
    {
        var timeline = new List<string>();
        var bus = BuildBus(services =>
            {
                services.AddSingleton<IAsyncEventHandler<TestAsyncEvent>>(_ => new TimelineAsyncHandler("A", timeline));
            }
        );

        await bus.PublishAsync(new TestAsyncEvent("x"));

        Assert.Equal(new[] { "async:A:x" }, timeline);
    }

    private static EventBusService BuildBus(Action<ServiceCollection> configure)
    {
        var services = new ServiceCollection();
        configure(services);

        return new EventBusService(services.BuildServiceProvider());
    }
}

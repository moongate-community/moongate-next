using System.Threading.Channels;
using Moongate.Abstractions.Data.Metrics;
using Moongate.Abstractions.Interfaces.Events;
using Moongate.Abstractions.Interfaces.Metrics;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Abstractions.Types.Metrics;
using Moongate.Server.Services.EventBus.Internal;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.EventBus;

/// <summary>
/// Default <see cref="IEventBusService" /> implementation. Routes IAsyncEvent through
/// sequential handler invocation on the calling thread (with await per handler), and
/// queues ITickEvent into a bounded-by-budget channel drained by the game loop.
/// </summary>
public sealed class EventBusService : IEventBusService, IMetricProvider
{
    private readonly ILogger _logger = Log.ForContext<EventBusService>();
    private readonly HandlerRegistry _registry;
    private readonly Channel<TickEnvelope> _tickQueue;

    private int _tickQueueDepth;
    private long _asyncEventsPublished;
    private long _tickEventsPublished;
    private long _handlerErrors;

    public string Prefix => "bus";

    public EventBusService(IServiceProvider serviceProvider)
    {
        _registry = new(serviceProvider);
        _tickQueue = Channel.CreateUnbounded<TickEnvelope>(
            new()
            {
                SingleReader = true,
                SingleWriter = false
            }
        );
    }

    public Action<Type, Exception, IMoongateEvent>? OnEventError { get; set; }

    public int CurrentTickQueueDepth => Volatile.Read(ref _tickQueueDepth);

    public IReadOnlyList<MetricSample> Collect()
        =>
        [
            new(
                "async_events_total",
                Interlocked.Read(ref _asyncEventsPublished),
                MetricType.Counter,
                Help: "Total async events published"
            ),
            new(
                "tick_events_total",
                Interlocked.Read(ref _tickEventsPublished),
                MetricType.Counter,
                Help: "Total tick events enqueued"
            ),
            new(
                "tick_queue_depth",
                Volatile.Read(ref _tickQueueDepth),
                Help: "Current number of tick events queued"
            ),
            new(
                "handler_errors_total",
                Interlocked.Read(ref _handlerErrors),
                MetricType.Counter,
                Help: "Total handler exceptions across async and tick paths"
            )
        ];

    public int DrainTickEvents(int maxItems)
    {
        var processed = 0;

        while (processed < maxItems && _tickQueue.Reader.TryRead(out var envelope))
        {
            Interlocked.Decrement(ref _tickQueueDepth);
            envelope.Dispatch(this);
            processed++;
        }

        return processed;
    }

    public void Publish<TEvent>(TEvent evt)
        where TEvent : ITickEvent
    {
        if (_tickQueue.Writer.TryWrite(new TickEnvelope<TEvent>(evt)))
        {
            Interlocked.Increment(ref _tickQueueDepth);
            Interlocked.Increment(ref _tickEventsPublished);
        }
    }

    public async Task PublishAsync<TEvent>(TEvent evt, CancellationToken cancellationToken = default)
        where TEvent : IAsyncEvent
    {
        Interlocked.Increment(ref _asyncEventsPublished);
        var handlers = _registry.ResolveAsync<TEvent>();

        for (var i = 0; i < handlers.Length; i++)
        {
            try
            {
                await handlers[i].HandleAsync(evt, cancellationToken);
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _handlerErrors);
                _logger.Error(
                    ex,
                    "Async handler {Handler} failed for {Event}",
                    handlers[i].GetType().Name,
                    typeof(TEvent).Name
                );
                OnEventError?.Invoke(handlers[i].GetType(), ex, evt);
            }
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _tickQueue.Writer.TryComplete();

        return Task.CompletedTask;
    }

    internal void InvokeTickHandlers<TEvent>(TEvent evt)
        where TEvent : ITickEvent
    {
        var handlers = _registry.ResolveTick<TEvent>();

        for (var i = 0; i < handlers.Length; i++)
        {
            try
            {
                handlers[i].Handle(evt);
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _handlerErrors);
                _logger.Error(
                    ex,
                    "Tick handler {Handler} failed for {Event}",
                    handlers[i].GetType().Name,
                    typeof(TEvent).Name
                );
                OnEventError?.Invoke(handlers[i].GetType(), ex, evt);
            }
        }
    }
}

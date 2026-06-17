using Moongate.Abstractions.Interfaces.Events;
using Moongate.Abstractions.Interfaces.Services;

namespace Moongate.Tests.Support;

/// <summary>
/// IEventBusService test double that records published tick and async events for assertions.
/// </summary>
public sealed class RecordingEventBusService : IEventBusService
{
    public List<IMoongateEvent> Published { get; } = [];

    public Action<Type, Exception, IMoongateEvent>? OnEventError { get; set; }

    public int CurrentTickQueueDepth => throw new NotSupportedException();

    public int DrainTickEvents(int maxItems)
        => throw new NotSupportedException();

    public void Publish<TEvent>(TEvent evt)
        where TEvent : ITickEvent
        => Published.Add(evt);

    public Task PublishAsync<TEvent>(TEvent evt, CancellationToken cancellationToken = default)
        where TEvent : IAsyncEvent
    {
        Published.Add(evt);

        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken)
        => throw new NotSupportedException();

    public Task StopAsync(CancellationToken cancellationToken)
        => throw new NotSupportedException();
}

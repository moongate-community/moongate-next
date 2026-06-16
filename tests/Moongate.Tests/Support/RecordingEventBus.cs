using Moongate.Abstractions.Interfaces.EventHandlers;
using Moongate.Abstractions.Interfaces.Events;
using Moongate.Abstractions.Interfaces.Services;

namespace Moongate.Tests.Support;

/// <summary>
/// IEventBusService stub that records every published event.
/// <see cref="StartAsync" /> and <see cref="StopAsync" /> return <see cref="Task.CompletedTask" />.
/// All other members throw <see cref="NotSupportedException" />.
/// </summary>
public sealed class RecordingEventBus : IEventBusService
{
    public List<object> Published { get; } = [];

    public Action<Type, Exception, IMoongateEvent>? OnEventError { get; set; }

    public int CurrentTickQueueDepth => throw new NotSupportedException();

    public int DrainTickEvents(int maxItems)
        => throw new NotSupportedException();

    public void Publish<TEvent>(TEvent evt)
        where TEvent : ITickEvent
        => Published.Add(evt!);

    public Task PublishAsync<TEvent>(TEvent evt, CancellationToken cancellationToken = default)
        where TEvent : IAsyncEvent
        => throw new NotSupportedException();

    public Task StartAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}

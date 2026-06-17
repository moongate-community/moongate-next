using Moongate.Abstractions.Interfaces.Events;
using Moongate.Abstractions.Interfaces.Services;

namespace Moongate.Tests.Support;

/// <summary>
///     IEventBusService no-op stub for tests that do not exercise event publishing.
///     All members throw <see cref="NotSupportedException" /> except the property setter.
/// </summary>
public sealed class NoopEventBusService : IEventBusService
{
    public Action<Type, Exception, IMoongateEvent>? OnEventError { get; set; }

    public int CurrentTickQueueDepth => throw new NotSupportedException();

    public int DrainTickEvents(int maxItems)
    {
        throw new NotSupportedException();
    }

    public void Publish<TEvent>(TEvent evt)
        where TEvent : ITickEvent
    {
        throw new NotSupportedException();
    }

    public Task PublishAsync<TEvent>(TEvent evt, CancellationToken cancellationToken = default)
        where TEvent : IAsyncEvent
    {
        throw new NotSupportedException();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }
}

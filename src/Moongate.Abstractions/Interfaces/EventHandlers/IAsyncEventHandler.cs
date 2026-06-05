using Moongate.Abstractions.Interfaces.Events;

namespace Moongate.Abstractions.Interfaces.EventHandlers;

/// <summary>
/// Handles a single <typeparamref name="TEvent" /> on the asynchronous path.
/// </summary>
/// <typeparam name="TEvent">The event type this handler reacts to.</typeparam>
public interface IAsyncEventHandler<in TEvent>
    where TEvent : IAsyncEvent
{
    /// <summary>
    /// Processes <paramref name="evt" /> asynchronously.
    /// </summary>
    /// <param name="evt">The event instance.</param>
    /// <param name="cancellationToken">Cancellation token propagated from the publisher.</param>
    Task HandleAsync(TEvent evt, CancellationToken cancellationToken);
}

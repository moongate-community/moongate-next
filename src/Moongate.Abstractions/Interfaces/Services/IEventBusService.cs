using Moongate.Abstractions.Interfaces.EventHandlers;
using Moongate.Abstractions.Interfaces.Events;

namespace Moongate.Abstractions.Interfaces.Services;

/// <summary>
/// Routes <see cref="IMoongateEvent" /> instances to registered handlers.
/// Async events go to the thread pool; tick events are queued and drained by
/// <see cref="IGameLoopService" /> on the game-loop thread.
/// </summary>
public interface IEventBusService : IMoongateService
{
    /// <summary>
    /// Optional callback invoked after every handler exception, in addition to the structured log entry.
    /// Arguments: handler type that threw, the exception, the event instance.
    /// </summary>
    Action<Type, Exception, IMoongateEvent>? OnEventError { get; set; }

    /// <summary>Current number of tick events queued for the next drain.</summary>
    int CurrentTickQueueDepth { get; }

    /// <summary>
    /// Drains up to <paramref name="maxItems" /> tick events, invoking their handlers on the calling thread.
    /// Intended for the game loop only; do not call from application code.
    /// </summary>
    /// <returns>Number of events actually processed.</returns>
    int DrainTickEvents(int maxItems);

    /// <summary>
    /// Enqueues <paramref name="evt" /> for processing on the next game loop tick.
    /// Returns immediately; handlers run later on the game-loop thread.
    /// </summary>
    void Publish<TEvent>(TEvent evt)
        where TEvent : ITickEvent;

    /// <summary>
    /// Dispatches <paramref name="evt" /> to every registered
    /// <see cref="IAsyncEventHandler{TEvent}" /> sequentially. The returned task
    /// completes when all handlers have finished (or thrown).
    /// </summary>
    Task PublishAsync<TEvent>(TEvent evt, CancellationToken cancellationToken = default)
        where TEvent : IAsyncEvent;
}

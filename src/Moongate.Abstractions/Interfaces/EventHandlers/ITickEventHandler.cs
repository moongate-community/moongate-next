using Moongate.Abstractions.Interfaces.Events;

namespace Moongate.Abstractions.Interfaces.EventHandlers;

/// <summary>
///     Handles a single <typeparamref name="TEvent" /> on the tick path.
/// </summary>
/// <typeparam name="TEvent">The event type this handler reacts to.</typeparam>
/// <remarks>
///     Implementations execute on the game-loop thread. They MUST be synchronous and
///     non-blocking. To trigger async work, publish an <see cref="IAsyncEvent" /> back
///     to the bus (wrap in <c>Task.Run</c> to avoid stealing the loop thread).
/// </remarks>
public interface ITickEventHandler<in TEvent>
    where TEvent : ITickEvent
{
    /// <summary>Processes <paramref name="evt" /> synchronously on the game-loop thread.</summary>
    void Handle(TEvent evt);
}

using Moongate.Abstractions.Interfaces.EventHandlers;

namespace Moongate.Abstractions.Interfaces.Events;

/// <summary>
/// Marker for events dispatched through the deterministic game-loop tick path.
/// </summary>
/// <remarks>
/// Tick events are queued and processed by <see cref="ITickEventHandler{TEvent}" />
/// handlers on the single game-loop thread, in publish order. Handlers MUST be
/// non-blocking; long-running work belongs in an async handler triggered by a
/// follow-up <see cref="IAsyncEvent" />.
/// </remarks>
public interface ITickEvent : IMoongateEvent { }

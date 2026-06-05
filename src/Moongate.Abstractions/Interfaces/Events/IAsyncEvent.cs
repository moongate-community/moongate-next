namespace Moongate.Abstractions.Interfaces.Events;

/// <summary>
/// Marker for events dispatched through the asynchronous multi-task path.
/// </summary>
/// <remarks>
/// Async events are processed by <see cref="IAsyncEventHandler{TEvent}" /> handlers
/// on the .NET thread pool. Handlers run sequentially per event in registration order.
/// </remarks>
public interface IAsyncEvent : IMoongateEvent { }

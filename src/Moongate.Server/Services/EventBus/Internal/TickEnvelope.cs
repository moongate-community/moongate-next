namespace Moongate.Server.Services.EventBus.Internal;

/// <summary>
/// Abstract base of the polymorphic envelope used to type-erase tick events in the queue.
/// Concrete <see cref="TickEnvelope{TEvent}" /> overrides <see cref="Dispatch" /> with a JIT-specialized call.
/// </summary>
internal abstract class TickEnvelope
{
    public abstract void Dispatch(EventBusService bus);
}

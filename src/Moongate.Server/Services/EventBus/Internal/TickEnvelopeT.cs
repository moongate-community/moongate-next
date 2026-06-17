using Moongate.Abstractions.Interfaces.Events;

namespace Moongate.Server.Services.EventBus.Internal;

/// <summary>
///     Generic envelope that preserves <typeparamref name="TEvent" /> through the
///     type-erased channel and dispatches via a virtual call (no reflection).
/// </summary>
internal sealed class TickEnvelope<TEvent> : TickEnvelope
    where TEvent : ITickEvent
{
    private readonly TEvent _evt;

    public TickEnvelope(TEvent evt)
    {
        _evt = evt;
    }

    public override void Dispatch(EventBusService bus)
    {
        bus.InvokeTickHandlers(_evt);
    }
}

using Moongate.Abstractions.Interfaces.Events;

namespace Moongate.Tests.Hosting.EventBus.Support;

internal sealed record TestAsyncEvent : IAsyncEvent
{
    public string Payload { get; }

    public TestAsyncEvent(string payload)
    {
        Payload = payload;
    }
}

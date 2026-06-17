using Moongate.Abstractions.Interfaces.Events;

namespace Moongate.Tests.Hosting.EventBus.Support;

internal sealed record TestAsyncEvent : IAsyncEvent
{
    public TestAsyncEvent(string payload)
    {
        Payload = payload;
    }

    public string Payload { get; }
}

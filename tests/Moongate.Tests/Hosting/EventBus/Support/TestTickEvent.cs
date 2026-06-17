using Moongate.Abstractions.Interfaces.Events;

namespace Moongate.Tests.Hosting.EventBus.Support;

internal sealed record TestTickEvent : ITickEvent
{
    public TestTickEvent(int value)
    {
        Value = value;
    }

    public int Value { get; }
}

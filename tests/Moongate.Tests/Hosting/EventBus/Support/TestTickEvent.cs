using Moongate.Abstractions.Interfaces.Events;

namespace Moongate.Tests.Hosting.EventBus.Support;

internal sealed record TestTickEvent : ITickEvent
{
    public int Value { get; }

    public TestTickEvent(int value)
    {
        Value = value;
    }
}

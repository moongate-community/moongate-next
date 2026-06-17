using Moongate.Abstractions.Interfaces.EventHandlers;

namespace Moongate.Tests.Hosting.EventBus.Support;

internal sealed class TimelineAsyncHandler : IAsyncEventHandler<TestAsyncEvent>
{
    private readonly string _name;
    private readonly List<string> _timeline;

    public TimelineAsyncHandler(string name, List<string> timeline)
    {
        _name = name;
        _timeline = timeline;
    }

    public Task HandleAsync(TestAsyncEvent evt, CancellationToken cancellationToken)
    {
        lock (_timeline)
        {
            _timeline.Add($"async:{_name}:{evt.Payload}");
        }

        return Task.CompletedTask;
    }
}

internal sealed class TimelineTickHandler : ITickEventHandler<TestTickEvent>
{
    private readonly string _name;
    private readonly List<string> _timeline;

    public TimelineTickHandler(string name, List<string> timeline)
    {
        _name = name;
        _timeline = timeline;
    }

    public void Handle(TestTickEvent evt)
    {
        lock (_timeline)
        {
            _timeline.Add($"tick:{_name}:{evt.Value}");
        }
    }
}

using DryIoc;
using Moongate.Abstractions.Interfaces.Timing;
using Moongate.Server.Extensions.Configuration;
using Moongate.Server.Extensions.EventBus;
using Moongate.Server.Extensions.Timing;
using Moongate.Tests.Support;

namespace Moongate.Tests.Hosting.Timing;

public class TimerWheelIntegrationTests : IDisposable
{
    private readonly string _dir = Path.Combine(
        Path.GetTempPath(),
        $"moongate-timerwheel-integration-{Guid.NewGuid():N}"
    );

    private string ConfigPath => Path.Combine(_dir, "moongate.yaml");

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task FullHost_TimerRegisteredAfterStart_FiresThroughGameLoop()
    {
        var container = new Container();
        container.AddMoongateEventBus();
        container.AddMoongateTimerWheel();
        container.AddMoongateConfig(ConfigPath);

        var orchestrator = container.Orchestrator();
        var timers = container.Resolve<ITimerService>();

        await orchestrator.StartAsync(CancellationToken.None);

        var fired = 0;
        timers.RegisterTimer("ping", TimeSpan.FromMilliseconds(50), () => Interlocked.Increment(ref fired));

        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(2);

        while (DateTime.UtcNow < deadline && Volatile.Read(ref fired) == 0)
        {
            await Task.Delay(10);
        }

        await orchestrator.StopAsync(CancellationToken.None);

        Assert.True(Volatile.Read(ref fired) >= 1, "timer should have fired at least once before stop");
    }
}

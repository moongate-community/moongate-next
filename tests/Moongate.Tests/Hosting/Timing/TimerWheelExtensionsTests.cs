using DryIoc;
using Moongate.Abstractions.Data.Timing;
using Moongate.Abstractions.Interfaces.Timing;
using Moongate.Server.Extensions.Configuration;
using Moongate.Server.Extensions.Timing;

namespace Moongate.Tests.Hosting.Timing;

public class TimerWheelExtensionsTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nh-timing-config-{Guid.NewGuid():N}");
    private string Path_ => Path.Combine(_dir, "moongate.yaml");

    [Fact]
    public void AddMoongateTimerWheel_AppliesCustomConfig()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(Path_, "timing:\n  tick_duration: \"00:00:00.0040000\"\n  wheel_size: 1024\n");

        var container = new Container();
        container.AddMoongateTimerWheel();
        container.AddMoongateConfig(Path_);

        var cfg = container.Resolve<TimerWheelConfig>();
        Assert.Equal(TimeSpan.FromMilliseconds(4), cfg.TickDuration);
        Assert.Equal(1024, cfg.WheelSize);
    }

    [Fact]
    public void AddMoongateTimerWheel_DefaultConfig_HasExpectedValues()
    {
        var container = new Container();
        container.AddMoongateTimerWheel();
        container.AddMoongateConfig(Path_);

        var cfg = container.Resolve<TimerWheelConfig>();
        Assert.Equal(TimeSpan.FromMilliseconds(8), cfg.TickDuration);
        Assert.Equal(512, cfg.WheelSize);
    }

    [Fact]
    public void AddMoongateTimerWheel_RegistersServiceAndConfig()
    {
        var container = new Container();
        container.AddMoongateTimerWheel();
        container.AddMoongateConfig(Path_);

        Assert.NotNull(container.Resolve<ITimerService>());
        Assert.NotNull(container.Resolve<TimerWheelConfig>());
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }
}

using Moongate.Abstractions.Data.Timing;
using Moongate.Server.Services.Timing;

namespace Moongate.Tests.Hosting.Timing;

public class TimerWheelServiceTests
{
    [Fact]
    public void CallbackException_DoesNotStopOtherTimers()
    {
        var svc = NewService(8, 8);
        var goodCalls = 0;
        svc.RegisterTimer("bad", TimeSpan.FromMilliseconds(8), () => throw new InvalidOperationException("boom"));
        svc.RegisterTimer("good", TimeSpan.FromMilliseconds(8), () => goodCalls++);

        svc.UpdateTicksDelta(0);
        svc.UpdateTicksDelta(8);

        Assert.Equal(1, goodCalls);
    }

    [Fact]
    public void CancelById_BeforeFire_PreventsCallback()
    {
        var svc = NewService(8, 8);
        var calls = 0;
        var id = svc.RegisterTimer("c", TimeSpan.FromMilliseconds(8), () => calls++);

        svc.UpdateTicksDelta(0);
        svc.UnregisterTimer(id);
        svc.UpdateTicksDelta(16);

        Assert.Equal(0, calls);
    }

    [Fact]
    public void CancelByName_BeforeFire_PreventsAllOfThem()
    {
        var svc = NewService(8, 8);
        var calls = 0;
        svc.RegisterTimer("group", TimeSpan.FromMilliseconds(8), () => calls++);
        svc.RegisterTimer("group", TimeSpan.FromMilliseconds(8), () => calls++);

        svc.UpdateTicksDelta(0);
        svc.UnregisterTimersByName("group");
        svc.UpdateTicksDelta(16);

        Assert.Equal(0, calls);
    }

    [Fact]
    public void CancelFromInsideCallback_PreventsFurtherFires()
    {
        var svc = NewService(8, 8);
        var calls = 0;
        string? id = null;
        id = svc.RegisterTimer(
            "self-cancel",
            TimeSpan.FromMilliseconds(8),
            () =>
            {
                calls++;
                svc.UnregisterTimer(id!);
            },
            repeat: true
        );

        svc.UpdateTicksDelta(0);
        svc.UpdateTicksDelta(8);  // fires, callback cancels itself
        svc.UpdateTicksDelta(64); // should not fire again

        Assert.Equal(1, calls);
    }

    [Fact]
    public void Ctor_NegativeTickDuration_Throws()
    {
        var cfg = new TimerWheelConfig { TickDuration = TimeSpan.FromMilliseconds(-1) };
        Assert.Throws<ArgumentOutOfRangeException>(() => new TimerWheelService(cfg));
    }

    [Fact]
    public void Ctor_ValidConfig_DoesNotThrow()
    {
        var cfg = new TimerWheelConfig { TickDuration = TimeSpan.FromMilliseconds(8), WheelSize = 16 };
        _ = new TimerWheelService(cfg);
    }

    [Fact]
    public void Ctor_ZeroTickDuration_Throws()
    {
        var cfg = new TimerWheelConfig { TickDuration = TimeSpan.Zero };
        Assert.Throws<ArgumentOutOfRangeException>(() => new TimerWheelService(cfg));
    }

    [Fact]
    public void Ctor_ZeroWheelSize_Throws()
    {
        var cfg = new TimerWheelConfig { TickDuration = TimeSpan.FromMilliseconds(8), WheelSize = 0 };
        Assert.Throws<ArgumentOutOfRangeException>(() => new TimerWheelService(cfg));
    }

    [Fact]
    public void Delay_PostponesFirstExecution_ThenIntervalApplies()
    {
        var svc = NewService();
        var fireTimestamps = new List<long>();
        var current = 0L;
        svc.RegisterTimer(
            "delayed",
            TimeSpan.FromMilliseconds(8),
            () => fireTimestamps.Add(current),
            TimeSpan.FromMilliseconds(24),
            true
        );

        svc.UpdateTicksDelta(0);

        for (var step = 8; step <= 48; step += 8)
        {
            current = step;
            svc.UpdateTicksDelta(step);
        }

        // 0..24 ms: no fire. At 24 ms first fire. Then 32, 40, 48.
        Assert.Equal(new[] { 24L, 32L, 40L, 48L }, fireTimestamps);
    }

    [Fact]
    public void LongInterval_WrapsWheelViaRounds()
    {
        // wheel = 8 slots × 8 ms = 64 ms wheel. Interval 200 ms → 25 ticks → 3 rounds + 1 offset.
        var svc = NewService(8, 8);
        var calls = 0;
        svc.RegisterTimer("long", TimeSpan.FromMilliseconds(200), () => calls++);

        svc.UpdateTicksDelta(0);
        svc.UpdateTicksDelta(192);
        Assert.Equal(0, calls);

        svc.UpdateTicksDelta(200);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void OneShot_FiresExactlyOnceAtDueTime()
    {
        var svc = NewService(8, 8);
        var calls = 0;
        svc.RegisterTimer("once", TimeSpan.FromMilliseconds(8), () => calls++);

        svc.UpdateTicksDelta(0);
        svc.UpdateTicksDelta(8);  // due
        svc.UpdateTicksDelta(16); // would re-fire if repeating
        svc.UpdateTicksDelta(24);

        Assert.Equal(1, calls);
    }

    [Fact]
    public void RegisterTimer_DistinctRegistrations_GetDistinctIds()
    {
        var svc = NewService();
        var a = svc.RegisterTimer("x", TimeSpan.FromMilliseconds(8), () => { });
        var b = svc.RegisterTimer("x", TimeSpan.FromMilliseconds(8), () => { });
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void RegisterTimer_EmptyName_Throws()
    {
        var svc = NewService();
        Assert.Throws<ArgumentException>(() => svc.RegisterTimer("", TimeSpan.FromMilliseconds(8), () => { }));
    }

    [Fact]
    public void RegisterTimer_NonPositiveInterval_Throws()
    {
        var svc = NewService();
        Assert.Throws<ArgumentOutOfRangeException>(() => svc.RegisterTimer("x", TimeSpan.Zero, () => { }));
    }

    [Fact]
    public void RegisterTimer_NullCallback_Throws()
    {
        var svc = NewService();
        Assert.Throws<ArgumentNullException>(() => svc.RegisterTimer("x", TimeSpan.FromMilliseconds(8), null!));
    }

    [Fact]
    public void RegisterTimer_ReturnsNonEmptyId()
    {
        var svc = NewService();
        var id = svc.RegisterTimer("x", TimeSpan.FromMilliseconds(8), () => { });
        Assert.False(string.IsNullOrEmpty(id));
    }

    [Fact]
    public void Repeating_FiresEveryInterval()
    {
        var svc = NewService(8, 8);
        var calls = 0;
        svc.RegisterTimer("rep", TimeSpan.FromMilliseconds(16), () => calls++, repeat: true);

        svc.UpdateTicksDelta(0);
        svc.UpdateTicksDelta(80); // 80/8 = 10 ticks → fires at 16, 32, 48, 64, 80 = 5 times

        Assert.Equal(5, calls);
    }

    [Fact]
    public async Task StopAsync_ClearsState()
    {
        var svc = NewService();
        svc.RegisterTimer("a", TimeSpan.FromMilliseconds(8), () => { });
        svc.RegisterTimer("b", TimeSpan.FromMilliseconds(8), () => { });

        await svc.StopAsync(CancellationToken.None);

        Assert.Equal(0, svc.UnregisterTimersByName("a"));
        Assert.Equal(0, svc.UnregisterTimersByName("b"));
    }

    [Fact]
    public void UnregisterAllTimers_ClearsState()
    {
        var svc = NewService();
        svc.RegisterTimer("a", TimeSpan.FromMilliseconds(8), () => { });
        svc.RegisterTimer("b", TimeSpan.FromMilliseconds(8), () => { });

        svc.UnregisterAllTimers();

        Assert.Equal(0, svc.UnregisterTimersByName("a"));
        Assert.Equal(0, svc.UnregisterTimersByName("b"));
    }

    [Fact]
    public void UnregisterTimer_EmptyId_ReturnsFalse()
    {
        var svc = NewService();
        Assert.False(svc.UnregisterTimer(""));
    }

    [Fact]
    public void UnregisterTimer_ExistingId_ReturnsTrue()
    {
        var svc = NewService();
        var id = svc.RegisterTimer("x", TimeSpan.FromMilliseconds(8), () => { });
        Assert.True(svc.UnregisterTimer(id));
    }

    [Fact]
    public void UnregisterTimer_UnknownId_ReturnsFalse()
    {
        var svc = NewService();
        Assert.False(svc.UnregisterTimer("nope"));
    }

    [Fact]
    public void UnregisterTimersByName_RemovesEveryTimerWithThatName()
    {
        var svc = NewService();
        svc.RegisterTimer("group", TimeSpan.FromMilliseconds(8), () => { });
        svc.RegisterTimer("group", TimeSpan.FromMilliseconds(8), () => { });
        svc.RegisterTimer("other", TimeSpan.FromMilliseconds(8), () => { });

        var removed = svc.UnregisterTimersByName("group");

        Assert.Equal(2, removed);
        Assert.Equal(0, svc.UnregisterTimersByName("group"));
        Assert.Equal(1, svc.UnregisterTimersByName("other"));
    }

    [Fact]
    public void UpdateTicksDelta_AdvancesByWholeTicks()
    {
        var svc = NewService();
        svc.UpdateTicksDelta(0);
        var processed = svc.UpdateTicksDelta(24); // 24/8 = 3 ticks
        Assert.Equal(3, processed);
    }

    [Fact]
    public void UpdateTicksDelta_FirstCall_InitializesAndReturnsZero()
    {
        var svc = NewService();
        Assert.Equal(0, svc.UpdateTicksDelta(1000));
    }

    [Fact]
    public void UpdateTicksDelta_NegativeTimestamp_Throws()
    {
        var svc = NewService();
        Assert.Throws<ArgumentOutOfRangeException>(() => svc.UpdateTicksDelta(-1));
    }

    [Fact]
    public void UpdateTicksDelta_PartialTick_DoesNotAdvance()
    {
        var svc = NewService();
        svc.UpdateTicksDelta(0);
        Assert.Equal(0, svc.UpdateTicksDelta(7)); // <8 ms
    }

    private static TimerWheelService NewService(int tickDurationMs = 8, int wheelSize = 16)
    {
        return new TimerWheelService(
            new TimerWheelConfig
            {
                TickDuration = TimeSpan.FromMilliseconds(tickDurationMs),
                WheelSize = wheelSize
            }
        );
    }
}

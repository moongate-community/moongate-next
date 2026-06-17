using Moongate.Abstractions.Data.Timing;
using Moongate.Abstractions.Types.Jobs;
using Moongate.Server.Services.Jobs;
using Moongate.Server.Services.Timing;

namespace Moongate.Tests.Server.Jobs;

public sealed class JobServiceTests
{
    [Fact]
    public void Cancel_RemovesJob()
    {
        var jobs = new JobService(NewWheel());
        var id = jobs.RegisterRecurring("x", TimeSpan.FromSeconds(1), () => { });

        Assert.True(jobs.Cancel(id));
        Assert.Empty(jobs.GetJobs());
        Assert.False(jobs.Cancel(id));
    }

    [Fact]
    public void FailingHandler_RecordsFailed_AndKeepsJobRegistered()
    {
        var wheel = NewWheel();
        var jobs = new JobService(wheel);

        jobs.RegisterRecurring("bad", TimeSpan.FromMilliseconds(8), () => throw new InvalidOperationException("boom"));

        wheel.UpdateTicksDelta(0);
        wheel.UpdateTicksDelta(8);

        var snap = Assert.Single(jobs.GetJobs());
        Assert.Equal(JobStatusType.Failed, snap.LastStatus);
        Assert.Contains("boom", snap.LastError);
        Assert.Equal(1, snap.RunCount);
    }

    [Fact]
    public void RegisterOnce_DoesNotRepeat()
    {
        var jobs = new JobService(NewWheel());

        jobs.RegisterOnce("once", TimeSpan.FromMilliseconds(8), () => { });

        Assert.False(Assert.Single(jobs.GetJobs()).Repeat);
    }

    [Fact]
    public void RegisterOnce_FiresOnce_AndStopsRepeating()
    {
        var wheel = NewWheel();
        var jobs = new JobService(wheel);
        var runs = 0;

        jobs.RegisterOnce("once", TimeSpan.FromMilliseconds(8), () => runs++);

        wheel.UpdateTicksDelta(0);
        wheel.UpdateTicksDelta(8);
        wheel.UpdateTicksDelta(16);
        wheel.UpdateTicksDelta(24);

        Assert.Equal(1, runs);
    }

    [Fact]
    public void RegisterRecurring_FiresAndRecordsSuccessMetadata()
    {
        var wheel = NewWheel();
        var jobs = new JobService(wheel);
        var runs = 0;

        jobs.RegisterRecurring("save", TimeSpan.FromMilliseconds(8), () => runs++, "world save");

        wheel.UpdateTicksDelta(0);
        wheel.UpdateTicksDelta(8);

        Assert.Equal(1, runs);
        var snap = Assert.Single(jobs.GetJobs());
        Assert.Equal("save", snap.Name);
        Assert.Equal("world save", snap.Description);
        Assert.Equal(JobSourceType.CSharp, snap.Source);
        Assert.True(snap.Repeat);
        Assert.Equal(1, snap.RunCount);
        Assert.Equal(JobStatusType.Success, snap.LastStatus);
        Assert.NotNull(snap.LastRunAt);
        Assert.NotNull(snap.NextRunAt);
    }

    [Fact]
    public void RunNow_SchedulesImmediateExtraRun()
    {
        var wheel = NewWheel();
        var jobs = new JobService(wheel);
        var runs = 0;
        var id = jobs.RegisterRecurring("rare", TimeSpan.FromSeconds(10), () => runs++);

        Assert.True(jobs.RunNow(id));

        wheel.UpdateTicksDelta(0);
        wheel.UpdateTicksDelta(8);

        Assert.Equal(1, runs);
    }

    [Fact]
    public void RunNow_UnknownId_ReturnsFalse()
    {
        Assert.False(new JobService(NewWheel()).RunNow("nope"));
    }

    [Fact]
    public void Source_IsRecorded()
    {
        var jobs = new JobService(NewWheel());

        jobs.RegisterRecurring("lua-job", TimeSpan.FromSeconds(1), () => { }, source: JobSourceType.Lua);

        Assert.Equal(JobSourceType.Lua, Assert.Single(jobs.GetJobs()).Source);
    }

    private static TimerWheelService NewWheel()
    {
        return new TimerWheelService(new TimerWheelConfig { TickDuration = TimeSpan.FromMilliseconds(8), WheelSize = 16 });
    }
}

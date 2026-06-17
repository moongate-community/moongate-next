using Moongate.Server.Data.World.Internal;

namespace Moongate.Tests.Server.World;

public sealed class LightCycleTests
{
    [Theory]
    [InlineData(3, 0, 12)]
    [InlineData(12, 0, 0)]
    [InlineData(5, 0, 6)]
    [InlineData(23, 0, 6)]
    [InlineData(6, 0, 0)]
    [InlineData(22, 0, 0)]
    [InlineData(0, 0, 12)]

    // night
    // day
    // dawn midpoint: 12 + (60)*(0-12)/120 = 6
    // dusk midpoint: 0 + (60)*(12-0)/120 = 6
    // dawn complete: full day
    // dusk start: still full day
    // pre-dawn night
    public void LevelFromHourMinute_FollowsDayNightCurve(int hour, int minute, int expected)
    {
        Assert.Equal(expected, LightCycle.LevelFromHourMinute(hour, minute));
    }

    [Fact]
    public void TimeOfDay_NormalizesTo24HoursAndWrapsNegative()
    {
        Assert.Equal((1, 30, 0), LightCycle.TimeOfDay(90));
        Assert.Equal((0, 0, 0), LightCycle.TimeOfDay(1440));
        Assert.Equal((23, 59, 0), LightCycle.TimeOfDay(-1));
    }

    [Fact]
    public void TotalUoMinutes_AcceleratesByConfiguredRate()
    {
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var now = start.AddSeconds(300); // 300 / 5 = 60 uo-minutes

        Assert.Equal(60d, LightCycle.TotalUoMinutes(now, start, 5.0), 6);
    }
}

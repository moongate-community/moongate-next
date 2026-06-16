namespace Moongate.Server.Data.World.Internal;

/// <summary>
/// Pure day/night light-cycle math and the accelerated UO world clock used by the light/time service.
/// </summary>
public static class LightCycle
{
    public const int DayLevel = 0;
    public const int NightLevel = 12;

    private const int MinutesPerDay = 24 * 60;

    /// <summary>Light level (0 = brightest day, higher = darker) for a UO time-of-day.</summary>
    public static int LevelFromHourMinute(int hour, int minute)
        => hour switch
        {
            < 4  => NightLevel,
            < 6  => NightLevel + ((hour - 4) * 60 + minute) * (DayLevel - NightLevel) / 120,
            < 22 => DayLevel,
            < 24 => DayLevel + ((hour - 22) * 60 + minute) * (NightLevel - DayLevel) / 120,
            _    => NightLevel
        };

    /// <summary>Normalizes total UO minutes to a 24-hour time-of-day (wraps negatives).</summary>
    public static (int Hour, int Minute, int Second) TimeOfDay(double totalUoMinutes)
    {
        var whole = (long)Math.Floor(totalUoMinutes);
        var normalized = (int)((whole % MinutesPerDay + MinutesPerDay) % MinutesPerDay);
        var second = (int)((totalUoMinutes - whole) * 60);

        return (normalized / 60, normalized % 60, second);
    }

    /// <summary>Elapsed UO minutes since the world start, given the real-seconds-per-UO-minute rate.</summary>
    public static double TotalUoMinutes(DateTime utcNow, DateTime worldStartUtc, double secondsPerUoMinute)
        => (utcNow - worldStartUtc).TotalSeconds / secondsPerUoMinute;
}

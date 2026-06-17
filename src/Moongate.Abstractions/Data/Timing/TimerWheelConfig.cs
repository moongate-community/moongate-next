namespace Moongate.Abstractions.Data.Timing;

/// <summary>
///     Configuration for the hashed timer wheel.
/// </summary>
public sealed class TimerWheelConfig
{
    /// <summary>
    ///     Wheel granularity. Timers cannot be shorter than this; accuracy is ±<see cref="TickDuration" />.
    ///     Default 8 ms.
    /// </summary>
    public TimeSpan TickDuration { get; set; } = TimeSpan.FromMilliseconds(8);

    /// <summary>
    ///     Number of slots in the wheel. Power of 2 recommended for fast modulo. Default 512.
    /// </summary>
    public int WheelSize { get; set; } = 512;
}

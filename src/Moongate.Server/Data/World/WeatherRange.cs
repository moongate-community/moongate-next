namespace Moongate.Server.Data.World;

/// <summary>
/// Represents a minimum and maximum weather intensity range.
/// </summary>
public readonly record struct WeatherRange
{
    public int Min { get; }

    public int Max { get; }

    public WeatherRange(int min, int max)
    {
        Min = min;
        Max = max;
    }
}

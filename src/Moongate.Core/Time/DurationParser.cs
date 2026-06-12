using System.Globalization;

namespace Moongate.Core.Time;

public static class DurationParser
{
    public static TimeSpan Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var trimmed = value.Trim();
        var unit = char.ToLowerInvariant(trimmed[^1]);

        if (!char.IsLetter(unit))
        {
            return TimeSpan.Parse(trimmed, CultureInfo.InvariantCulture);
        }

        var numberText = trimmed[..^1];

        if (!double.TryParse(numberText, NumberStyles.Float, CultureInfo.InvariantCulture, out var amount))
        {
            return TimeSpan.Parse(trimmed, CultureInfo.InvariantCulture);
        }

        return unit switch
        {
            's' => TimeSpan.FromSeconds(amount),
            'm' => TimeSpan.FromMinutes(amount),
            'h' => TimeSpan.FromHours(amount),
            'd' => TimeSpan.FromDays(amount),
            _   => TimeSpan.Parse(trimmed, CultureInfo.InvariantCulture)
        };
    }
}

using System.Globalization;
using Moongate.UO.Data.Interfaces.Hues;

namespace Moongate.Server.Data.Templates;

public sealed record HueColorSummary(int Index, byte R, byte G, byte B, string Hex);

public sealed record HueSummary(
    int Value,
    string Hex,
    string Name,
    bool IsNone,
    bool IsKnown,
    IReadOnlyList<HueColorSummary> Colors
)
{
    public static string FormatHue(int value)
    {
        return $"0x{value.ToString("X3", CultureInfo.InvariantCulture)}";
    }

    public static HueSummary FromValue(int value, IHueStore hues)
    {
        ArgumentNullException.ThrowIfNull(hues);

        if (value == 0)
        {
            return new HueSummary(0, FormatHue(value), "None", true, true, []);
        }

        var hue = hues.GetHue(value - 1);

        if (hue is null)
        {
            return new HueSummary(value, FormatHue(value), "Unknown hue", false, false, []);
        }

        var colors = new List<HueColorSummary>(hue.Colors.Length);

        for (var index = 0; index < hue.Colors.Length; index++)
        {
            var (r, g, b) = hue.GetRgb(index);
            colors.Add(new HueColorSummary(index, r, g, b, $"#{r:X2}{g:X2}{b:X2}"));
        }

        return new HueSummary(value, FormatHue(value), hue.Name, false, true, colors);
    }
}

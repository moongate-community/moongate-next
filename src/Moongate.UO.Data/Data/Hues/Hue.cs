using Moongate.UO.Data.Internal.Colors;

namespace Moongate.UO.Data.Data.Hues;

/// <summary>
/// A single UO hue: 32 RGB555 gradient colours, the table start/end shade indices, and its name.
/// </summary>
public sealed class Hue
{
    public Hue(ushort[] colors, ushort tableStart, ushort tableEnd, string name)
    {
        Colors = colors;
        TableStart = tableStart;
        TableEnd = tableEnd;
        Name = name;
    }

    public ushort[] Colors { get; }

    public ushort TableStart { get; }

    public ushort TableEnd { get; }

    public string Name { get; }

    public (byte R, byte G, byte B) GetRgb(int colorIndex)
    {
        return Rgb555.ToRgb(Colors[colorIndex]);
    }
}

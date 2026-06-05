namespace Moongate.UO.Data.Internal.Colors;

/// <summary>
/// Converts a 15-bit RGB555 colour (as stored in UO mul files) to 8-bit-per-channel RGB.
/// </summary>
public static class Rgb555
{
    public static (byte R, byte G, byte B) ToRgb(ushort color)
    {
        var r = (color >> 10) & 0x1F;
        var g = (color >> 5) & 0x1F;
        var b = color & 0x1F;

        return (
                   (byte)((r << 3) | (r >> 2)),
                   (byte)((g << 3) | (g >> 2)),
                   (byte)((b << 3) | (b >> 2))
               );
    }
}

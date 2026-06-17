using Moongate.UO.Data.Data.Hues;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.UO.Data.Animations;

/// <summary>
///     Applies a UO <see cref="Hue" /> to a decoded body frame: every opaque gray pixel (R==G==B) is replaced
///     by the hue's 32-colour ramp entry indexed by its 5-bit shade (<c>R &gt;&gt; 3</c>), preserving alpha.
///     Non-gray and transparent pixels are left untouched (partial hue).
/// </summary>
public static class HueApplier
{
    public static void Apply(Image<Rgba32> image, Hue hue)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(hue);

        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                var pixel = image[x, y];

                if (pixel.A == 0 || pixel.R != pixel.G || pixel.G != pixel.B)
                {
                    continue;
                }

                var (r, g, b) = hue.GetRgb(pixel.R >> 3);
                image[x, y] = new Rgba32(r, g, b, pixel.A);
            }
        }
    }
}

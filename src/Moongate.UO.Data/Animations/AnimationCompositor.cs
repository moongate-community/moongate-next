using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.UO.Data.Animations;

/// <summary>
///     Composites decoded animation frames (body, then hair, then facial hair, …) onto a shared canvas,
///     aligned by frame centre, drawing opaque pixels top-over-bottom in list order. The returned canvas is
///     not trimmed (the caller's CropAndPad does that). Does not dispose the input layers.
/// </summary>
public static class AnimationCompositor
{
    public static Image<Rgba32> Compose(IReadOnlyList<DecodedFrame> layers)
    {
        ArgumentNullException.ThrowIfNull(layers);

        if (layers.Count == 0)
        {
            return new Image<Rgba32>(1, 1);
        }

        var topLeftX = new int[layers.Count];
        var topLeftY = new int[layers.Count];

        for (var i = 0; i < layers.Count; i++)
        {
            topLeftX[i] = 0x200 - layers[i].CenterX;
            topLeftY[i] = 0x200 - layers[i].CenterY - layers[i].Image.Height;
        }

        var minX = topLeftX.Min();
        var minY = topLeftY.Min();
        var maxX = int.MinValue;
        var maxY = int.MinValue;

        for (var i = 0; i < layers.Count; i++)
        {
            maxX = Math.Max(maxX, topLeftX[i] + layers[i].Image.Width);
            maxY = Math.Max(maxY, topLeftY[i] + layers[i].Image.Height);
        }

        var canvas = new Image<Rgba32>(maxX - minX, maxY - minY);

        for (var i = 0; i < layers.Count; i++)
        {
            var offsetX = topLeftX[i] - minX;
            var offsetY = topLeftY[i] - minY;
            var image = layers[i].Image;

            for (var y = 0; y < image.Height; y++)
            {
                for (var x = 0; x < image.Width; x++)
                {
                    var pixel = image[x, y];

                    if (pixel.A == 0)
                    {
                        continue;
                    }

                    canvas[offsetX + x, offsetY + y] = pixel;
                }
            }
        }

        return canvas;
    }
}

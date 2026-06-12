using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.UO.Data.Animations;

/// <summary>A decoded animation frame plus its hotspot centre, used to align layers when compositing.</summary>
public sealed class DecodedFrame : IDisposable
{
    public DecodedFrame(Image<Rgba32> image, int centerX, int centerY)
    {
        ArgumentNullException.ThrowIfNull(image);

        Image = image;
        CenterX = centerX;
        CenterY = centerY;
    }

    public Image<Rgba32> Image { get; }

    public int CenterX { get; }

    public int CenterY { get; }

    public void Dispose()
        => Image.Dispose();
}

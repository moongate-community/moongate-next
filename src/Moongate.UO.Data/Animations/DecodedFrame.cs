using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.UO.Data.Animations;

/// <summary>A decoded animation frame plus its hotspot centre, used to align layers when compositing.</summary>
public sealed class DecodedFrame : IDisposable
{
    private readonly Image<Rgba32> _image;
    private readonly int _centerX;
    private readonly int _centerY;

    public DecodedFrame(Image<Rgba32> image, int centerX, int centerY)
    {
        ArgumentNullException.ThrowIfNull(image);

        _image = image;
        _centerX = centerX;
        _centerY = centerY;
    }

    public Image<Rgba32> Image => _image;

    public int CenterX => _centerX;

    public int CenterY => _centerY;

    public void Dispose()
        => _image.Dispose();
}

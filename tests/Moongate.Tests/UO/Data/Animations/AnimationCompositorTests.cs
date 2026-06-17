using Moongate.UO.Data.Animations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.Tests.UO.Data.Animations;

public sealed class AnimationCompositorTests
{
    [Fact]
    public void Compose_AlignsByCentre_AndOverlaysOpaquePixels()
    {
        // base: 2x2 all red, centre (520,520) -> top-left shared (512-520, 512-520-2) = (-8,-10)
        var baseImg = new Image<Rgba32>(2, 2);

        for (var y = 0; y < 2; y++)
        {
            for (var x = 0; x < 2; x++)
            {
                baseImg[x, y] = new Rgba32(255, 0, 0, 255);
            }
        }

        // overlay: 1x1 green, centre (520,520), height 1 -> top-left (-8, 512-520-1) = (-8,-9)
        var overlay = new Image<Rgba32>(1, 1);
        overlay[0, 0] = new Rgba32(0, 255, 0, 255);

        using var result =
            AnimationCompositor.Compose([new DecodedFrame(baseImg, 520, 520), new DecodedFrame(overlay, 520, 520)]);

        Assert.Equal(2, result.Width);
        Assert.Equal(2, result.Height);
        Assert.Equal(new Rgba32(0, 255, 0, 255), result[0, 1]); // overlay landed here
        Assert.Equal(new Rgba32(255, 0, 0, 255), result[0, 0]); // base elsewhere
        Assert.Equal(new Rgba32(255, 0, 0, 255), result[1, 1]);
    }

    [Fact]
    public void Compose_Empty_ReturnsTinyImage()
    {
        using var result = AnimationCompositor.Compose([]);

        Assert.Equal(1, result.Width);
        Assert.Equal(1, result.Height);
    }
}

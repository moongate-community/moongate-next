using Moongate.UO.Data.Animations;
using Moongate.UO.Data.Data.Hues;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.Tests.UO.Data.Animations;

public sealed class HueApplierTests
{
    private static Hue BuildHue()
    {
        var colors = new ushort[32];
        colors[0] = 0x7C00;   // shade 0 -> red   (R5=31)
        colors[31] = 0x03E0;  // shade 31 -> green (G5=31)

        return new Hue(colors, 0, 0, "test");
    }

    [Fact]
    public void Apply_TintsGrayPixels_LeavesOthers()
    {
        using var image = new Image<Rgba32>(2, 2);
        image[0, 0] = new Rgba32(248, 248, 248, 255); // gray, shade 31 (248>>3 = 31)
        image[1, 0] = new Rgba32(0, 0, 0, 255);       // gray, shade 0
        image[0, 1] = new Rgba32(10, 20, 30, 255);    // non-gray -> untouched
        image[1, 1] = new Rgba32(0, 0, 0, 0);         // transparent -> untouched

        HueApplier.Apply(image, BuildHue());

        Assert.Equal(new Rgba32(0, 255, 0, 255), image[0, 0]); // ramp[31] = green
        Assert.Equal(new Rgba32(255, 0, 0, 255), image[1, 0]); // ramp[0] = red
        Assert.Equal(new Rgba32(10, 20, 30, 255), image[0, 1]); // unchanged
        Assert.Equal(0, image[1, 1].A);                         // unchanged (transparent)
    }
}

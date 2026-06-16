using Moongate.UO.Data.Gumps;

namespace Moongate.Tests.UO.Data.Gumps;

public sealed class GumpDecoderTests
{
    [Fact]
    public void Decode_TransparentAndOpaqueRuns()
    {
        using var image = GumpDecoder.Decode(BuildGump(), 2, 1);

        Assert.Equal(2, image.Width);
        Assert.Equal(1, image.Height);
        Assert.Equal(0, image[0, 0].A);   // transparent
        Assert.Equal(255, image[1, 0].A); // opaque
        Assert.Equal(255, image[1, 0].R); // red
        Assert.Equal(0, image[1, 0].G);
        Assert.Equal(0, image[1, 0].B);
    }

    [Fact]
    public void Decode_TruncatedData_DoesNotThrow()
    {
        var truncated = BuildGump()[..6]; // cut mid-row

        using var image = GumpDecoder.Decode(truncated, 2, 1);

        Assert.Equal(2, image.Width);
    }

    [Fact]
    public void Decode_ZeroSize_ReturnsMinimalTransparentImage()
    {
        using var image = GumpDecoder.Decode([], 0, 0);

        Assert.True(image.Width >= 1);
        Assert.Equal(0, image[0, 0].A);
    }

    // A 2x1 gump: row-lookup table (1 dword = row 0 at dword offset 1),
    // then row 0 RLE: (color 0x0000, run 1) = transparent, (color 0x7C00, run 1) = red.
    private static byte[] BuildGump()
        =>
        [
            0x01, 0x00, 0x00, 0x00, // lookup[0] = 1 (dwords) -> byte offset 4
            0x00, 0x00, 0x01, 0x00, // color 0x0000, run 1  (transparent)
            0x00, 0x7C, 0x01, 0x00  // color 0x7C00, run 1  (red)
        ];
}

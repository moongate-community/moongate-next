using Moongate.UO.Data.Animations;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.Tests.UO.Data.Animations;

public sealed class AnimationFrameDecoderTests
{
    [Fact]
    public void Decode_FrameOutOfRange_ReturnsNull()
    {
        Assert.Null(AnimationFrameDecoder.Decode(BuildBlock(), 5));
    }

    [Fact]
    public void Decode_PaintsRunPixels_RestTransparent()
    {
        using var image = AnimationFrameDecoder.Decode(BuildBlock(), 0);

        Assert.NotNull(image);
        Assert.Equal(2, image!.Width);
        Assert.Equal(2, image.Height);
        Assert.Equal(new Rgba32(255, 0, 0, 255), image[0, 0]);
        Assert.Equal(new Rgba32(255, 0, 0, 255), image[1, 0]);
        Assert.Equal(0, image[0, 1].A); // uncovered -> transparent
        Assert.Equal(0, image[1, 1].A);
    }

    [Fact]
    public void Decode_ZeroSizeFrame_ReturnsNull()
    {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);

        for (var i = 0; i < 256; i++)
        {
            w.Write((ushort)0);
        }

        w.Write(1);
        w.Write(8);
        w.Write((short)0);
        w.Write((short)0);
        w.Write((short)0);
        w.Write((short)0); // width=height=0
        var block = ms.ToArray();

        Assert.Null(AnimationFrameDecoder.Decode(block, 0));
    }

    [Fact]
    public void DecodeFrame_ReturnsImageAndCenters()
    {
        using var decoded = AnimationFrameDecoder.DecodeFrame(BuildBlock(), 0);

        Assert.NotNull(decoded);
        Assert.Equal(0x200, decoded!.CenterX);
        Assert.Equal(0x200 - 2, decoded.CenterY);
        Assert.Equal(2, decoded.Image.Width);
        Assert.Equal(new Rgba32(255, 0, 0, 255), decoded.Image[0, 0]); // same content as Decode
    }

    private static byte[] BuildBlock()
    {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);

        // 256-entry ARGB1555 palette; index 1 = red (R=31 -> bits 10..14 -> 0x7C00).
        for (var i = 0; i < 256; i++)
        {
            w.Write((ushort)(i == 1 ? 0x7C00 : 0x0000));
        }

        // start = position after palette (= 512). frameCount, then offsets relative to start.
        w.Write(1); // frameCount
        w.Write(8); // frame[0] offset: lookup table is 4 (count) + 4 (one offset) = 8 bytes after start

        // frame[0] at start+8
        w.Write((short)0x200);       // xCenter -> xBase = xCenter - 0x200 = 0
        w.Write((short)(0x200 - 2)); // yCenter -> yBase = yCenter + height - 0x200 = 0
        w.Write((short)2);           // width
        w.Write((short)2);           // height
        w.Write(0x80200002);         // run header: xOffset=0, yOffset=0, runLength=2 (XOR 0x80200000)
        w.Write((byte)1);            // pixel (0,0) -> palette[1] red
        w.Write((byte)1);            // pixel (1,0) -> palette[1] red
        w.Write(0x7FFF7FFF);         // terminator

        return ms.ToArray();
    }
}

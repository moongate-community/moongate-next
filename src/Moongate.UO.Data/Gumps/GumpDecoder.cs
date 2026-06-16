using System.Buffers.Binary;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.UO.Data.Gumps;

/// <summary>Decodes a single UO gump (run-length encoded ARGB1555 scanlines) into an image.</summary>
public static class GumpDecoder
{
    /// <summary>
    /// Decodes gump pixel <paramref name="data" /> of the given <paramref name="width" />/<paramref name="height" />.
    /// Layout: <paramref name="height" /> little-endian uint row offsets (in dwords from the data start),
    /// then per row a sequence of (color: u16, run: u16) pairs; color 0 is transparent, otherwise opaque.
    /// Tolerates truncated/short data by leaving the remaining pixels transparent.
    /// </summary>
    public static Image<Rgba32> Decode(ReadOnlySpan<byte> data, int width, int height)
    {
        var image = new Image<Rgba32>(Math.Max(1, width), Math.Max(1, height));

        if (width <= 0 || height <= 0)
        {
            return image;
        }

        for (var y = 0; y < height; y++)
        {
            var lookupPos = y * 4;

            if (lookupPos + 4 > data.Length)
            {
                break;
            }

            var rowOffsetDwords = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(lookupPos, 4));
            var pos = (int)Math.Min((long)rowOffsetDwords * 4, data.Length);
            var x = 0;

            while (x < width)
            {
                if (pos + 4 > data.Length)
                {
                    break;
                }

                var color = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(pos, 2));
                var run = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(pos + 2, 2));
                pos += 4;

                if (run == 0)
                {
                    break;
                }

                var pixel = color == 0 ? default : ToRgba(color);

                for (var i = 0; i < run && x < width; i++)
                {
                    image[x, y] = pixel;
                    x++;
                }
            }
        }

        return image;
    }

    private static Rgba32 ToRgba(ushort value)
    {
        var r = (byte)(((value >> 10) & 0x1F) * 255 / 31);
        var g = (byte)(((value >> 5) & 0x1F) * 255 / 31);
        var b = (byte)((value & 0x1F) * 255 / 31);

        return new(r, g, b, 255);
    }
}

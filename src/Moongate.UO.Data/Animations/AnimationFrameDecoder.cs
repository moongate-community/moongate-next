using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.UO.Data.Animations;

/// <summary>
/// Decodes a single frame from a UO <c>anim.mul</c> body-action-direction block (ARGB1555 palette + RLE)
/// into an <see cref="Image{Rgba32}" />. Returns <c>null</c> for an out-of-range or empty frame.
/// </summary>
public static class AnimationFrameDecoder
{
    private const int DoubleXor = (0x200 << 22) | (0x200 << 12);
    private const int RleTerminator = 0x7FFF7FFF;

    public static Image<Rgba32>? Decode(byte[] block, int frame)
    {
        ArgumentNullException.ThrowIfNull(block);

        if (block.Length < 512 + 4)
        {
            return null;
        }

        using var stream = new MemoryStream(block, writable: false);
        using var reader = new BinaryReader(stream);

        var palette = new Rgba32[256];

        for (var i = 0; i < 256; i++)
        {
            palette[i] = FromArgb1555(reader.ReadUInt16());
        }

        var start = (int)stream.Position;
        var frameCount = reader.ReadInt32();

        if (frame < 0 || frame >= frameCount)
        {
            return null;
        }

        var offsets = new int[frameCount];

        for (var i = 0; i < frameCount; i++)
        {
            offsets[i] = start + reader.ReadInt32();
        }

        stream.Position = offsets[frame];

        var xCenter = reader.ReadInt16();
        var yCenter = reader.ReadInt16();
        var width = reader.ReadInt16();
        var height = reader.ReadInt16();

        if (width <= 0 || height <= 0)
        {
            return null;
        }

        var image = new Image<Rgba32>(width, height);
        var xBase = xCenter - 0x200;
        var yBase = (yCenter - height) + 0x200;

        int header;

        while ((header = reader.ReadInt32()) != RleTerminator)
        {
            header ^= DoubleXor;

            var xOffset = (header >> 22) & 0x3FF;
            var yOffset = (header >> 12) & 0x3FF;
            var runLength = header & 0xFFF;

            var x = xBase + xOffset;
            var y = yBase + yOffset;

            for (var i = 0; i < runLength; i++)
            {
                var paletteIndex = reader.ReadByte();
                var px = x + i;

                if (px >= 0 && px < width && y >= 0 && y < height)
                {
                    image[px, y] = palette[paletteIndex];
                }
            }
        }

        return image;
    }

    private static Rgba32 FromArgb1555(ushort value)
    {
        return new Rgba32(Expand5To8((value >> 10) & 0x1F), Expand5To8((value >> 5) & 0x1F), Expand5To8(value & 0x1F), 255);
    }

    // 5-bit -> 8-bit channel expansion via bit replication, so 0x1F maps to 0xFF (full range).
    private static byte Expand5To8(int value)
        => (byte)((value << 3) | (value >> 2));
}

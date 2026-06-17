using System.Buffers.Binary;
using System.IO.Compression;
using Moongate.UO.Data.Files;
using Moongate.UO.Data.Interfaces.Files;
using Moongate.UO.Data.Interfaces.Gumps;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.UO.Data.Gumps;

/// <summary>
///     Reads and decodes gumps via a <see cref="FileIndex" />, supporting both the legacy
///     <c>gumpidx.mul</c>/<c>gumpart.mul</c> pair (dimensions come from the index <c>extra</c>, data is raw)
///     and the modern <c>gumpartLegacyMUL.uop</c> (zlib-compressed; the decompressed data is prefixed with
///     the width/height as two little-endian <c>uint</c>s, followed by the row-lookup table + RLE).
/// </summary>
public sealed class GumpStore : IGumpStore
{
    private const int GumpIndexLength = 0x10000;
    private const int GumpFileId = 12;

    private readonly FileIndex _index;
    private readonly Lock _sync = new();

    public GumpStore(IUoFileResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);

        _index = new FileIndex(
            resolver.Resolve("gumpidx.mul"),
            resolver.Resolve("gumpart.mul"),
            resolver.Resolve("gumpartLegacyMUL.uop"),
            GumpIndexLength,
            GumpFileId,
            ".tga",
            GumpIndexLength,
            false,
            new NullVerdataPatchSource()
        );
    }

    public Image<Rgba32>? GetGump(int gumpId)
    {
        if (gumpId < 0 || gumpId >= GumpIndexLength)
        {
            return null;
        }

        lock (_sync)
        {
            var stream = _index.Seek(gumpId, out var length, out var extra, out _);

            if (stream is null || length <= 0)
            {
                return null;
            }

            var raw = ReadExactly(stream, length);

            if (raw is null)
            {
                return null;
            }

            // Legacy .mul path: the index carries (width << 16) | height; the data is the raw gump.
            if (extra > 0)
            {
                var width = (extra >> 16) & 0xFFFF;
                var height = extra & 0xFFFF;

                return width > 0 && height > 0 ? GumpDecoder.Decode(raw, width, height) : null;
            }

            // UOP path: data is zlib-compressed; the decompressed stream is prefixed with width/height
            // (two little-endian uints) and then the standard row-lookup table + RLE.
            return DecodeUop(raw);
        }
    }

    private static Image<Rgba32>? DecodeUop(byte[] compressed)
    {
        byte[] data;

        try
        {
            using var input = new MemoryStream(compressed);
            using var zlib = new ZLibStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            zlib.CopyTo(output);
            data = output.ToArray();
        }
        catch (InvalidDataException)
        {
            return null;
        }

        var direct = TryReadGump(data);

        if (direct is not null)
        {
            return direct;
        }

        byte[] bwt;

        try
        {
            bwt = BwtDecompress.Decompress(data);
        }
        catch
        {
            return null;
        }

        if (bwt.Length == 0)
        {
            return null;
        }

        return TryReadGump(bwt);
    }

    private static byte[]? ReadExactly(Stream stream, int length)
    {
        var buffer = new byte[length];
        var read = 0;

        while (read < length)
        {
            var n = stream.Read(buffer, read, length - read);

            if (n <= 0)
            {
                break;
            }

            read += n;
        }

        return read < length ? null : buffer;
    }

    private static Image<Rgba32>? TryReadGump(byte[] data)
    {
        if (data.Length < 8)
        {
            return null;
        }

        var w = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(0, 4));
        var h = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(4, 4));

        if (w == 0 || w > 2048 || h == 0 || h > 4096)
        {
            return null;
        }

        return GumpDecoder.Decode(data.AsSpan(8), (int)w, (int)h);
    }
}

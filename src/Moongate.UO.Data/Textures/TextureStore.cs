using System.Buffers.Binary;
using Moongate.UO.Data.Files;
using Moongate.UO.Data.Interfaces.Files;
using Moongate.UO.Data.Interfaces.Textures;
using Moongate.UO.Data.Internal.Colors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.UO.Data.Textures;

/// <summary>
///     Decodes land textures (raw RGB555 squares) from <c>texmaps.mul</c>. Missing files yield no
///     textures (non-fatal).
/// </summary>
public sealed class TextureStore : ITextureStore
{
    private readonly Dictionary<int, Image<Rgba32>> _cache = [];
    private readonly FileIndex _fileIndex;

    private byte[]? _buffer;

    public TextureStore(IUoFileResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);

        _fileIndex = new FileIndex(
            resolver.Resolve("texidx.mul"),
            resolver.Resolve("texmaps.mul"),
            0x4000,
            -1,
            new NullVerdataPatchSource()
        );
    }

    public Image<Rgba32>? GetTexture(int index, bool clone = true)
    {
        if (index < 0)
        {
            return null;
        }

        if (_cache.TryGetValue(index, out var cached))
        {
            return clone ? cached.Clone() : cached;
        }

        var stream = _fileIndex.Seek(index, out var length, out _, out _);

        if (stream is null || length <= 0)
        {
            return null;
        }

        var dim = length == 64 * 64 * 2 ? 64 : 128;

        if (length < dim * dim * 2)
        {
            return null;
        }

        if (_buffer is null || _buffer.Length < length)
        {
            _buffer = new byte[length];
        }

        stream.ReadExactly(_buffer, 0, length);

        var image = new Image<Rgba32>(dim, dim);
        var pixels = _buffer.AsSpan(0, dim * dim * 2);

        for (var y = 0; y < dim; y++)
        {
            for (var x = 0; x < dim; x++)
            {
                var color = BinaryPrimitives.ReadUInt16LittleEndian(pixels[((y * dim + x) * 2)..]);
                var (r, g, b) = Rgb555.ToRgb(color);
                image[x, y] = new Rgba32(r, g, b, 255);
            }
        }

        _cache[index] = image;

        return clone ? image.Clone() : image;
    }

    public bool IsValidTexture(int index)
    {
        return GetTexture(index) is not null;
    }
}

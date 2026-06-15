using Moongate.UO.Data.Files;
using Moongate.UO.Data.Interfaces.Files;
using Moongate.UO.Data.Interfaces.Gumps;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.UO.Data.Gumps;

/// <summary>Reads and decodes gumps from <c>gumpidx.mul</c>/<c>gumpart.mul</c> via a <see cref="FileIndex" />.</summary>
public sealed class GumpStore : IGumpStore
{
    private const int GumpIndexLength = 0x10000;
    private const int GumpFileId = 12;

    private readonly FileIndex _index;

    public GumpStore(IUoFileResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);

        _index = new FileIndex(
            resolver.Resolve("gumpidx.mul"),
            resolver.Resolve("gumpart.mul"),
            GumpIndexLength,
            GumpFileId,
            new NullVerdataPatchSource()
        );
    }

    public Image<Rgba32>? GetGump(int gumpId)
    {
        if (gumpId < 0 || gumpId >= GumpIndexLength)
        {
            return null;
        }

        var stream = _index.Seek(gumpId, out var length, out var extra, out _);

        if (stream is null || length <= 0 || extra <= 0)
        {
            return null;
        }

        var width = (extra >> 16) & 0xFFFF;
        var height = extra & 0xFFFF;

        if (width <= 0 || height <= 0)
        {
            return null;
        }

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

        return read < length ? null : GumpDecoder.Decode(buffer, width, height);
    }
}

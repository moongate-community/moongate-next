using Moongate.UO.Data.Files;
using Moongate.UO.Data.Interfaces.Animations;
using Moongate.UO.Data.Interfaces.Files;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.UO.Data.Animations;

/// <summary>
/// Decodes body frames from <c>anim.mul</c>/<c>anim.idx</c>, applying <c>Body.def</c> remapping and a
/// direction fallback. Thin glue over <see cref="AnimationIndex" />, <see cref="AnimationFrameDecoder" />
/// and the shared <see cref="FileIndex" />, mirroring <c>ArtService</c>.
/// </summary>
public sealed class AnimationService : IAnimationService
{
    private const int AnimIndexLength = 0x40000;
    private const int AnimFileId = 6;
    private const int DirectionCount = 5;

    private readonly FileIndex _fileIndex;
    private readonly BodyDefTable _bodyDef;

    public AnimationService(IUoFileResolver resolver, BodyDefTable bodyDef)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(bodyDef);

        _bodyDef = bodyDef;
        _fileIndex = new FileIndex(
            resolver.Resolve("anim.idx"),
            resolver.Resolve("anim.mul"),
            AnimIndexLength,
            AnimFileId,
            new NullVerdataPatchSource()
        );
    }

    public Image<Rgba32>? GetBodyFrame(int body, int action = 0, int direction = 1, int frame = 0)
    {
        var (graphic, _) = _bodyDef.Resolve(body);

        var image = TryDecode(graphic, action, direction, frame);

        if (image is not null)
        {
            return image;
        }

        for (var d = 0; d < DirectionCount; d++)
        {
            if (d == direction)
            {
                continue;
            }

            image = TryDecode(graphic, action, d, frame);

            if (image is not null)
            {
                return image;
            }
        }

        return null;
    }

    private Image<Rgba32>? TryDecode(int graphic, int action, int direction, int frame)
    {
        var index = AnimationIndex.GetIndex(graphic, action, direction);

        if (index < 0)
        {
            return null;
        }

        var stream = _fileIndex.Seek(index, out var length, out _, out _);

        if (stream is null || length <= 0)
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

        return read < length ? null : AnimationFrameDecoder.Decode(buffer, frame);
    }
}

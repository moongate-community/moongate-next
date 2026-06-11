using Moongate.UO.Data.Interfaces.Animations;
using Moongate.UO.Data.Interfaces.Files;
using Moongate.UO.Data.Interfaces.Hues;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.UO.Data.Animations;

/// <summary>
/// Decodes body frames from the UO animation files, applying <c>Body.def</c> remapping + hue and
/// <c>Bodyconv.def</c> routing (anim2..anim5), with a direction fallback. Thin glue over the animation
/// building blocks plus <see cref="HueApplier" />, mirroring <c>ArtService</c>.
/// </summary>
public sealed class AnimationService : IAnimationService
{
    private const int DirectionCount = 5;

    private readonly AnimationFileSet _fileSet;
    private readonly BodyDefTable _bodyDef;
    private readonly BodyConvTable _bodyConv;
    private readonly IHueStore _hueStore;

    public AnimationService(IUoFileResolver resolver, BodyDefTable bodyDef, BodyConvTable bodyConv, IHueStore hueStore)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(bodyDef);
        ArgumentNullException.ThrowIfNull(bodyConv);
        ArgumentNullException.ThrowIfNull(hueStore);

        _bodyDef = bodyDef;
        _bodyConv = bodyConv;
        _hueStore = hueStore;
        _fileSet = new AnimationFileSet(resolver);
    }

    public Image<Rgba32>? GetBodyFrame(int body, int action = 0, int direction = 1, int frame = 0)
    {
        var (graphic, hue) = _bodyDef.Resolve(body);

        int fileType;
        int index0;

        if (_bodyConv.TryRoute(graphic, out var route))
        {
            fileType = route.FileType;
            index0 = route.TranslatedIndex;
        }
        else
        {
            fileType = 1;
            index0 = graphic;
        }

        var image = DecodeWithFallback(index0, action, direction, frame, fileType);

        if (image is null)
        {
            return null;
        }

        if (hue != 0)
        {
            var resolved = _hueStore.GetHue(hue - 1);

            if (resolved is not null)
            {
                HueApplier.Apply(image, resolved);
            }
        }

        return image;
    }

    private Image<Rgba32>? DecodeWithFallback(int index0, int action, int direction, int frame, int fileType)
    {
        var image = TryDecode(index0, action, direction, frame, fileType);

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

            image = TryDecode(index0, action, d, frame, fileType);

            if (image is not null)
            {
                return image;
            }
        }

        return null;
    }

    private Image<Rgba32>? TryDecode(int body, int action, int direction, int frame, int fileType)
    {
        var index = AnimationIndex.GetIndex(body, action, direction, fileType);

        if (index < 0)
        {
            return null;
        }

        var stream = _fileSet.Seek(fileType, index, out var length);

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

using Moongate.UO.Data.Data.Hues;
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
        _fileSet = new(resolver);
    }

    public Image<Rgba32>? GetBodyFrame(int body, int action = 0, int direction = 1, int frame = 0, int hue = 0)
    {
        var decoded = GetDecodedFrame(body, action, direction, frame, hue);

        if (decoded is null)
        {
            for (var d = 0; d < DirectionCount; d++)
            {
                if (d == direction)
                {
                    continue;
                }

                decoded = GetDecodedFrame(body, action, d, frame, hue);

                if (decoded is not null)
                {
                    break;
                }
            }
        }

        return decoded?.Image;
    }

    public DecodedFrame? GetDecodedFrame(int graphic, int action, int direction, int frame, int hue)
    {
        var (resolved, bodyDefHue) = _bodyDef.Resolve(graphic);

        int fileType;
        int index0;

        if (_bodyConv.TryRoute(resolved, out var route))
        {
            fileType = route.FileType;
            index0 = route.TranslatedIndex;
        }
        else
        {
            fileType = 1;
            index0 = resolved;
        }

        var decoded = TryDecodeFrame(index0, action, direction, frame, fileType);

        if (decoded is null)
        {
            return null;
        }

        var effectiveHue = hue != 0 ? hue : bodyDefHue;

        if (effectiveHue != 0)
        {
            var resolvedHue = ResolveHue(effectiveHue);

            if (resolvedHue is not null)
            {
                HueApplier.Apply(decoded.Image, resolvedHue);
            }
        }

        return decoded;
    }

    private Hue? ResolveHue(int hueValue)
    {
        var index = (hueValue & 0x3FFF) - 1; // mask UO mode flags (e.g. 0x8000 partial), packet id is 1-based

        return index >= 0 ? _hueStore.GetHue(index) : null;
    }

    private DecodedFrame? TryDecodeFrame(int body, int action, int direction, int frame, int fileType)
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

        return read < length ? null : AnimationFrameDecoder.DecodeFrame(buffer, frame);
    }
}

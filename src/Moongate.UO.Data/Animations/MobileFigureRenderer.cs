using Moongate.UO.Data.Interfaces.Animations;
using Moongate.UO.Data.Interfaces.Tiles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.UO.Data.Animations;

/// <summary>
/// Composites a dressed mobile figure: the body (skin-hued) plus hair and facial hair (hued), all decoded
/// at the same pose and combined via <see cref="AnimationCompositor" />.
/// </summary>
public sealed class MobileFigureRenderer : IMobileFigureRenderer
{
    private const int DirectionCount = 5;

    private readonly IAnimationService _animation;
    private readonly ITileDataStore _tileData;

    public MobileFigureRenderer(IAnimationService animation, ITileDataStore tileData)
    {
        ArgumentNullException.ThrowIfNull(animation);
        ArgumentNullException.ThrowIfNull(tileData);

        _animation = animation;
        _tileData = tileData;
    }

    public Image<Rgba32>? Render(MobileRenderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        DecodedFrame? body = null;
        var chosenDirection = 0;

        for (var d = 0; d < DirectionCount; d++)
        {
            body = _animation.GetDecodedFrame(request.Body, 0, d, 0, request.SkinHue);

            if (body is not null)
            {
                chosenDirection = d;

                break;
            }
        }

        if (body is null)
        {
            return null;
        }

        var layers = new List<DecodedFrame> { body };

        AddHairLayer(layers, request.HairStyle, request.HairHue, chosenDirection);
        AddHairLayer(layers, request.FacialHairStyle, request.FacialHairHue, chosenDirection);

        try
        {
            return AnimationCompositor.Compose(layers);
        }
        finally
        {
            foreach (var layer in layers)
            {
                layer.Dispose();
            }
        }
    }

    private void AddHairLayer(List<DecodedFrame> layers, int style, int hue, int direction)
    {
        if (style == 0)
        {
            return;
        }

        var animationId = _tileData.GetItem(style).Animation;

        if (animationId <= 0)
        {
            return;
        }

        var frame = _animation.GetDecodedFrame(animationId, 0, direction, 0, hue);

        if (frame is not null)
        {
            layers.Add(frame);
        }
    }
}

using Moongate.UO.Data.Interfaces.Animations;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Interfaces.Tiles;
using Moongate.UO.Data.Types.Items;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.UO.Data.Animations;

/// <summary>
///     Composites a dressed mobile figure: body (skin-hued), hair, facial hair, and worn equipment (each routed
///     via <see cref="EquipConvTable" /> and hued), drawn in front-facing layer order via <see cref="AnimationCompositor" />.
/// </summary>
public sealed class MobileFigureRenderer : IMobileFigureRenderer
{
    private const int DirectionCount = 5;

    private readonly IAnimationService _animation;
    private readonly EquipConvTable _equipConv;
    private readonly IItemTemplateService _itemTemplates;
    private readonly ITileDataStore _tileData;

    public MobileFigureRenderer(
        IAnimationService animation,
        ITileDataStore tileData,
        IItemTemplateService itemTemplates,
        EquipConvTable equipConv
    )
    {
        ArgumentNullException.ThrowIfNull(animation);
        ArgumentNullException.ThrowIfNull(tileData);
        ArgumentNullException.ThrowIfNull(itemTemplates);
        ArgumentNullException.ThrowIfNull(equipConv);

        _animation = animation;
        _tileData = tileData;
        _itemTemplates = itemTemplates;
        _equipConv = equipConv;
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

        var layers = new List<(int Priority, DecodedFrame Frame)> { (EquipmentDrawOrder.BodyPriority, body) };

        AddHairLayer(layers, request.HairStyle, request.HairHue, chosenDirection, ItemLayerType.Hair);
        AddHairLayer(layers, request.FacialHairStyle, request.FacialHairHue, chosenDirection, ItemLayerType.FacialHair);
        AddEquipmentLayers(layers, request, chosenDirection);

        try
        {
            var ordered = layers.OrderBy(layer => layer.Priority).Select(layer => layer.Frame).ToArray();

            return AnimationCompositor.Compose(ordered);
        }
        finally
        {
            foreach (var layer in layers)
            {
                layer.Frame.Dispose();
            }
        }
    }

    private void AddEquipmentLayers(
        List<(int Priority, DecodedFrame Frame)> layers,
        MobileRenderRequest request,
        int direction
    )
    {
        foreach (var itemId in request.Equipment)
        {
            if (!_itemTemplates.TryGet(itemId, out var definition) || definition.Layer is null)
            {
                continue;
            }

            var priority = EquipmentDrawOrder.Priority(definition.Layer.Value);

            if (priority == EquipmentDrawOrder.Skip)
            {
                continue;
            }

            var equipmentAnim = _tileData.GetItem(definition.ItemId).Animation;

            if (equipmentAnim <= 0)
            {
                continue;
            }

            int finalAnim;
            int hue;

            if (_equipConv.TryConvert(request.Body, equipmentAnim, out var conversion))
            {
                finalAnim = conversion.AnimId;
                hue = conversion.Hue != 0 ? conversion.Hue : definition.Hue;
            }
            else
            {
                finalAnim = equipmentAnim;
                hue = definition.Hue;
            }

            var frame = _animation.GetDecodedFrame(finalAnim, 0, direction, 0, hue);

            if (frame is not null)
            {
                layers.Add((priority, frame));
            }
        }
    }

    private void AddHairLayer(
        List<(int Priority, DecodedFrame Frame)> layers,
        int style,
        int hue,
        int direction,
        ItemLayerType layer
    )
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
            layers.Add((EquipmentDrawOrder.Priority(layer), frame));
        }
    }
}

using Moongate.UO.Data.Data.Animations;
using Moongate.UO.Data.Interfaces.Animations;
using Moongate.UO.Data.Interfaces.Gumps;
using Moongate.UO.Data.Interfaces.Hues;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Interfaces.Tiles;
using Moongate.UO.Data.Types.Items;
using Moongate.UO.Data.Types.Mobiles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Moongate.UO.Data.Animations;

/// <summary>Composites a UO paperdoll from gump art: background, body, hair/beard, and worn equipment, hued.</summary>
public sealed class PaperdollRenderer : IPaperdollRenderer
{
    private const int MaleOffset = 50000;
    private const int FemaleOffset = 60000;
    private const int BackgroundMale = 0x07D0;
    private const int BackgroundFemale = 0x07D1;
    private const int BodyMale = 0x000C;
    private const int BodyFemale = 0x000D;

    private readonly IGumpStore _gumps;
    private readonly IItemTemplateService _itemTemplates;
    private readonly ITileDataStore _tileData;
    private readonly IHueStore _hues;

    public PaperdollRenderer(IGumpStore gumps, IItemTemplateService itemTemplates, ITileDataStore tileData, IHueStore hues)
    {
        ArgumentNullException.ThrowIfNull(gumps);
        ArgumentNullException.ThrowIfNull(itemTemplates);
        ArgumentNullException.ThrowIfNull(tileData);
        ArgumentNullException.ThrowIfNull(hues);

        _gumps = gumps;
        _itemTemplates = itemTemplates;
        _tileData = tileData;
        _hues = hues;
    }

    public Image<Rgba32>? Render(PaperdollRenderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var female = request.Gender == GenderType.Female;
        var layers = new List<(int Priority, Image<Rgba32> Image)>();

        if (request.IncludeBackground)
        {
            var background = _gumps.GetGump(female ? BackgroundFemale : BackgroundMale);

            if (background is not null)
            {
                layers.Add((PaperdollDrawOrder.BackgroundPriority, background));
            }
        }

        var body = LoadHued(female ? BodyFemale : BodyMale, request.SkinHue);

        if (body is null)
        {
            DisposeAll(layers);

            return null;
        }

        layers.Add((PaperdollDrawOrder.BodyPriority, body));

        AddGenderGump(layers, request.HairStyle, request.HairHue, PaperdollDrawOrder.Priority(ItemLayerType.Hair), female);
        AddGenderGump(layers, request.FacialHairStyle, request.FacialHairHue, PaperdollDrawOrder.Priority(ItemLayerType.FacialHair), female);
        AddEquipment(layers, request, female);

        var ordered = layers.OrderBy(layer => layer.Priority).Select(layer => layer.Image).ToArray();
        var width = ordered.Max(image => image.Width);
        var height = ordered.Max(image => image.Height);
        var canvas = new Image<Rgba32>(width, height);

        foreach (var image in ordered)
        {
            canvas.Mutate(context => context.DrawImage(image, new Point(0, 0), 1f));
            image.Dispose();
        }

        return canvas;
    }

    private void AddGenderGump(List<(int, Image<Rgba32>)> layers, int style, int hue, int priority, bool female)
    {
        if (style <= 0)
        {
            return;
        }

        var image = LoadGenderGump(style, hue, female);

        if (image is not null)
        {
            layers.Add((priority, image));
        }
    }

    private void AddEquipment(List<(int, Image<Rgba32>)> layers, PaperdollRenderRequest request, bool female)
    {
        foreach (var id in request.Equipment)
        {
            if (!_itemTemplates.TryGet(id, out var definition) || definition?.Layer is null)
            {
                continue;
            }

            var priority = PaperdollDrawOrder.Priority(definition.Layer.Value);

            if (priority == PaperdollDrawOrder.Skip)
            {
                continue;
            }

            var anim = _tileData.GetItem(definition.ItemId).Animation;

            if (anim <= 0)
            {
                continue;
            }

            var image = LoadGenderGump(anim, definition.Hue, female);

            if (image is not null)
            {
                layers.Add((priority, image));
            }
        }
    }

    private Image<Rgba32>? LoadGenderGump(int artId, int hue, bool female)
    {
        var image = LoadHued(artId + (female ? FemaleOffset : MaleOffset), hue);

        if (image is null && female)
        {
            image = LoadHued(artId + MaleOffset, hue);
        }

        return image;
    }

    private Image<Rgba32>? LoadHued(int gumpId, int hue)
    {
        var image = _gumps.GetGump(gumpId);

        if (image is null)
        {
            return null;
        }

        var index = (hue & 0x3FFF) - 1;

        if (index >= 0)
        {
            var resolved = _hues.GetHue(index);

            if (resolved is not null)
            {
                HueApplier.Apply(image, resolved);
            }
        }

        return image;
    }

    private static void DisposeAll(List<(int Priority, Image<Rgba32> Image)> layers)
    {
        foreach (var layer in layers)
        {
            layer.Image.Dispose();
        }
    }
}

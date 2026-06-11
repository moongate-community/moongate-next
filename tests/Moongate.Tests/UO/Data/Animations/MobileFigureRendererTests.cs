using Moongate.UO.Data.Animations;
using Moongate.UO.Data.Data.Tiles;
using Moongate.UO.Data.Interfaces.Animations;
using Moongate.UO.Data.Interfaces.Tiles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.Tests.UO.Data.Animations;

public sealed class MobileFigureRendererTests
{
    private sealed class FakeAnimation : IAnimationService
    {
        private readonly HashSet<int> _available;

        public FakeAnimation(params int[] availableGraphics)
        {
            _available = availableGraphics.ToHashSet();
        }

        public List<(int Graphic, int Direction, int Hue)> Calls { get; } = [];

        public Image<Rgba32>? GetBodyFrame(int body, int action = 0, int direction = 1, int frame = 0, int hue = 0)
            => null;

        public DecodedFrame? GetDecodedFrame(int graphic, int action, int direction, int frame, int hue)
        {
            Calls.Add((graphic, direction, hue));

            if (!_available.Contains(graphic))
            {
                return null;
            }

            var image = new Image<Rgba32>(1, 1);
            image[0, 0] = new Rgba32(255, 255, 255, 255);

            return new DecodedFrame(image, 0x200, 0x200);
        }
    }

    private sealed class FakeTileData : ITileDataStore
    {
        private readonly Dictionary<int, int> _hairAnim;

        public FakeTileData(Dictionary<int, int> hairAnim)
        {
            _hairAnim = hairAnim;
        }

        public IReadOnlyList<LandData> LandTable => [];

        public IReadOnlyList<ItemData> ItemTable => [];

        public ItemData GetItem(int id)
            => new() { Animation = _hairAnim.GetValueOrDefault(id) };

        public LandData GetLand(int id)
            => new();
    }

    private sealed class FakeItems : Moongate.UO.Data.Interfaces.Services.IItemTemplateService
    {
        private readonly Dictionary<string, Moongate.UO.Data.Templates.Items.ItemTemplateDefinition> _map = new(StringComparer.OrdinalIgnoreCase);

        public void Add(string id, int itemId, Moongate.UO.Data.Types.Items.ItemLayerType layer, int hue)
            => _map[id] = new() { Id = id, ItemId = itemId, Layer = layer, Hue = hue };

        public int Count => _map.Count;

        public void Clear() => _map.Clear();

        public IReadOnlyCollection<Moongate.UO.Data.Templates.Items.ItemTemplateDefinition> GetAll() => _map.Values.ToArray();

        public bool TryGet(string id, out Moongate.UO.Data.Templates.Items.ItemTemplateDefinition? definition) => _map.TryGetValue(id, out definition);

        public void UpsertRange(IEnumerable<Moongate.UO.Data.Templates.Items.ItemTemplateDefinition> templates)
        {
            foreach (var t in templates) { _map[t.Id] = t; }
        }
    }

    private static EquipConvTable EmptyEquipConv()
        => new(Path.Combine(Path.GetTempPath(), $"noequip-{Guid.NewGuid():N}.def"));

    [Fact]
    public void Render_BodyOnly_WhenNoHair()
    {
        var anim = new FakeAnimation(400);
        var renderer = new MobileFigureRenderer(anim, new FakeTileData([]), new FakeItems(), EmptyEquipConv());

        using var image = renderer.Render(new MobileRenderRequest(400, 1002, 0, 0, 0, 0, []));

        Assert.NotNull(image);
        Assert.DoesNotContain(anim.Calls, c => c.Graphic != 400); // only the body was decoded
    }

    [Fact]
    public void Render_CompositesHair_AtBodyDirectionWithHairHue()
    {
        var anim = new FakeAnimation(400, 9000);                 // body 400, hair anim 9000
        var tile = new FakeTileData(new Dictionary<int, int> { [3000] = 9000 }); // hair style 3000 -> anim 9000
        var renderer = new MobileFigureRenderer(anim, tile, new FakeItems(), EmptyEquipConv());

        using var image = renderer.Render(new MobileRenderRequest(400, 1002, 3000, 1110, 0, 0, []));

        Assert.NotNull(image);
        Assert.Contains(anim.Calls, c => c.Graphic == 400 && c.Hue == 1002);
        Assert.Contains(anim.Calls, c => c.Graphic == 9000 && c.Hue == 1110); // hair drawn, hair-hued
    }

    [Fact]
    public void Render_NullBody_ReturnsNull()
    {
        var renderer = new MobileFigureRenderer(new FakeAnimation(), new FakeTileData([]), new FakeItems(), EmptyEquipConv());

        Assert.Null(renderer.Render(new MobileRenderRequest(400, 0, 0, 0, 0, 0, [])));
    }

    [Fact]
    public void Render_DrawsEquipment_AtItemHue()
    {
        var anim = new FakeAnimation(400, 7000);                                   // body + shirt equip anim
        var tile = new FakeTileData(new Dictionary<int, int> { [5000] = 7000 });   // item graphic 5000 -> anim 7000
        var items = new FakeItems();
        items.Add("shirt", itemId: 5000, Moongate.UO.Data.Types.Items.ItemLayerType.Shirt, hue: 2000);
        var renderer = new MobileFigureRenderer(anim, tile, items, EmptyEquipConv());

        using var image = renderer.Render(new MobileRenderRequest(400, 1002, 0, 0, 0, 0, ["shirt"]));

        Assert.NotNull(image);
        Assert.Contains(anim.Calls, c => c.Graphic == 7000 && c.Hue == 2000); // shirt anim, item hue
    }

    [Fact]
    public void Render_SkipsNonDrawableLayerAndUnknownItem()
    {
        var anim = new FakeAnimation(400, 7000);
        var tile = new FakeTileData(new Dictionary<int, int> { [5000] = 7000 });
        var items = new FakeItems();
        items.Add("ring", itemId: 5000, Moongate.UO.Data.Types.Items.ItemLayerType.Ring, hue: 0); // non-drawable
        var renderer = new MobileFigureRenderer(anim, tile, items, EmptyEquipConv());

        using var image = renderer.Render(new MobileRenderRequest(400, 0, 0, 0, 0, 0, ["ring", "missing"]));

        Assert.NotNull(image);
        Assert.DoesNotContain(anim.Calls, c => c.Graphic == 7000); // ring skipped, missing skipped
    }

    [Fact]
    public void Render_EquipConvOverridesAnimAndHue()
    {
        var path = Path.Combine(Path.GetTempPath(), $"ec-{Guid.NewGuid():N}.def");
        File.WriteAllText(path, "400\t7000 7001 0\t2500\t# convert\n"); // (400,7000)->(7001, hue 2500)

        try
        {
            var anim = new FakeAnimation(400, 7001);                                 // converted anim available
            var tile = new FakeTileData(new Dictionary<int, int> { [5000] = 7000 }); // item -> base anim 7000
            var items = new FakeItems();
            items.Add("robe", itemId: 5000, Moongate.UO.Data.Types.Items.ItemLayerType.OuterTorso, hue: 1111);
            var renderer = new MobileFigureRenderer(anim, tile, items, new EquipConvTable(path));

            using var image = renderer.Render(new MobileRenderRequest(400, 0, 0, 0, 0, 0, ["robe"]));

            Assert.NotNull(image);
            Assert.Contains(anim.Calls, c => c.Graphic == 7001 && c.Hue == 2500); // converted anim, override hue beats 1111
        }
        finally
        {
            File.Delete(path);
        }
    }
}

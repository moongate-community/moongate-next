using System.Diagnostics.CodeAnalysis;
using Moongate.UO.Data.Animations;
using Moongate.UO.Data.Data.Animations;
using Moongate.UO.Data.Data.Hues;
using Moongate.UO.Data.Data.Tiles;
using Moongate.UO.Data.Interfaces.Gumps;
using Moongate.UO.Data.Interfaces.Hues;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Interfaces.Tiles;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Types.Items;
using Moongate.UO.Data.Types.Mobiles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.Tests.UO.Data.Animations;

public sealed class PaperdollRendererTests
{
    private const int MaleBody = 0x000C;
    private const int FemaleBody = 0x000D;

    // ─── Fakes ───────────────────────────────────────────────────────────────

    private sealed class FakeGumpStore : IGumpStore
    {
        public HashSet<int> Available { get; } = [];
        public List<int> Requested { get; } = [];

        public Image<Rgba32>? GetGump(int gumpId)
        {
            Requested.Add(gumpId);

            return Available.Contains(gumpId) ? new Image<Rgba32>(4, 4) : null;
        }
    }

    private sealed class FakeItemTemplates : IItemTemplateService
    {
        private readonly Dictionary<string, ItemTemplateDefinition> _map = new(StringComparer.OrdinalIgnoreCase);

        public int Count => _map.Count;

        public void Add(string id, int itemId, ItemLayerType layer)
            => _map[id] = new ItemTemplateDefinition { Id = id, ItemId = itemId, Layer = layer };

        public void Clear()
            => _map.Clear();

        public IReadOnlyCollection<ItemTemplateDefinition> GetAll()
            => _map.Values.ToArray();

        public void ReplaceAll(IEnumerable<ItemTemplateDefinition> templates)
        {
            _map.Clear();
            UpsertRange(templates);
        }

        public bool TryGet(string id, [NotNullWhen(true)] out ItemTemplateDefinition? definition)
            => _map.TryGetValue(id, out definition);

        public void UpsertRange(IEnumerable<ItemTemplateDefinition> templates)
        {
            foreach (var t in templates)
            {
                _map[t.Id] = t;
            }
        }
    }

    private sealed class FakeTileData : ITileDataStore
    {
        private readonly Dictionary<int, int> _animMap = [];

        public IReadOnlyList<LandData> LandTable => [];
        public IReadOnlyList<ItemData> ItemTable => [];

        public void SetAnimation(int itemId, int animation)
            => _animMap[itemId] = animation;

        public ItemData GetItem(int id)
            => new() { Animation = _animMap.GetValueOrDefault(id) };

        public LandData GetLand(int id)
            => new();
    }

    private sealed class NullHueStore : IHueStore
    {
        public IReadOnlyList<Hue> Hues => [];
        public int Count => 0;
        public Hue? GetHue(int index) => null;
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static PaperdollRenderer BuildRenderer(FakeGumpStore gumps, FakeItemTemplates? items = null, FakeTileData? tiles = null)
        => new(gumps, items ?? new FakeItemTemplates(), tiles ?? new FakeTileData(), new NullHueStore());

    private static PaperdollRenderRequest MakeRequest(
        GenderType gender = GenderType.Male,
        IReadOnlyList<string>? equipment = null
    ) => new(gender, 0, 0, 0, 0, 0, equipment ?? [], false);

    // ─── Tests ───────────────────────────────────────────────────────────────

    [Fact]
    public void Render_ReturnsNull_WhenBodyGumpUnavailable()
    {
        var gumps = new FakeGumpStore(); // nothing available
        var renderer = BuildRenderer(gumps);

        var result = renderer.Render(MakeRequest());

        Assert.Null(result);
    }

    [Fact]
    public void Render_MaleBody_ReturnsNonNull_AndRequestsMaleBodyGump()
    {
        var gumps = new FakeGumpStore();
        gumps.Available.Add(MaleBody);
        var renderer = BuildRenderer(gumps);

        using var result = renderer.Render(MakeRequest(GenderType.Male));

        Assert.NotNull(result);
        Assert.Contains(MaleBody, gumps.Requested);
    }

    [Fact]
    public void Render_FemaleBody_ReturnsNonNull_AndRequestsFemaleBodyGump()
    {
        var gumps = new FakeGumpStore();
        gumps.Available.Add(FemaleBody);
        var renderer = BuildRenderer(gumps);

        using var result = renderer.Render(MakeRequest(GenderType.Female));

        Assert.NotNull(result);
        Assert.Contains(FemaleBody, gumps.Requested);
    }

    [Fact]
    public void Render_FemaleEquipGumpMissing_FallsBackToMaleOffset()
    {
        // anim 100: female gump (100+60000) missing, male gump (100+50000) present
        const int Anim = 100;
        const int MaleGump = Anim + 50000;  // 50100
        const int FemaleGump = Anim + 60000; // 60100

        var gumps = new FakeGumpStore();
        gumps.Available.Add(FemaleBody);
        gumps.Available.Add(MaleGump); // only male offset available

        var items = new FakeItemTemplates();
        items.Add("sword", 7, ItemLayerType.OneHanded);

        var tiles = new FakeTileData();
        tiles.SetAnimation(7, Anim);

        var renderer = BuildRenderer(gumps, items, tiles);
        var request = new PaperdollRenderRequest(GenderType.Female, 0, 0, 0, 0, 0, ["sword"], false);

        using var result = renderer.Render(request);

        Assert.NotNull(result);
        Assert.Contains(FemaleGump, gumps.Requested); // tried female first
        Assert.Contains(MaleGump, gumps.Requested);   // then fell back to male
    }

    [Fact]
    public void Render_BackpackLayer_IsSkipped()
    {
        const int Anim = 200;
        const int MaleGump = Anim + 50000; // 50200

        var gumps = new FakeGumpStore();
        gumps.Available.Add(MaleBody);
        gumps.Available.Add(MaleGump); // available but should not be requested

        var items = new FakeItemTemplates();
        items.Add("pack", 9, ItemLayerType.Backpack);

        var tiles = new FakeTileData();
        tiles.SetAnimation(9, Anim);

        var renderer = BuildRenderer(gumps, items, tiles);
        var request = new PaperdollRenderRequest(GenderType.Male, 0, 0, 0, 0, 0, ["pack"], false);

        using var result = renderer.Render(request);

        Assert.NotNull(result);
        Assert.DoesNotContain(MaleGump, gumps.Requested);
    }
}

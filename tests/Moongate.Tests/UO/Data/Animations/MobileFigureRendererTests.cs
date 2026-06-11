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

    [Fact]
    public void Render_BodyOnly_WhenNoHair()
    {
        var anim = new FakeAnimation(400);
        var renderer = new MobileFigureRenderer(anim, new FakeTileData([]));

        using var image = renderer.Render(new MobileRenderRequest(400, 1002, 0, 0, 0, 0));

        Assert.NotNull(image);
        Assert.DoesNotContain(anim.Calls, c => c.Graphic != 400); // only the body was decoded
    }

    [Fact]
    public void Render_CompositesHair_AtBodyDirectionWithHairHue()
    {
        var anim = new FakeAnimation(400, 9000);                 // body 400, hair anim 9000
        var tile = new FakeTileData(new Dictionary<int, int> { [3000] = 9000 }); // hair style 3000 -> anim 9000
        var renderer = new MobileFigureRenderer(anim, tile);

        using var image = renderer.Render(new MobileRenderRequest(400, 1002, 3000, 1110, 0, 0));

        Assert.NotNull(image);
        Assert.Contains(anim.Calls, c => c.Graphic == 400 && c.Hue == 1002);
        Assert.Contains(anim.Calls, c => c.Graphic == 9000 && c.Hue == 1110); // hair drawn, hair-hued
    }

    [Fact]
    public void Render_NullBody_ReturnsNull()
    {
        var renderer = new MobileFigureRenderer(new FakeAnimation(), new FakeTileData([]));

        Assert.Null(renderer.Render(new MobileRenderRequest(400, 0, 0, 0, 0, 0)));
    }
}

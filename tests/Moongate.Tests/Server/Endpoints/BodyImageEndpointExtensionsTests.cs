using Microsoft.AspNetCore.Http.HttpResults;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.Data.Mobiles;
using Moongate.Server.Extensions.Endpoints;
using Moongate.UO.Data.Interfaces.Animations;
using Moongate.UO.Data.Interfaces.Bodies;
using Moongate.UO.Data.Types.Bodies;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.Tests.Server.Endpoints;

public sealed class BodyImageEndpointExtensionsTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"moongate-body-endpoint-{Guid.NewGuid():N}");
    private readonly DirectoriesConfig _directories;

    public BodyImageEndpointExtensionsTests()
    {
        _directories = new(_root, Enum.GetNames<DirectoryType>());
    }

    private sealed class FakeAnimationService : IAnimationService
    {
        private readonly HashSet<int> _available;

        public FakeAnimationService(IEnumerable<int> available)
        {
            _available = available.ToHashSet();
        }

        public int RenderCount { get; private set; }

        public Image<Rgba32>? GetBodyFrame(int body, int action = 0, int direction = 1, int frame = 0, int hue = 0)
        {
            RenderCount++;

            if (!_available.Contains(body))
            {
                return null;
            }

            var image = new Image<Rgba32>(3, 3);
            image[1, 1] = new Rgba32(255, 255, 255, 255);

            return image;
        }

        public Moongate.UO.Data.Animations.DecodedFrame? GetDecodedFrame(int graphic, int action, int direction, int frame, int hue)
            => null;
    }

    private sealed class FakeBodyDataStore : IBodyDataStore
    {
        private readonly int[] _bodies;

        public FakeBodyDataStore(params int[] bodies)
        {
            _bodies = bodies;
        }

        public int Count => _bodies.Length;

        public UoBodyType GetBodyType(int bodyId)
            => _bodies.Contains(bodyId) ? UoBodyType.Monster : UoBodyType.Empty;

        public IReadOnlyCollection<int> GetClassifiedBodies()
            => _bodies;
    }

    [Fact]
    public async Task Get_ExistingBody_ReturnsPngFile()
    {
        var result = await BodyImageEndpointExtensions.HandleGetBodyImageAsync(
            "400", null, new FakeAnimationService([400]), _directories, CancellationToken.None);

        Assert.Contains("PhysicalFile", result.GetType().Name, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Get_MissingBody_ReturnsNotFound()
    {
        var result = await BodyImageEndpointExtensions.HandleGetBodyImageAsync(
            "999", null, new FakeAnimationService([400]), _directories, CancellationToken.None);

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task Get_NonNumericBody_ReturnsBadRequest()
    {
        var result = await BodyImageEndpointExtensions.HandleGetBodyImageAsync(
            "0x190", null, new FakeAnimationService([400]), _directories, CancellationToken.None);

        Assert.IsType<BadRequest<string>>(result);
    }

    [Fact]
    public async Task Get_CachedSecondCall_DoesNotRegenerate()
    {
        var animation = new FakeAnimationService([400]);

        await BodyImageEndpointExtensions.HandleGetBodyImageAsync("400", null, animation, _directories, CancellationToken.None);
        await BodyImageEndpointExtensions.HandleGetBodyImageAsync("400", null, animation, _directories, CancellationToken.None);

        Assert.Equal(1, animation.RenderCount); // second call served from disk cache
    }

    [Fact]
    public async Task Build_TalliesGeneratedAndSkipped()
    {
        var result = await BodyImageEndpointExtensions.HandleBuildBodyImagesAsync(
            new FakeAnimationService([400]), new FakeBodyDataStore(400, 401), _directories, CancellationToken.None);

        var ok = Assert.IsType<Ok<BodyImageBuildResult>>(result);
        Assert.Equal(2, ok.Value!.TotalBodies);
        Assert.Equal(1, ok.Value.Generated); // 400 rendered
        Assert.Equal(1, ok.Value.Skipped);   // 401 has no image
    }

    [Fact]
    public void GetCachePath_IncludesHueWhenNonZero()
    {
        Assert.EndsWith($"{Path.DirectorySeparatorChar}400.png", BodyImageEndpointExtensions.GetCachePath(_directories, 400, 0));
        Assert.EndsWith($"{Path.DirectorySeparatorChar}400_1003.png", BodyImageEndpointExtensions.GetCachePath(_directories, 400, 1003));
    }

    [Fact]
    public async Task Get_DifferentHues_RenderSeparately()
    {
        var animation = new FakeAnimationService([400]);

        await BodyImageEndpointExtensions.HandleGetBodyImageAsync("400", 1003, animation, _directories, CancellationToken.None);
        await BodyImageEndpointExtensions.HandleGetBodyImageAsync("400", 2002, animation, _directories, CancellationToken.None);
        await BodyImageEndpointExtensions.HandleGetBodyImageAsync("400", 1003, animation, _directories, CancellationToken.None); // cache hit

        Assert.Equal(2, animation.RenderCount); // hue 1003 and hue 2002 each rendered once; the third served from cache
        Assert.True(File.Exists(BodyImageEndpointExtensions.GetCachePath(_directories, 400, 1003)));
        Assert.True(File.Exists(BodyImageEndpointExtensions.GetCachePath(_directories, 400, 2002)));
    }

    [Fact]
    public async Task Get_NegativeHue_TreatedAsBase()
    {
        var animation = new FakeAnimationService([400]);

        await BodyImageEndpointExtensions.HandleGetBodyImageAsync("400", -5, animation, _directories, CancellationToken.None);

        Assert.True(File.Exists(BodyImageEndpointExtensions.GetCachePath(_directories, 400, 0))); // negative hue -> base file
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }
    }
}

using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.Extensions.Endpoints;
using Moongate.UO.Data.Interfaces.Maps;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.Tests.Server.Endpoints;

public class MapImageEndpointExtensionsTests : IDisposable
{
    private readonly DirectoriesConfig _directories;
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"moongate-map-endpoint-{Guid.NewGuid():N}");

    public MapImageEndpointExtensionsTests()
    {
        _directories = new DirectoriesConfig(_root, Enum.GetNames<DirectoryType>());
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task HandleGetMapImageAsync_CacheHit_DoesNotRegenerate()
    {
        var cachePath = MapImageEndpointExtensions.GetCachePath(_directories, 1);
        File.WriteAllBytes(cachePath, [0x89, 0x50, 0x4E, 0x47]);
        var service = new TestMapImageService();

        _ = await MapImageEndpointExtensions.HandleGetMapImageAsync(
            1,
            service,
            _directories,
            CancellationToken.None
        );

        Assert.Equal(0, service.RenderCount);
    }

    [Fact]
    public async Task HandleGetMapImageAsync_CacheMiss_GeneratesUnderCacheDirectory()
    {
        var service = new TestMapImageService();

        var result = await MapImageEndpointExtensions.HandleGetMapImageAsync(
            0,
            service,
            _directories,
            CancellationToken.None
        );

        var cachePath = Path.Combine(_directories[DirectoryType.Cache], "images", "maps", "0.png");
        Assert.True(File.Exists(cachePath));
        Assert.Equal(1, service.RenderCount);
        Assert.Contains("PhysicalFile", result.GetType().Name, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleGetMapImageAsync_MissingMap_ReturnsNotFound()
    {
        var service = new TestMapImageService(false);

        var result = await MapImageEndpointExtensions.HandleGetMapImageAsync(
            9,
            service,
            _directories,
            CancellationToken.None
        );

        Assert.Contains("NotFound", result.GetType().Name, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class TestMapImageService : IMapImageService
    {
        private readonly bool _returnImage;

        public TestMapImageService(bool returnImage = true)
        {
            _returnImage = returnImage;
        }

        public int RenderCount { get; private set; }

        public Image? GetMapImage(int mapId)
        {
            RenderCount++;

            if (!_returnImage)
            {
                return null;
            }

            var image = new Image<Rgb24>(1, 1);
            image[0, 0] = new Rgb24(255, 0, 0);

            return image;
        }
    }
}

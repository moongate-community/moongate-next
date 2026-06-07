using Microsoft.AspNetCore.Http.HttpResults;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.Data.Items;
using Moongate.Server.Extensions.Endpoints;
using Moongate.UO.Data.Interfaces.Art;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.Tests.Server.Endpoints;

public class ItemImageEndpointExtensionsTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"moongate-item-endpoint-{Guid.NewGuid():N}");
    private readonly DirectoriesConfig _directories;

    public ItemImageEndpointExtensionsTests()
    {
        _directories = new(_root, Enum.GetNames<DirectoryType>());
    }

    [Fact]
    public async Task HandleGetItemImageAsync_CacheMiss_GeneratesUnderCacheDirectory()
    {
        var service = new TestArtService([1]);

        var result = await ItemImageEndpointExtensions.HandleGetItemImageAsync(
            "0x001",
            service,
            _directories,
            CancellationToken.None
        );

        var cachePath = Path.Combine(_directories[DirectoryType.Cache], "images", "items", "0x001.png");
        Assert.True(File.Exists(cachePath));
        Assert.Equal(1, service.RenderCount);
        Assert.Contains("PhysicalFile", result.GetType().Name, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleGetItemImageAsync_CacheHit_DoesNotRegenerate()
    {
        var cachePath = ItemImageEndpointExtensions.GetCachePath(_directories, 2);
        File.WriteAllBytes(cachePath, [0x89, 0x50, 0x4E, 0x47]);
        var service = new TestArtService([2]);

        _ = await ItemImageEndpointExtensions.HandleGetItemImageAsync(
            "0x002",
            service,
            _directories,
            CancellationToken.None
        );

        Assert.Equal(0, service.RenderCount);
    }

    [Fact]
    public async Task HandleGetItemImageAsync_InvalidFormat_ReturnsBadRequest()
    {
        var service = new TestArtService([1]);

        var result = await ItemImageEndpointExtensions.HandleGetItemImageAsync(
            "1",
            service,
            _directories,
            CancellationToken.None
        );

        Assert.Contains("BadRequest", result.GetType().Name, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleGetItemImageAsync_MissingArt_ReturnsNotFound()
    {
        var service = new TestArtService([]);

        var result = await ItemImageEndpointExtensions.HandleGetItemImageAsync(
            "0x001",
            service,
            _directories,
            CancellationToken.None
        );

        Assert.Contains("NotFound", result.GetType().Name, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleBuildItemImagesAsync_GeneratesEveryAvailableItem()
    {
        var service = new TestArtService([0, 2], maxItemId: 2);

        var result = await ItemImageEndpointExtensions.HandleBuildItemImagesAsync(
            service,
            _directories,
            CancellationToken.None
        );

        var ok = Assert.IsType<Ok<ItemImageBuildResult>>(result);
        Assert.NotNull(ok.Value);
        Assert.Equal(2, ok.Value.Generated);
        Assert.Equal(0, ok.Value.Cached);
        Assert.Equal(1, ok.Value.Skipped);
        Assert.Equal(0, ok.Value.Failed);
        Assert.True(File.Exists(ItemImageEndpointExtensions.GetCachePath(_directories, 0)));
        Assert.True(File.Exists(ItemImageEndpointExtensions.GetCachePath(_directories, 2)));
    }

    [Fact]
    public async Task HandleBuildItemImagesAsync_CacheHit_CountsCachedItem()
    {
        var cachePath = ItemImageEndpointExtensions.GetCachePath(_directories, 0);
        File.WriteAllBytes(cachePath, [0x89, 0x50, 0x4E, 0x47]);
        var service = new TestArtService([0], maxItemId: 0);

        var result = await ItemImageEndpointExtensions.HandleBuildItemImagesAsync(
            service,
            _directories,
            CancellationToken.None
        );

        var ok = Assert.IsType<Ok<ItemImageBuildResult>>(result);
        Assert.NotNull(ok.Value);
        Assert.Equal(0, ok.Value.Generated);
        Assert.Equal(1, ok.Value.Cached);
        Assert.Equal(0, ok.Value.Skipped);
        Assert.Equal(0, service.RenderCount);
    }

    [Fact]
    public void FormatFileName_NormalizesToMinimumThreeHexDigits()
    {
        Assert.Equal("0x001.png", ItemImageEndpointExtensions.FormatFileName(1));
        Assert.Equal("0x1000.png", ItemImageEndpointExtensions.FormatFileName(0x1000));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }

        GC.SuppressFinalize(this);
    }

    private sealed class TestArtService : IArtService
    {
        private readonly HashSet<int> _availableItemIds;

        public TestArtService(IEnumerable<int> availableItemIds, int maxItemId = 10)
        {
            _availableItemIds = availableItemIds.ToHashSet();
            MaxItemId = maxItemId;
        }

        public int MaxItemId { get; }

        public int RenderCount { get; private set; }

        public Image<Rgba32>? GetArt(int itemId, bool clone = true)
        {
            RenderCount++;

            if (!_availableItemIds.Contains(itemId))
            {
                return null;
            }

            var image = new Image<Rgba32>(3, 3);
            image[1, 1] = new(255, 255, 255, 255);

            return image;
        }

        public bool IsValidArt(int itemId)
            => _availableItemIds.Contains(itemId);
    }
}

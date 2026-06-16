using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Persistence.Data;
using Moongate.Server.Data.Mobiles;
using Moongate.Server.Extensions.Endpoints;
using Moongate.UO.Data.Animations;
using Moongate.UO.Data.Interfaces.Animations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.Tests.Server.Endpoints;

public sealed class HairImageEndpointExtensionsTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"moongate-hairimg-{Guid.NewGuid():N}");
    private readonly DirectoriesConfig _directories;

    public HairImageEndpointExtensionsTests()
    {
        _directories = new(_root, Enum.GetNames<DirectoryType>());
    }

    private sealed class FakeRenderer : IMobileFigureRenderer
    {
        private readonly bool _hasImage;

        public FakeRenderer(bool hasImage)
        {
            _hasImage = hasImage;
        }

        public int RenderCount { get; private set; }

        public Image<Rgba32>? Render(MobileRenderRequest request)
        {
            RenderCount++;

            if (!_hasImage)
            {
                return null;
            }

            var img = new Image<Rgba32>(3, 3);
            img[1, 1] = new(255, 255, 255, 255);

            return img;
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }
    }

    [Fact]
    public async Task HandleGetHairImage_RendererReturnsNull_ReturnsNotFound()
    {
        var result = await HairImageEndpointExtensions.HandleGetHairImageAsync(
                         0x203B,
                         0,
                         null,
                         false,
                         new FakeRenderer(false),
                         _directories,
                         CancellationToken.None
                     );

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task HandleGetHairImage_RendersAndCaches()
    {
        var renderer = new FakeRenderer(true);

        await HairImageEndpointExtensions.HandleGetHairImageAsync(
            0x203B,
            0,
            null,
            false,
            renderer,
            _directories,
            CancellationToken.None
        );
        await HairImageEndpointExtensions.HandleGetHairImageAsync(
            0x203B,
            0,
            null,
            false,
            renderer,
            _directories,
            CancellationToken.None
        );

        Assert.Equal(1, renderer.RenderCount);
    }

    [Fact]
    public void HandleListHairStyles_Facial_ReturnsFacialCatalog()
        => Assert.Equal(7, Ok(HairImageEndpointExtensions.HandleListHairStyles(true, null)).TotalCount);

    [Fact]
    public void HandleListHairStyles_NonFacial_ReturnsHairCatalog()
    {
        var page = Ok(HairImageEndpointExtensions.HandleListHairStyles(false, null));

        Assert.Equal(10, page.TotalCount);
        Assert.All(page.Items, item => Assert.False(item.IsFacial));
        Assert.StartsWith("/api/mobiles/hair/", page.Items[0].ImageUrl);
    }

    [Fact]
    public void HandleListHairStyles_Search_FiltersByName()
        => Assert.Equal(1, Ok(HairImageEndpointExtensions.HandleListHairStyles(false, "topknot")).TotalCount);

    private static PagedResult<HairStyleSummary> Ok(IResult result)
        => Assert.IsType<Ok<PagedResult<HairStyleSummary>>>(result).Value!;
}

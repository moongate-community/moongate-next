using Microsoft.AspNetCore.Http.HttpResults;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.Extensions.Endpoints;
using Moongate.UO.Data.Data.Animations;
using Moongate.UO.Data.Interfaces.Animations;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Mobiles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.Tests.Server.Endpoints;

public sealed class MobileTemplatePaperdollEndpointExtensionsTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"moongate-paperdoll-{Guid.NewGuid():N}");
    private readonly DirectoriesConfig _directories;

    public MobileTemplatePaperdollEndpointExtensionsTests()
    {
        _directories = new(_root, Enum.GetNames<DirectoryType>());
    }

    private sealed class FakeTemplates : IMobileTemplateService
    {
        private readonly Dictionary<string, MobileTemplateDefinition> _map = new(StringComparer.OrdinalIgnoreCase);

        public int Count => _map.Count;

        public void Add(MobileTemplateDefinition def)
            => _map[def.Id] = def;

        public void Clear()
            => _map.Clear();

        public IReadOnlyCollection<MobileTemplateDefinition> GetAll()
            => _map.Values.ToArray();

        public bool TryGet(string id, out MobileTemplateDefinition? definition)
            => _map.TryGetValue(id, out definition);

        public void UpsertRange(IEnumerable<MobileTemplateDefinition> templates)
        {
            foreach (var t in templates) { _map[t.Id] = t; }
        }

        public void ReplaceAll(IEnumerable<MobileTemplateDefinition> templates)
        {
            _map.Clear();
            foreach (var t in templates) { _map[t.Id] = t; }
        }
    }

    private sealed class FakeRenderer : IPaperdollRenderer
    {
        private readonly bool _hasImage;

        public FakeRenderer(bool hasImage)
        {
            _hasImage = hasImage;
        }

        public int RenderCount { get; private set; }

        public Image<Rgba32>? Render(PaperdollRenderRequest request)
        {
            RenderCount++;

            if (!_hasImage) { return null; }

            var img = new Image<Rgba32>(3, 3);
            img[1, 1] = new(255, 255, 255, 255);

            return img;
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) { Directory.Delete(_root, true); }
    }

    [Fact]
    public async Task Get_ExistingTemplate_ReturnsPngFile()
    {
        var templates = new FakeTemplates();
        templates.Add(new() { Id = "town_guard", Body = 400 });

        var result = await MobileTemplatePaperdollEndpointExtensions.HandleGetPaperdollAsync(
                         "town_guard",
                         templates,
                         new FakeRenderer(true),
                         _directories,
                         CancellationToken.None
                     );

        Assert.Contains("PhysicalFile", result.GetType().Name, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Get_MissingTemplate_ReturnsNotFound()
    {
        var result = await MobileTemplatePaperdollEndpointExtensions.HandleGetPaperdollAsync(
                         "ghost",
                         new FakeTemplates(),
                         new FakeRenderer(true),
                         _directories,
                         CancellationToken.None
                     );

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task Get_RendererReturnsNull_ReturnsNotFound()
    {
        var templates = new FakeTemplates();
        templates.Add(new() { Id = "town_guard", Body = 400 });

        var result = await MobileTemplatePaperdollEndpointExtensions.HandleGetPaperdollAsync(
                         "town_guard",
                         templates,
                         new FakeRenderer(false),
                         _directories,
                         CancellationToken.None
                     );

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task Get_CachedSecondCall_DoesNotRerender()
    {
        var templates = new FakeTemplates();
        templates.Add(new() { Id = "town_guard", Body = 400 });
        var renderer = new FakeRenderer(true);

        await MobileTemplatePaperdollEndpointExtensions.HandleGetPaperdollAsync(
            "town_guard",
            templates,
            renderer,
            _directories,
            CancellationToken.None
        );
        await MobileTemplatePaperdollEndpointExtensions.HandleGetPaperdollAsync(
            "town_guard",
            templates,
            renderer,
            _directories,
            CancellationToken.None
        );

        Assert.Equal(1, renderer.RenderCount);
    }
}

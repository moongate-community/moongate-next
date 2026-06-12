using Microsoft.AspNetCore.Http.HttpResults;
using Moongate.Persistence.Data;
using Moongate.Server.Data.Templates;
using Moongate.Server.Extensions.Endpoints;
using Moongate.Server.Interfaces.Services.Templates;
using Moongate.UO.Data.Data.Hues;
using Moongate.UO.Data.Interfaces.Hues;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Types.Items;

namespace Moongate.Tests.Server.Endpoints;

public sealed class ItemTemplateEndpointExtensionsTests
{
    private sealed class FakeTemplateService : IItemTemplateService
    {
        private readonly Dictionary<string, ItemTemplateDefinition> _templates = new(StringComparer.OrdinalIgnoreCase);

        public int Count => _templates.Count;

        public void Clear()
            => _templates.Clear();

        public IReadOnlyCollection<ItemTemplateDefinition> GetAll()
            => _templates.Values.ToArray();

        public void ReplaceAll(IEnumerable<ItemTemplateDefinition> templates)
        {
            _templates.Clear();
            UpsertRange(templates);
        }

        public bool TryGet(string id, out ItemTemplateDefinition? definition)
            => _templates.TryGetValue(id, out definition);

        public void UpsertRange(IEnumerable<ItemTemplateDefinition> templates)
        {
            foreach (var template in templates)
            {
                _templates[template.Id] = template;
            }
        }
    }

    private sealed class FakeHueStore : IHueStore
    {
        private readonly List<Hue> _hues = [];

        public IReadOnlyList<Hue> Hues => _hues;

        public int Count => _hues.Count;

        public Hue? GetHue(int index)
            => index >= 0 && index < _hues.Count ? _hues[index] : null;
    }

    private sealed class FakeAuthoringService : IItemTemplateAuthoringService
    {
        public ItemTemplateSaveResult? CreateResult { get; set; }

        public ItemTemplateSaveResult? UpdateResult { get; set; }

        public Exception? CreateException { get; set; }

        public Exception? UpdateException { get; set; }

        public ValueTask<ItemTemplateSaveResult> CreateAsync(
            ItemTemplateEditRequest request,
            CancellationToken cancellationToken = default
        )
        {
            if (CreateException is not null)
            {
                throw CreateException;
            }

            return ValueTask.FromResult(CreateResult ?? NewSaveResult(request.Id));
        }

        public ValueTask<ItemTemplateSaveResult?> UpdateAsync(
            string id,
            ItemTemplateEditRequest request,
            CancellationToken cancellationToken = default
        )
        {
            if (UpdateException is not null)
            {
                throw UpdateException;
            }

            return ValueTask.FromResult(UpdateResult);
        }
    }

    [Fact]
    public async Task HandleCreate_InvalidRequest_ReturnsBadRequest()
    {
        var authoring = new FakeAuthoringService
        {
            CreateException = new InvalidOperationException("invalid template")
        };

        var result = await ItemTemplateEndpointExtensions.HandleCreateAsync(
                         authoring,
                         new() { Id = "", ItemId = -1 },
                         CancellationToken.None
                     );

        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal("invalid template", badRequest.Value);
    }

    [Fact]
    public async Task HandleCreate_ReturnsOkSaveResult()
    {
        var authoring = new FakeAuthoringService();

        var result = await ItemTemplateEndpointExtensions.HandleCreateAsync(
                         authoring,
                         new() { Id = "web_crate", ItemId = 0x0E3F },
                         CancellationToken.None
                     );

        var ok = Assert.IsType<Ok<ItemTemplateSaveResult>>(result);
        Assert.Equal("web_crate", ok.Value!.Template.Id);
    }

    [Fact]
    public void HandleDetail_ExistingTemplate_ReturnsDetail()
    {
        var result = ItemTemplateEndpointExtensions.HandleDetail(
            SeedTemplates(),
            new FakeHueStore(),
            "longsword"
        );

        var ok = Assert.IsType<Ok<ItemTemplateDetail>>(result);
        Assert.Equal("longsword", ok.Value!.Id);
        Assert.Equal("Sharp blade", ok.Value.Comment);
        Assert.Equal("/api/items/0x0F61.png", ok.Value.ImageUrl);
    }

    [Fact]
    public void HandleDetail_MissingTemplate_ReturnsNotFound()
    {
        var result = ItemTemplateEndpointExtensions.HandleDetail(
            SeedTemplates(),
            new FakeHueStore(),
            "missing"
        );

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public void HandleList_AppliesStructuredFilters()
    {
        var result = ItemTemplateEndpointExtensions.HandleList(
            SeedTemplates(),
            new FakeHueStore(),
            1,
            20,
            null,
            "container",
            "Rare",
            null,
            "true"
        );

        var ok = Assert.IsType<Ok<PagedResult<ItemTemplateSummary>>>(result);
        Assert.Equal("crate_base", Assert.Single(ok.Value!.Items).Id);
    }

    [Fact]
    public void HandleList_InvalidAbstract_ReturnsBadRequest()
    {
        var result = ItemTemplateEndpointExtensions.HandleList(
            SeedTemplates(),
            new FakeHueStore(),
            1,
            20,
            null,
            null,
            null,
            null,
            "sometimes"
        );

        Assert.IsType<BadRequest<string>>(result);
    }

    [Fact]
    public void HandleList_InvalidRarity_ReturnsBadRequest()
    {
        var result = ItemTemplateEndpointExtensions.HandleList(
            SeedTemplates(),
            new FakeHueStore(),
            1,
            20,
            null,
            null,
            "Mythic",
            null,
            null
        );

        Assert.IsType<BadRequest<string>>(result);
    }

    [Fact]
    public void HandleList_ReturnsPagedSummaries()
    {
        var templates = SeedTemplates();

        var result = ItemTemplateEndpointExtensions.HandleList(
            templates,
            new FakeHueStore(),
            1,
            2,
            null,
            null,
            null,
            null,
            null
        );

        var ok = Assert.IsType<Ok<PagedResult<ItemTemplateSummary>>>(result);
        Assert.Equal(2, ok.Value!.Items.Count);
        Assert.Equal(3, ok.Value.TotalCount);
        Assert.Equal(["crate_base", "longsword"], ok.Value.Items.Select(static item => item.Id));
    }

    [Theory, InlineData("long", "longsword"), InlineData("Sharp", "longsword"), InlineData("weapon", "longsword"),
     InlineData("combat_script", "longsword"), InlineData("3937", "longsword"), InlineData("0x0F61", "longsword"),
     InlineData("0xF61", "longsword"), InlineData("125", "longsword"), InlineData("55", "longsword")]
    public void HandleList_SearchesApprovedFields(string search, string expectedId)
    {
        var result = ItemTemplateEndpointExtensions.HandleList(
            SeedTemplates(),
            new FakeHueStore(),
            1,
            20,
            search,
            null,
            null,
            null,
            null
        );

        var ok = Assert.IsType<Ok<PagedResult<ItemTemplateSummary>>>(result);
        Assert.Equal(expectedId, Assert.Single(ok.Value!.Items).Id);
    }

    [Fact]
    public async Task HandleUpdate_IdMismatch_ReturnsBadRequest()
    {
        var authoring = new FakeAuthoringService
        {
            UpdateException = new InvalidOperationException("Route id must match the request id.")
        };

        var result = await ItemTemplateEndpointExtensions.HandleUpdateAsync(
                         authoring,
                         "crate",
                         new() { Id = "other", ItemId = 0x0E3F },
                         CancellationToken.None
                     );

        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Contains("match", badRequest.Value);
    }

    [Fact]
    public async Task HandleUpdate_MissingTemplate_ReturnsNotFound()
    {
        var result = await ItemTemplateEndpointExtensions.HandleUpdateAsync(
                         new FakeAuthoringService(),
                         "missing",
                         new() { Id = "missing", ItemId = 0x0E3F },
                         CancellationToken.None
                     );

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task HandleUpdate_ReturnsOkSaveResult()
    {
        var authoring = new FakeAuthoringService
        {
            UpdateResult = NewSaveResult("web_crate")
        };

        var result = await ItemTemplateEndpointExtensions.HandleUpdateAsync(
                         authoring,
                         "web_crate",
                         new() { Id = "web_crate", ItemId = 0x0E3F },
                         CancellationToken.None
                     );

        var ok = Assert.IsType<Ok<ItemTemplateSaveResult>>(result);
        Assert.Equal("web_crate", ok.Value!.Template.Id);
    }

    private static ItemTemplateSaveResult NewSaveResult(string id)
        => new(
            ItemTemplateDetail.FromDefinition(
                new()
                {
                    Id = id,
                    Name = id,
                    ItemId = 0x0E3F
                },
                new FakeHueStore()
            ),
            "_web.yaml"
        );

    private static FakeTemplateService SeedTemplates()
    {
        var service = new FakeTemplateService();
        service.UpsertRange(
            [
                new()
                {
                    Id = "longsword",
                    Name = "Longsword",
                    Comment = "Sharp blade",
                    ItemId = 0x0F61,
                    ScriptId = "combat_script",
                    Rarity = ItemRarity.Common,
                    Layer = ItemLayerType.OneHanded,
                    Value = new()
                    {
                        Buy = 125,
                        Sell = 55
                    },
                    Tags = ["weapon"]
                },
                new()
                {
                    Id = "crate_base",
                    Name = "Crate",
                    Comment = "Storage",
                    ItemId = 0x0E3F,
                    Rarity = ItemRarity.Rare,
                    Tags = ["container"],
                    IsAbstract = true
                },
                new()
                {
                    Id = "robe_dark",
                    Name = "Dark Robe",
                    ItemId = 0x1F03,
                    Rarity = ItemRarity.Uncommon,
                    Layer = ItemLayerType.OuterTorso,
                    Tags = ["clothing"]
                }
            ]
        );

        return service;
    }
}

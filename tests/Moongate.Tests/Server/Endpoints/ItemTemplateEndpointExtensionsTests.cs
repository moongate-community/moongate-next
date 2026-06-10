using Microsoft.AspNetCore.Http.HttpResults;
using Moongate.Persistence.Data;
using Moongate.Server.Data.Templates;
using Moongate.Server.Extensions.Endpoints;
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

    [Fact]
    public void HandleList_ReturnsPagedSummaries()
    {
        var templates = SeedTemplates();

        var result = ItemTemplateEndpointExtensions.HandleList(
            templates,
            new FakeHueStore(),
            page: 1,
            pageSize: 2,
            search: null,
            tag: null,
            rarity: null,
            layer: null,
            abstractText: null
        );

        var ok = Assert.IsType<Ok<PagedResult<ItemTemplateSummary>>>(result);
        Assert.Equal(2, ok.Value!.Items.Count);
        Assert.Equal(3, ok.Value.TotalCount);
        Assert.Equal(["crate_base", "longsword"], ok.Value.Items.Select(static item => item.Id));
    }

    [Theory]
    [InlineData("long", "longsword")]
    [InlineData("Sharp", "longsword")]
    [InlineData("weapon", "longsword")]
    [InlineData("combat_script", "longsword")]
    [InlineData("3937", "longsword")]
    [InlineData("0x0F61", "longsword")]
    [InlineData("0xF61", "longsword")]
    public void HandleList_SearchesApprovedFields(string search, string expectedId)
    {
        var result = ItemTemplateEndpointExtensions.HandleList(
            SeedTemplates(),
            new FakeHueStore(),
            page: 1,
            pageSize: 20,
            search,
            tag: null,
            rarity: null,
            layer: null,
            abstractText: null
        );

        var ok = Assert.IsType<Ok<PagedResult<ItemTemplateSummary>>>(result);
        Assert.Equal(expectedId, Assert.Single(ok.Value!.Items).Id);
    }

    [Fact]
    public void HandleList_AppliesStructuredFilters()
    {
        var result = ItemTemplateEndpointExtensions.HandleList(
            SeedTemplates(),
            new FakeHueStore(),
            page: 1,
            pageSize: 20,
            search: null,
            tag: "container",
            rarity: "Rare",
            layer: null,
            abstractText: "true"
        );

        var ok = Assert.IsType<Ok<PagedResult<ItemTemplateSummary>>>(result);
        Assert.Equal("crate_base", Assert.Single(ok.Value!.Items).Id);
    }

    [Fact]
    public void HandleList_InvalidRarity_ReturnsBadRequest()
    {
        var result = ItemTemplateEndpointExtensions.HandleList(
            SeedTemplates(),
            new FakeHueStore(),
            page: 1,
            pageSize: 20,
            search: null,
            tag: null,
            rarity: "Mythic",
            layer: null,
            abstractText: null
        );

        Assert.IsType<BadRequest<string>>(result);
    }

    [Fact]
    public void HandleList_InvalidAbstract_ReturnsBadRequest()
    {
        var result = ItemTemplateEndpointExtensions.HandleList(
            SeedTemplates(),
            new FakeHueStore(),
            page: 1,
            pageSize: 20,
            search: null,
            tag: null,
            rarity: null,
            layer: null,
            abstractText: "sometimes"
        );

        Assert.IsType<BadRequest<string>>(result);
    }

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

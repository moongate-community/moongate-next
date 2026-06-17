using Microsoft.AspNetCore.Http.HttpResults;
using Moongate.Persistence.Data;
using Moongate.Server.Data.Templates;
using Moongate.Server.Extensions.Endpoints;
using Moongate.Server.Services.Loot;
using Moongate.UO.Data.Templates.Loot;

namespace Moongate.Tests.Server.Endpoints;

public sealed class LootTemplateEndpointExtensionsTests
{
    [Fact]
    public void HandleDetail_ExistingId_ReturnsProjectedDetail()
    {
        var registry = new LootTableRegistry([Table("orc_common")], []);
        var service = new LootTemplateProjectionService([]);

        var result = LootTemplateEndpointExtensions.HandleDetail(registry, service, "orc_common");

        var ok = Assert.IsType<Ok<LootTemplateDetail>>(result);
        Assert.Equal("orc_common", ok.Value!.Id);
    }

    [Fact]
    public void HandleDetail_UnknownId_ReturnsNotFound()
    {
        var registry = new LootTableRegistry([], []);
        var service = new LootTemplateProjectionService([]);

        var result = LootTemplateEndpointExtensions.HandleDetail(registry, service, "missing");

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public void HandleList_ReturnsPagedLootTemplateSummaries()
    {
        var registry = new LootTableRegistry([Table("orc_common"), Table("dragon_rare")], []);

        var result = LootTemplateEndpointExtensions.HandleList(registry, 1, 10, "orc");

        var ok = Assert.IsType<Ok<PagedResult<LootTemplateSummary>>>(result);
        Assert.Equal("orc_common", Assert.Single(ok.Value!.Items).Id);
        Assert.Equal(1, ok.Value.TotalCount);
    }

    private static LootTableDefinition Table(string id)
    {
        return new LootTableDefinition
        {
            Id = id,
            Content = [new LootNode { Item = "gold_coin" }]
        };
    }
}

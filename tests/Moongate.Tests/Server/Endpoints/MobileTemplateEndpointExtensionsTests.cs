using Microsoft.AspNetCore.Http.HttpResults;
using Moongate.Persistence.Data;
using Moongate.Server.Data.Templates;
using Moongate.Server.Extensions.Endpoints;
using Moongate.UO.Data.Templates.Mobiles;
using Moongate.UO.Data.Types.Mobiles;

namespace Moongate.Tests.Server.Endpoints;

public sealed class MobileTemplateEndpointExtensionsTests
{
    private sealed class FakeMobileTemplateService : Moongate.UO.Data.Interfaces.Services.IMobileTemplateService
    {
        private readonly Dictionary<string, MobileTemplateDefinition> _templates = new(StringComparer.OrdinalIgnoreCase);

        public int Count => _templates.Count;

        public void Clear()
            => _templates.Clear();

        public IReadOnlyCollection<MobileTemplateDefinition> GetAll()
            => _templates.Values.ToArray();

        public bool TryGet(string id, out MobileTemplateDefinition? definition)
            => _templates.TryGetValue(id, out definition);

        public void UpsertRange(IEnumerable<MobileTemplateDefinition> templates)
        {
            foreach (var template in templates)
            {
                _templates[template.Id] = template;
            }
        }
    }

    private static FakeMobileTemplateService Seed()
    {
        var service = new FakeMobileTemplateService();
        service.UpsertRange(
            [
                new MobileTemplateDefinition { Id = "town_guard", Name = "a guard", Body = 400, Notoriety = NotorietyType.Innocent, Tags = ["npc", "guard"] },
                new MobileTemplateDefinition { Id = "brigand", Name = "a brigand", Body = 401, Notoriety = NotorietyType.Criminal, Tags = ["npc"] },
                new MobileTemplateDefinition { Id = "base_humanoid", Body = 0, IsAbstract = true, Notoriety = NotorietyType.Innocent, Tags = ["humanoid"] }
            ]
        );

        return service;
    }

    private static PagedResult<MobileTemplateSummary> ListOk(Microsoft.AspNetCore.Http.IResult result)
        => Assert.IsType<Ok<PagedResult<MobileTemplateSummary>>>(result).Value!;

    [Fact]
    public void HandleList_NoFilters_ReturnsAllOrdered()
    {
        var page = ListOk(MobileTemplateEndpointExtensions.HandleList(Seed(), null, null, null, null, null, null));

        Assert.Equal(3, page.TotalCount);
        Assert.Equal("base_humanoid", page.Items[0].Id); // ordered by id
    }

    [Fact]
    public void HandleList_Search_MatchesNameAndBodyHex()
    {
        Assert.Equal(1, ListOk(MobileTemplateEndpointExtensions.HandleList(Seed(), null, null, "brigand", null, null, null)).TotalCount);
        Assert.Equal(1, ListOk(MobileTemplateEndpointExtensions.HandleList(Seed(), null, null, "0x0190", null, null, null)).TotalCount); // body 400 hex
    }

    [Fact]
    public void HandleList_TagFilter_IsCaseInsensitive()
    {
        Assert.Equal(2, ListOk(MobileTemplateEndpointExtensions.HandleList(Seed(), null, null, null, "NPC", null, null)).TotalCount);
    }

    [Fact]
    public void HandleList_NotorietyFilter_Filters()
    {
        Assert.Equal(1, ListOk(MobileTemplateEndpointExtensions.HandleList(Seed(), null, null, null, null, "Criminal", null)).TotalCount);
    }

    [Fact]
    public void HandleList_AbstractFilter_Filters()
    {
        Assert.Equal(1, ListOk(MobileTemplateEndpointExtensions.HandleList(Seed(), null, null, null, null, null, "true")).TotalCount);
        Assert.Equal(2, ListOk(MobileTemplateEndpointExtensions.HandleList(Seed(), null, null, null, null, null, "false")).TotalCount);
    }

    [Fact]
    public void HandleList_UnknownNotoriety_ReturnsBadRequest()
    {
        Assert.IsType<BadRequest<string>>(MobileTemplateEndpointExtensions.HandleList(Seed(), null, null, null, null, "Nope", null));
    }

    [Fact]
    public void HandleList_NonBoolAbstract_ReturnsBadRequest()
    {
        Assert.IsType<BadRequest<string>>(MobileTemplateEndpointExtensions.HandleList(Seed(), null, null, null, null, null, "maybe"));
    }

    [Fact]
    public void HandleList_Pagination_Bounds()
    {
        var page = ListOk(MobileTemplateEndpointExtensions.HandleList(Seed(), 1, 2, null, null, null, null));

        Assert.Equal(2, page.Items.Count);
        Assert.Equal(3, page.TotalCount);
        Assert.Equal(2, page.TotalPages);
    }

    [Fact]
    public void HandleDetail_Existing_ReturnsDetail()
    {
        var ok = Assert.IsType<Ok<MobileTemplateDetail>>(MobileTemplateEndpointExtensions.HandleDetail(Seed(), "town_guard"));

        Assert.Equal("town_guard", ok.Value!.Id);
        Assert.Equal("/api/mobiles/400.png", ok.Value.ImageUrl);
    }

    [Fact]
    public void HandleDetail_Missing_ReturnsNotFound()
    {
        Assert.IsType<NotFound>(MobileTemplateEndpointExtensions.HandleDetail(Seed(), "ghost"));
    }
}

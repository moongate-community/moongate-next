using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moongate.Persistence.Data;
using Moongate.Server.Data.Templates;
using Moongate.Server.Extensions.Endpoints;
using Moongate.Server.Interfaces.Services.Templates;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Mobiles;
using Moongate.UO.Data.Types.Mobiles;

namespace Moongate.Tests.Server.Endpoints;

public sealed class MobileTemplateEndpointExtensionsTests
{
    private sealed class FakeMobileAuthoringService : IMobileTemplateAuthoringService
    {
        public MobileTemplateSaveResult? CreateResult { get; set; }

        public MobileTemplateSaveResult? UpdateResult { get; set; }

        public Exception? CreateException { get; set; }

        public Exception? UpdateException { get; set; }

        public ValueTask<MobileTemplateSaveResult> CreateAsync(
            MobileTemplateEditRequest request,
            CancellationToken cancellationToken = default
        )
        {
            if (CreateException is not null)
            {
                throw CreateException;
            }

            return ValueTask.FromResult(CreateResult ?? NewSaveResult(request.Id));
        }

        public ValueTask<MobileTemplateSaveResult?> UpdateAsync(
            string id,
            MobileTemplateEditRequest request,
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

    private sealed class FakeMobileTemplateService : IMobileTemplateService
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

    [Fact]
    public void HandleDetail_Existing_ReturnsDetail()
    {
        var ok =
            Assert.IsType<Ok<MobileTemplateDetail>>(MobileTemplateEndpointExtensions.HandleDetail(Seed(), "town_guard"));

        Assert.Equal("town_guard", ok.Value!.Id);
        Assert.Equal("/api/mobile-templates/town_guard/image.png", ok.Value.ImageUrl);
    }

    [Fact]
    public void HandleDetail_Missing_ReturnsNotFound()
        => Assert.IsType<NotFound>(MobileTemplateEndpointExtensions.HandleDetail(Seed(), "ghost"));

    [Fact]
    public void HandleList_AbstractFilter_Filters()
    {
        Assert.Equal(
            1,
            ListOk(MobileTemplateEndpointExtensions.HandleList(Seed(), null, null, null, null, null, "true")).TotalCount
        );
        Assert.Equal(
            2,
            ListOk(MobileTemplateEndpointExtensions.HandleList(Seed(), null, null, null, null, null, "false")).TotalCount
        );
    }

    [Fact]
    public void HandleList_NoFilters_ReturnsAllOrdered()
    {
        var page = ListOk(MobileTemplateEndpointExtensions.HandleList(Seed(), null, null, null, null, null, null));

        Assert.Equal(3, page.TotalCount);
        Assert.Equal("base_humanoid", page.Items[0].Id); // ordered by id
    }

    [Fact]
    public void HandleList_NonBoolAbstract_ReturnsBadRequest()
        => Assert.IsType<BadRequest<string>>(
            MobileTemplateEndpointExtensions.HandleList(Seed(), null, null, null, null, null, "maybe")
        );

    [Fact]
    public void HandleList_NotorietyFilter_Filters()
        => Assert.Equal(
            1,
            ListOk(MobileTemplateEndpointExtensions.HandleList(Seed(), null, null, null, null, "Criminal", null)).TotalCount
        );

    [Fact]
    public void HandleList_Pagination_Bounds()
    {
        var page = ListOk(MobileTemplateEndpointExtensions.HandleList(Seed(), 1, 2, null, null, null, null));

        Assert.Equal(2, page.Items.Count);
        Assert.Equal(3, page.TotalCount);
        Assert.Equal(2, page.TotalPages);
    }

    [Fact]
    public void HandleList_Search_MatchesNameAndBodyHex()
    {
        Assert.Equal(
            1,
            ListOk(MobileTemplateEndpointExtensions.HandleList(Seed(), null, null, "brigand", null, null, null)).TotalCount
        );
        Assert.Equal(
            1,
            ListOk(MobileTemplateEndpointExtensions.HandleList(Seed(), null, null, "0x0190", null, null, null)).TotalCount
        ); // body 400 hex
    }

    [Fact]
    public void HandleList_TagFilter_IsCaseInsensitive()
        => Assert.Equal(
            2,
            ListOk(MobileTemplateEndpointExtensions.HandleList(Seed(), null, null, null, "NPC", null, null)).TotalCount
        );

    [Fact]
    public void HandleList_UnknownNotoriety_ReturnsBadRequest()
        => Assert.IsType<BadRequest<string>>(
            MobileTemplateEndpointExtensions.HandleList(Seed(), null, null, null, null, "Nope", null)
        );

    [Fact]
    public async Task HandleCreate_ReturnsOkWithSaveResult()
    {
        var authoring = new FakeMobileAuthoringService();

        var result = await MobileTemplateEndpointExtensions.HandleCreateAsync(
                         authoring,
                         new() { Id = "web_guard", Body = 400 },
                         CancellationToken.None
                     );

        var ok = Assert.IsType<Ok<MobileTemplateSaveResult>>(result);
        Assert.Equal("web_guard", ok.Value!.Template.Id);
    }

    [Fact]
    public async Task HandleCreate_InvalidRequest_ReturnsBadRequest()
    {
        var authoring = new FakeMobileAuthoringService
        {
            CreateException = new InvalidOperationException("id is required")
        };

        var result = await MobileTemplateEndpointExtensions.HandleCreateAsync(
                         authoring,
                         new() { Id = "", Body = 0 },
                         CancellationToken.None
                     );

        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal("id is required", badRequest.Value);
    }

    [Fact]
    public async Task HandleUpdate_ExistingTemplate_ReturnsOkWithSaveResult()
    {
        var authoring = new FakeMobileAuthoringService
        {
            UpdateResult = NewSaveResult("web_guard")
        };

        var result = await MobileTemplateEndpointExtensions.HandleUpdateAsync(
                         authoring,
                         "web_guard",
                         new() { Id = "web_guard", Body = 400 },
                         CancellationToken.None
                     );

        var ok = Assert.IsType<Ok<MobileTemplateSaveResult>>(result);
        Assert.Equal("web_guard", ok.Value!.Template.Id);
    }

    [Fact]
    public async Task HandleUpdate_MissingTemplate_ReturnsNotFound()
    {
        var result = await MobileTemplateEndpointExtensions.HandleUpdateAsync(
                         new FakeMobileAuthoringService(),
                         "missing",
                         new() { Id = "missing", Body = 0 },
                         CancellationToken.None
                     );

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task HandleUpdate_InvalidRequest_ReturnsBadRequest()
    {
        var authoring = new FakeMobileAuthoringService
        {
            UpdateException = new InvalidOperationException("Route id must match the request id.")
        };

        var result = await MobileTemplateEndpointExtensions.HandleUpdateAsync(
                         authoring,
                         "guard",
                         new() { Id = "other", Body = 400 },
                         CancellationToken.None
                     );

        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Contains("match", badRequest.Value);
    }

    private static MobileTemplateSaveResult NewSaveResult(string id)
        => new(
            MobileTemplateDetail.FromDefinition(
                new()
                {
                    Id = id,
                    Name = id,
                    Body = 400
                }
            ),
            "_web.yaml"
        );

    private static PagedResult<MobileTemplateSummary> ListOk(IResult result)
        => Assert.IsType<Ok<PagedResult<MobileTemplateSummary>>>(result).Value!;

    private static FakeMobileTemplateService Seed()
    {
        var service = new FakeMobileTemplateService();
        service.UpsertRange(
            [
                new()
                {
                    Id = "town_guard", Name = "a guard", Body = 400, Notoriety = NotorietyType.Innocent,
                    Tags = ["npc", "guard"]
                },
                new() { Id = "brigand", Name = "a brigand", Body = 401, Notoriety = NotorietyType.Criminal, Tags = ["npc"] },
                new()
                {
                    Id = "base_humanoid", Body = 0, IsAbstract = true, Notoriety = NotorietyType.Innocent,
                    Tags = ["humanoid"]
                }
            ]
        );

        return service;
    }
}

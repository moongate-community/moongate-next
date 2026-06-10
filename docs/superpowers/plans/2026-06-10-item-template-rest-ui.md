# Item Template REST UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build an admin-only, read-only REST API and Admin UI catalog for browsing loaded item templates with server-side search, filters, item images, and real hue palette swatches.

**Architecture:** The backend exposes minimal API endpoints that read from `IItemTemplateService` and `IHueStore`, map registry models into explicit DTOs, and use a small reusable in-memory list query helper. The frontend adds an Admin navigation view composed from focused React components: catalog panel, table, detail panel, image cell, and hue swatch.

**Tech Stack:** ASP.NET Core minimal APIs, DryIoc-registered services, xUnit endpoint/model tests, React 19, TypeScript, Vite, Tailwind utility classes, lucide-react icons.

---

## File Structure

Backend files:

- Create `src/Moongate.Server/Data/ListQueries/InMemoryListQuery.cs` for reusable in-memory search/filter/pagination.
- Create `src/Moongate.Server/Data/Templates/HueSummary.cs` for hue DTOs and palette mapping.
- Create `src/Moongate.Server/Data/Templates/ItemTemplateSummary.cs` for list DTO projection.
- Create `src/Moongate.Server/Data/Templates/ItemTemplateDetail.cs` for detail DTO projection.
- Create `src/Moongate.Server/Extensions/Endpoints/ItemTemplateEndpointExtensions.cs` for `/api/admin/item-templates`.
- Create `src/Moongate.Server/Extensions/Endpoints/AdminHueEndpointExtensions.cs` for `/api/admin/hues/{hue}`.
- Modify `src/Moongate.Server/Bootstrap/WebHostExtensions.cs` to map the new endpoints.
- Create tests under `tests/Moongate.Tests/Server/Data/` and `tests/Moongate.Tests/Server/Endpoints/`.

Frontend files:

- Create `ui/src/types/itemTemplates.ts` for API types.
- Create `ui/src/lib/adminItemTemplatesClient.ts` for REST calls.
- Create `ui/src/components/admin/itemTemplates/HueSwatch.tsx`.
- Create `ui/src/components/admin/itemTemplates/ItemImageCell.tsx`.
- Create `ui/src/components/admin/itemTemplates/ItemTemplateTable.tsx`.
- Create `ui/src/components/admin/itemTemplates/ItemTemplateDetailPanel.tsx`.
- Create `ui/src/components/admin/itemTemplates/ItemTemplateCatalogPanel.tsx`.
- Modify `ui/src/types/admin.ts`, `ui/src/data/navigation.ts`, and `ui/src/pages/AdminDashboard.tsx`.

---

### Task 1: Generic In-Memory List Query Helper

**Files:**
- Create: `src/Moongate.Server/Data/ListQueries/InMemoryListQuery.cs`
- Test: `tests/Moongate.Tests/Server/Data/InMemoryListQueryTests.cs`

- [ ] **Step 1: Write the failing tests**

Create `tests/Moongate.Tests/Server/Data/InMemoryListQueryTests.cs`:

```csharp
using Moongate.Persistence.Data;
using Moongate.Server.Data.ListQueries;

namespace Moongate.Tests.Server.Data;

public sealed class InMemoryListQueryTests
{
    private sealed record Entry(string Id, string Name, string Tag, bool Enabled);

    [Fact]
    public void Apply_SearchesConfiguredFields_CaseInsensitive()
    {
        var entries = new[]
        {
            new Entry("alpha", "Iron Sword", "weapon", true),
            new Entry("beta", "Cotton Shirt", "clothing", true),
            new Entry("gamma", "Wooden Crate", "container", false)
        };

        var result = InMemoryListQuery.Apply(
            entries,
            PageRequest.Normalize(1, 20, "sWoRd"),
            entry => [entry.Id, entry.Name, entry.Tag],
            []
        );

        var item = Assert.Single(result.Items);
        Assert.Equal("alpha", item.Id);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public void Apply_ComposesFiltersBeforePaging()
    {
        var entries = new[]
        {
            new Entry("alpha", "Iron Sword", "weapon", true),
            new Entry("beta", "Copper Sword", "weapon", false),
            new Entry("gamma", "Wooden Crate", "container", true)
        };

        var result = InMemoryListQuery.Apply(
            entries,
            PageRequest.Normalize(1, 1, "sword"),
            entry => [entry.Id, entry.Name, entry.Tag],
            [entry => entry.Enabled]
        );

        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal("alpha", Assert.Single(result.Items).Id);
    }

    [Fact]
    public void Apply_ReturnsRequestedPageMetadata()
    {
        var entries = Enumerable.Range(1, 5)
                                .Select(index => new Entry($"id-{index}", $"Name {index}", "tag", true))
                                .ToArray();

        var result = InMemoryListQuery.Apply(
            entries,
            PageRequest.Normalize(2, 2, null),
            entry => [entry.Id, entry.Name],
            []
        );

        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(["id-3", "id-4"], result.Items.Select(static entry => entry.Id));
    }
}
```

- [ ] **Step 2: Run tests to verify RED**

Run:

```bash
dotnet test tests/Moongate.Tests/Moongate.Tests.csproj --filter "FullyQualifiedName~InMemoryListQueryTests"
```

Expected: build fails with `CS0234` or `CS0103` because `Moongate.Server.Data.ListQueries.InMemoryListQuery` does not exist.

- [ ] **Step 3: Implement the helper**

Create `src/Moongate.Server/Data/ListQueries/InMemoryListQuery.cs`:

```csharp
using Moongate.Persistence.Data;

namespace Moongate.Server.Data.ListQueries;

public static class InMemoryListQuery
{
    public static PagedResult<T> Apply<T>(
        IEnumerable<T> source,
        PageRequest request,
        Func<T, IEnumerable<string?>> searchableFields,
        IReadOnlyCollection<Func<T, bool>> filters
    )
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(searchableFields);
        ArgumentNullException.ThrowIfNull(filters);

        var query = source;

        foreach (var filter in filters)
        {
            query = query.Where(filter);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(item => MatchesSearch(item, request.Search, searchableFields));
        }

        var filtered = query.ToArray();
        var pageItems = filtered.Skip((request.Page - 1) * request.PageSize)
                                .Take(request.PageSize)
                                .ToArray();

        return new(pageItems, request.Page, request.PageSize, filtered.Length);
    }

    private static bool MatchesSearch<T>(
        T item,
        string search,
        Func<T, IEnumerable<string?>> searchableFields
    )
        => searchableFields(item)
           .Any(field => !string.IsNullOrWhiteSpace(field)
                         && field.Contains(search, StringComparison.OrdinalIgnoreCase));
}
```

- [ ] **Step 4: Run tests to verify GREEN**

Run:

```bash
dotnet test tests/Moongate.Tests/Moongate.Tests.csproj --filter "FullyQualifiedName~InMemoryListQueryTests"
```

Expected: PASS, 3 tests.

- [ ] **Step 5: Commit**

```bash
git add src/Moongate.Server/Data/ListQueries/InMemoryListQuery.cs tests/Moongate.Tests/Server/Data/InMemoryListQueryTests.cs
git commit -m "Add in-memory list query helper"
```

---

### Task 2: Item Template and Hue API DTOs

**Files:**
- Create: `src/Moongate.Server/Data/Templates/HueSummary.cs`
- Create: `src/Moongate.Server/Data/Templates/ItemTemplateSummary.cs`
- Create: `src/Moongate.Server/Data/Templates/ItemTemplateDetail.cs`
- Test: `tests/Moongate.Tests/Server/Data/ItemTemplateApiModelTests.cs`

- [ ] **Step 1: Write the failing tests**

Create `tests/Moongate.Tests/Server/Data/ItemTemplateApiModelTests.cs`:

```csharp
using Moongate.Core.Types;
using Moongate.Server.Data.Templates;
using Moongate.Tests.Support;
using Moongate.UO.Data.Data.Hues;
using Moongate.UO.Data.Interfaces.Hues;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Types.Items;

namespace Moongate.Tests.Server.Data;

public sealed class ItemTemplateApiModelTests
{
    private sealed class FakeHueStore : IHueStore
    {
        private readonly List<Hue> _hues = [];

        public IReadOnlyList<Hue> Hues => _hues;
        public int Count => _hues.Count;

        public void Add(Hue hue)
            => _hues.Add(hue);

        public Hue? GetHue(int index)
            => index >= 0 && index < _hues.Count ? _hues[index] : null;
    }

    [Fact]
    public void HueSummary_FromValueZero_ReturnsNoneDescriptor()
    {
        var summary = HueSummary.FromValue(0, new FakeHueStore());

        Assert.True(summary.IsNone);
        Assert.True(summary.IsKnown);
        Assert.Equal("0x000", summary.Hex);
        Assert.Empty(summary.Colors);
    }

    [Fact]
    public void HueSummary_FromValidHue_ReturnsThirtyTwoRgbColors()
    {
        var store = new FakeHueStore();
        var colors = Enumerable.Repeat((ushort)0x7FFF, 32).ToArray();
        store.Add(new(colors, 0, 31, "Bright White"));

        var summary = HueSummary.FromValue(1, store);

        Assert.False(summary.IsNone);
        Assert.True(summary.IsKnown);
        Assert.Equal("Bright White", summary.Name);
        Assert.Equal("0x001", summary.Hex);
        Assert.Equal(32, summary.Colors.Count);
        Assert.Equal("#FFFFFF", summary.Colors[0].Hex);
    }

    [Fact]
    public void HueSummary_FromUnknownHue_ReturnsUnknownDescriptor()
    {
        var summary = HueSummary.FromValue(99, new FakeHueStore());

        Assert.False(summary.IsNone);
        Assert.False(summary.IsKnown);
        Assert.Equal("0x063", summary.Hex);
        Assert.Empty(summary.Colors);
    }

    [Fact]
    public void ItemTemplateSummary_MapsDisplayFields()
    {
        var template = new ItemTemplateDefinition
        {
            Id = "longsword",
            Name = "Longsword",
            ItemId = 0x0F61,
            Hue = 1,
            Rarity = ItemRarity.Common,
            Layer = ItemLayerType.OneHanded,
            Tags = ["weapon"],
            IsAbstract = false
        };

        var summary = ItemTemplateSummary.FromDefinition(template, new FakeHueStore());

        Assert.Equal("longsword", summary.Id);
        Assert.Equal("0x0F61", summary.ItemIdHex);
        Assert.Equal("/api/items/0x0F61.png", summary.ImageUrl);
        Assert.Equal("Common", summary.Rarity);
        Assert.Equal("OneHanded", summary.Layer);
        Assert.Equal(["weapon"], summary.Tags);
    }

    [Fact]
    public void ItemTemplateDetail_MapsParamsAndComment()
    {
        var template = new ItemTemplateDefinition
        {
            Id = "crate",
            Name = "Crate",
            Comment = "Container base",
            BaseItem = "base_container",
            ItemId = 0x0E3F,
            ScriptId = "container_script",
            Visibility = UserLevelType.GameMaster,
            Amount = 2,
            Weight = 3,
            IsMovable = true,
            IsStackable = false,
            GumpId = 42,
            Params =
            {
                ["capacity"] = new() { Type = ItemTemplateParamType.Integer, Value = "125" }
            }
        };

        var detail = ItemTemplateDetail.FromDefinition(template, new FakeHueStore());

        Assert.Equal("Container base", detail.Comment);
        Assert.Equal("base_container", detail.BaseItem);
        Assert.Equal("container_script", detail.ScriptId);
        Assert.Equal("GameMaster", detail.Visibility);
        Assert.Equal(2, detail.Amount);
        Assert.Equal(3, detail.Weight);
        Assert.True(detail.IsMovable);
        Assert.False(detail.IsStackable);
        Assert.Equal(42, detail.GumpId);
        var param = Assert.Single(detail.Params);
        Assert.Equal("capacity", param.Key);
        Assert.Equal("Integer", param.Type);
        Assert.Equal("125", param.Value);
    }
}
```

- [ ] **Step 2: Run tests to verify RED**

Run:

```bash
dotnet test tests/Moongate.Tests/Moongate.Tests.csproj --filter "FullyQualifiedName~ItemTemplateApiModelTests"
```

Expected: build fails because `Moongate.Server.Data.Templates` DTOs do not exist.

- [ ] **Step 3: Implement hue DTO**

Create `src/Moongate.Server/Data/Templates/HueSummary.cs`:

```csharp
using System.Globalization;
using Moongate.UO.Data.Interfaces.Hues;

namespace Moongate.Server.Data.Templates;

public sealed record HueColorSummary(int Index, byte R, byte G, byte B, string Hex);

public sealed record HueSummary(
    int Value,
    string Hex,
    string Name,
    bool IsNone,
    bool IsKnown,
    IReadOnlyList<HueColorSummary> Colors
)
{
    public static HueSummary FromValue(int value, IHueStore hues)
    {
        ArgumentNullException.ThrowIfNull(hues);

        if (value == 0)
        {
            return new(0, FormatHue(value), "None", true, true, []);
        }

        var hue = hues.GetHue(value - 1);

        if (hue is null)
        {
            return new(value, FormatHue(value), "Unknown hue", false, false, []);
        }

        var colors = new List<HueColorSummary>(hue.Colors.Length);

        for (var index = 0; index < hue.Colors.Length; index++)
        {
            var (r, g, b) = hue.GetRgb(index);
            colors.Add(new(index, r, g, b, $"#{r:X2}{g:X2}{b:X2}"));
        }

        return new(value, FormatHue(value), hue.Name, false, true, colors);
    }

    public static string FormatHue(int value)
        => $"0x{value.ToString("X3", CultureInfo.InvariantCulture)}";
}
```

- [ ] **Step 4: Implement item template DTOs**

Create `src/Moongate.Server/Data/Templates/ItemTemplateSummary.cs`:

```csharp
using System.Globalization;
using Moongate.UO.Data.Interfaces.Hues;
using Moongate.UO.Data.Templates.Items;

namespace Moongate.Server.Data.Templates;

public sealed record ItemTemplateSummary(
    string Id,
    string Name,
    int ItemId,
    string ItemIdHex,
    string ImageUrl,
    string Rarity,
    string? Layer,
    IReadOnlyList<string> Tags,
    bool IsAbstract,
    HueSummary Hue
)
{
    public static ItemTemplateSummary FromDefinition(ItemTemplateDefinition template, IHueStore hues)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(hues);

        var itemIdHex = FormatItemId(template.ItemId);

        return new(
            template.Id,
            template.Name,
            template.ItemId,
            itemIdHex,
            $"/api/items/{itemIdHex}.png",
            template.Rarity.ToString(),
            template.Layer?.ToString(),
            [..template.Tags],
            template.IsAbstract,
            HueSummary.FromValue(template.Hue, hues)
        );
    }

    public static string FormatItemId(int itemId)
        => $"0x{itemId.ToString("X4", CultureInfo.InvariantCulture)}";
}
```

Create `src/Moongate.Server/Data/Templates/ItemTemplateDetail.cs`:

```csharp
using Moongate.UO.Data.Interfaces.Hues;
using Moongate.UO.Data.Templates.Items;

namespace Moongate.Server.Data.Templates;

public sealed record ItemTemplateParamSummary(string Key, string Type, string Value);

public sealed record ItemTemplateDetail(
    string Id,
    string Name,
    int ItemId,
    string ItemIdHex,
    string ImageUrl,
    string Rarity,
    string? Layer,
    IReadOnlyList<string> Tags,
    bool IsAbstract,
    HueSummary Hue,
    string Comment,
    string? BaseItem,
    string ScriptId,
    string Visibility,
    int Amount,
    int Weight,
    bool IsStackable,
    bool IsMovable,
    int? GumpId,
    IReadOnlyList<ItemTemplateParamSummary> Params
)
{
    public static ItemTemplateDetail FromDefinition(ItemTemplateDefinition template, IHueStore hues)
    {
        var summary = ItemTemplateSummary.FromDefinition(template, hues);
        var parameters = template.Params
                                 .OrderBy(static pair => pair.Key, StringComparer.OrdinalIgnoreCase)
                                 .Select(static pair => new ItemTemplateParamSummary(
                                             pair.Key,
                                             pair.Value.Type.ToString(),
                                             pair.Value.Value
                                         ))
                                 .ToArray();

        return new(
            summary.Id,
            summary.Name,
            summary.ItemId,
            summary.ItemIdHex,
            summary.ImageUrl,
            summary.Rarity,
            summary.Layer,
            summary.Tags,
            summary.IsAbstract,
            summary.Hue,
            template.Comment,
            template.BaseItem,
            template.ScriptId,
            template.Visibility.ToString(),
            template.Amount,
            template.Weight,
            template.IsStackable,
            template.IsMovable,
            template.GumpId,
            parameters
        );
    }
}
```

- [ ] **Step 5: Run tests to verify GREEN**

Run:

```bash
dotnet test tests/Moongate.Tests/Moongate.Tests.csproj --filter "FullyQualifiedName~ItemTemplateApiModelTests"
```

Expected: PASS, 5 tests.

- [ ] **Step 6: Commit**

```bash
git add src/Moongate.Server/Data/Templates/HueSummary.cs src/Moongate.Server/Data/Templates/ItemTemplateSummary.cs src/Moongate.Server/Data/Templates/ItemTemplateDetail.cs tests/Moongate.Tests/Server/Data/ItemTemplateApiModelTests.cs
git commit -m "Add item template API models"
```

---

### Task 3: Item Template List Endpoint

**Files:**
- Create: `src/Moongate.Server/Extensions/Endpoints/ItemTemplateEndpointExtensions.cs`
- Test: `tests/Moongate.Tests/Server/Endpoints/ItemTemplateEndpointExtensionsTests.cs`

- [ ] **Step 1: Write failing list endpoint tests**

Create `tests/Moongate.Tests/Server/Endpoints/ItemTemplateEndpointExtensionsTests.cs`:

```csharp
using Microsoft.AspNetCore.Http.HttpResults;
using Moongate.Persistence.Data;
using Moongate.Server.Data.Templates;
using Moongate.Server.Extensions.Endpoints;
using Moongate.Tests.Support;
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
        public void Clear() => _templates.Clear();
        public IReadOnlyCollection<ItemTemplateDefinition> GetAll() => _templates.Values.ToArray();
        public bool TryGet(string id, out ItemTemplateDefinition? definition) => _templates.TryGetValue(id, out definition);
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
        public Hue? GetHue(int index) => index >= 0 && index < _hues.Count ? _hues[index] : null;
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
            rarity: "Legendary",
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
```

- [ ] **Step 2: Run tests to verify RED**

Run:

```bash
dotnet test tests/Moongate.Tests/Moongate.Tests.csproj --filter "FullyQualifiedName~ItemTemplateEndpointExtensionsTests"
```

Expected: build fails because `ItemTemplateEndpointExtensions` does not exist.

- [ ] **Step 3: Implement list endpoint handler and mapping shell**

Create `src/Moongate.Server/Extensions/Endpoints/ItemTemplateEndpointExtensions.cs`:

```csharp
using Moongate.Core.Types;
using Moongate.Persistence.Data;
using Moongate.Server.Data.ListQueries;
using Moongate.Server.Data.Templates;
using Moongate.UO.Data.Interfaces.Hues;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Types.Items;

namespace Moongate.Server.Extensions.Endpoints;

public static class ItemTemplateEndpointExtensions
{
    public static IEndpointRouteBuilder MapMoongateItemTemplates(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin/item-templates")
                             .WithTags("Admin Item Templates")
                             .RequireAuthorization(policy => policy.RequireRole(nameof(UserLevelType.Administrator)));

        group.MapGet(
                 "/",
                 (
                     IItemTemplateService templates,
                     IHueStore hues,
                     HttpRequest request,
                     int? page,
                     int? pageSize,
                     string? search,
                     string? tag,
                     string? rarity,
                     string? layer
                 ) => HandleList(
                     templates,
                     hues,
                     page,
                     pageSize,
                     search,
                     tag,
                     rarity,
                     layer,
                     request.Query["abstract"].FirstOrDefault()
                 )
             )
             .WithName("ListItemTemplates")
             .WithSummary("Returns a paginated, searchable list of item templates.");

        return endpoints;
    }

    internal static IResult HandleList(
        IItemTemplateService templates,
        IHueStore hues,
        int? page,
        int? pageSize,
        string? search,
        string? tag,
        string? rarity,
        string? layer,
        string? abstractText
    )
    {
        ArgumentNullException.ThrowIfNull(templates);
        ArgumentNullException.ThrowIfNull(hues);

        if (!TryParseEnum<ItemRarity>(rarity, out var rarityFilter, out var rarityError))
        {
            return TypedResults.BadRequest(rarityError);
        }

        if (!TryParseEnum<ItemLayerType>(layer, out var layerFilter, out var layerError))
        {
            return TypedResults.BadRequest(layerError);
        }

        if (!TryParseOptionalBool(abstractText, out var abstractFilter))
        {
            return TypedResults.BadRequest("abstract must be true or false.");
        }

        var filters = new List<Func<ItemTemplateDefinition, bool>>();

        if (!string.IsNullOrWhiteSpace(tag))
        {
            filters.Add(template => template.Tags.Any(templateTag => string.Equals(
                templateTag,
                tag.Trim(),
                StringComparison.OrdinalIgnoreCase
            )));
        }

        if (rarityFilter.HasValue)
        {
            filters.Add(template => template.Rarity == rarityFilter.Value);
        }

        if (layerFilter.HasValue)
        {
            filters.Add(template => template.Layer == layerFilter.Value);
        }

        if (abstractFilter.HasValue)
        {
            filters.Add(template => template.IsAbstract == abstractFilter.Value);
        }

        var request = PageRequest.Normalize(page, pageSize, search);
        var ordered = templates.GetAll().OrderBy(static template => template.Id, StringComparer.OrdinalIgnoreCase);
        var result = InMemoryListQuery.Apply(ordered, request, SearchFields, filters);

        return TypedResults.Ok(result.Select(template => ItemTemplateSummary.FromDefinition(template, hues)));
    }

    private static IEnumerable<string?> SearchFields(ItemTemplateDefinition template)
    {
        yield return template.Id;
        yield return template.Name;
        yield return template.Comment;
        yield return template.ScriptId;
        yield return template.ItemId.ToString(System.Globalization.CultureInfo.InvariantCulture);
        yield return ItemTemplateSummary.FormatItemId(template.ItemId);
        yield return $"0x{template.ItemId:X}";

        foreach (var tag in template.Tags)
        {
            yield return tag;
        }
    }

    private static bool TryParseEnum<TEnum>(string? value, out TEnum? parsed, out string error)
        where TEnum : struct
    {
        parsed = null;
        error = "";

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (Enum.TryParse<TEnum>(value, true, out var result))
        {
            parsed = result;
            return true;
        }

        error = $"Unknown {typeof(TEnum).Name} '{value}'.";
        return false;
    }

    private static bool TryParseOptionalBool(string? value, out bool? parsed)
    {
        parsed = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (!bool.TryParse(value, out var result))
        {
            return false;
        }

        parsed = result;
        return true;
    }
}
```

- [ ] **Step 4: Run list tests to verify GREEN**

Run:

```bash
dotnet test tests/Moongate.Tests/Moongate.Tests.csproj --filter "FullyQualifiedName~ItemTemplateEndpointExtensionsTests"
```

Expected: PASS for list endpoint tests.

- [ ] **Step 5: Commit**

```bash
git add src/Moongate.Server/Extensions/Endpoints/ItemTemplateEndpointExtensions.cs tests/Moongate.Tests/Server/Endpoints/ItemTemplateEndpointExtensionsTests.cs
git commit -m "Add item template list endpoint"
```

---

### Task 4: Item Template Detail Endpoint and Hue Endpoint

**Files:**
- Modify: `src/Moongate.Server/Extensions/Endpoints/ItemTemplateEndpointExtensions.cs`
- Create: `src/Moongate.Server/Extensions/Endpoints/AdminHueEndpointExtensions.cs`
- Modify: `tests/Moongate.Tests/Server/Endpoints/ItemTemplateEndpointExtensionsTests.cs`
- Create: `tests/Moongate.Tests/Server/Endpoints/AdminHueEndpointExtensionsTests.cs`

- [ ] **Step 1: Add failing detail tests**

Append to `ItemTemplateEndpointExtensionsTests`:

```csharp
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
```

- [ ] **Step 2: Add failing hue endpoint tests**

Create `tests/Moongate.Tests/Server/Endpoints/AdminHueEndpointExtensionsTests.cs`:

```csharp
using Microsoft.AspNetCore.Http.HttpResults;
using Moongate.Server.Data.Templates;
using Moongate.Server.Extensions.Endpoints;
using Moongate.UO.Data.Data.Hues;
using Moongate.UO.Data.Interfaces.Hues;

namespace Moongate.Tests.Server.Endpoints;

public sealed class AdminHueEndpointExtensionsTests
{
    private sealed class FakeHueStore : IHueStore
    {
        private readonly List<Hue> _hues = [];
        public IReadOnlyList<Hue> Hues => _hues;
        public int Count => _hues.Count;
        public void Add(Hue hue) => _hues.Add(hue);
        public Hue? GetHue(int index) => index >= 0 && index < _hues.Count ? _hues[index] : null;
    }

    [Fact]
    public void HandleGetHue_Zero_ReturnsNone()
    {
        var result = AdminHueEndpointExtensions.HandleGetHue(new FakeHueStore(), 0);

        var ok = Assert.IsType<Ok<HueSummary>>(result);
        Assert.True(ok.Value!.IsNone);
    }

    [Fact]
    public void HandleGetHue_KnownHue_ReturnsColors()
    {
        var store = new FakeHueStore();
        store.Add(new(Enumerable.Repeat((ushort)0x7FFF, 32).ToArray(), 0, 31, "White"));

        var result = AdminHueEndpointExtensions.HandleGetHue(store, 1);

        var ok = Assert.IsType<Ok<HueSummary>>(result);
        Assert.True(ok.Value!.IsKnown);
        Assert.Equal(32, ok.Value.Colors.Count);
    }

    [Fact]
    public void HandleGetHue_UnknownNonZero_ReturnsNotFound()
    {
        var result = AdminHueEndpointExtensions.HandleGetHue(new FakeHueStore(), 99);

        Assert.IsType<NotFound>(result);
    }
}
```

- [ ] **Step 3: Run tests to verify RED**

Run:

```bash
dotnet test tests/Moongate.Tests/Moongate.Tests.csproj --filter "FullyQualifiedName~ItemTemplateEndpointExtensionsTests|FullyQualifiedName~AdminHueEndpointExtensionsTests"
```

Expected: build fails because `HandleDetail` and `AdminHueEndpointExtensions` do not exist.

- [ ] **Step 4: Implement detail handler**

Add to the mapped group in `ItemTemplateEndpointExtensions.MapMoongateItemTemplates`:

```csharp
        group.MapGet(
                 "/{id}",
                 (IItemTemplateService templates, IHueStore hues, string id) => HandleDetail(templates, hues, id)
             )
             .WithName("GetItemTemplate")
             .WithSummary("Returns a full read-only item template definition.");
```

Add this handler to `ItemTemplateEndpointExtensions`:

```csharp
    internal static IResult HandleDetail(
        IItemTemplateService templates,
        IHueStore hues,
        string id
    )
    {
        ArgumentNullException.ThrowIfNull(templates);
        ArgumentNullException.ThrowIfNull(hues);

        return templates.TryGet(id, out var template)
                   ? TypedResults.Ok(ItemTemplateDetail.FromDefinition(template, hues))
                   : TypedResults.NotFound();
    }
```

- [ ] **Step 5: Implement hue endpoint**

Create `src/Moongate.Server/Extensions/Endpoints/AdminHueEndpointExtensions.cs`:

```csharp
using Moongate.Core.Types;
using Moongate.Server.Data.Templates;
using Moongate.UO.Data.Interfaces.Hues;

namespace Moongate.Server.Extensions.Endpoints;

public static class AdminHueEndpointExtensions
{
    public static IEndpointRouteBuilder MapMoongateAdminHues(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin/hues")
                             .WithTags("Admin Hues")
                             .RequireAuthorization(policy => policy.RequireRole(nameof(UserLevelType.Administrator)));

        group.MapGet(
                 "/{hue:int}",
                 (IHueStore hues, int hue) => HandleGetHue(hues, hue)
             )
             .WithName("GetAdminHue")
             .WithSummary("Returns a UO hue palette descriptor.");

        return endpoints;
    }

    internal static IResult HandleGetHue(IHueStore hues, int hue)
    {
        ArgumentNullException.ThrowIfNull(hues);

        var summary = HueSummary.FromValue(hue, hues);

        return summary.IsKnown ? TypedResults.Ok(summary) : TypedResults.NotFound();
    }
}
```

- [ ] **Step 6: Run endpoint tests to verify GREEN**

Run:

```bash
dotnet test tests/Moongate.Tests/Moongate.Tests.csproj --filter "FullyQualifiedName~ItemTemplateEndpointExtensionsTests|FullyQualifiedName~AdminHueEndpointExtensionsTests"
```

Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add src/Moongate.Server/Extensions/Endpoints/ItemTemplateEndpointExtensions.cs src/Moongate.Server/Extensions/Endpoints/AdminHueEndpointExtensions.cs tests/Moongate.Tests/Server/Endpoints/ItemTemplateEndpointExtensionsTests.cs tests/Moongate.Tests/Server/Endpoints/AdminHueEndpointExtensionsTests.cs
git commit -m "Add item template detail and hue endpoints"
```

---

### Task 5: Wire Endpoints Into HTTP Pipeline

**Files:**
- Modify: `src/Moongate.Server/Bootstrap/WebHostExtensions.cs`

- [ ] **Step 1: Add endpoint mapping**

Modify `MapMoongateHttpPipeline` in `src/Moongate.Server/Bootstrap/WebHostExtensions.cs`:

```csharp
        app.MapMoongateAuth();
        app.MapMoongateAdminUsers();
        app.MapMoongateItemTemplates();
        app.MapMoongateAdminHues();
        app.MapMoongateVersion();
```

- [ ] **Step 2: Run backend tests**

Run:

```bash
dotnet test tests/Moongate.Tests/Moongate.Tests.csproj --filter "FullyQualifiedName~ItemTemplateEndpointExtensionsTests|FullyQualifiedName~AdminHueEndpointExtensionsTests|FullyQualifiedName~InMemoryListQueryTests|FullyQualifiedName~ItemTemplateApiModelTests"
```

Expected: PASS.

- [ ] **Step 3: Commit**

```bash
git add src/Moongate.Server/Bootstrap/WebHostExtensions.cs
git commit -m "Map item template admin endpoints"
```

---

### Task 6: Frontend Types and REST Client

**Files:**
- Create: `ui/src/types/itemTemplates.ts`
- Create: `ui/src/lib/adminItemTemplatesClient.ts`

- [ ] **Step 1: Add TypeScript API types**

Create `ui/src/types/itemTemplates.ts`:

```typescript
export type HueColorSummary = {
  index: number;
  r: number;
  g: number;
  b: number;
  hex: string;
};

export type HueSummary = {
  value: number;
  hex: string;
  name: string;
  isNone: boolean;
  isKnown: boolean;
  colors: HueColorSummary[];
};

export type ItemTemplateSummary = {
  id: string;
  name: string;
  itemId: number;
  itemIdHex: string;
  imageUrl: string;
  rarity: string;
  layer: string | null;
  tags: string[];
  isAbstract: boolean;
  hue: HueSummary;
};

export type ItemTemplateParamSummary = {
  key: string;
  type: string;
  value: string;
};

export type ItemTemplateDetail = ItemTemplateSummary & {
  comment: string;
  baseItem: string | null;
  scriptId: string;
  visibility: string;
  amount: number;
  weight: number;
  isStackable: boolean;
  isMovable: boolean;
  gumpId: number | null;
  params: ItemTemplateParamSummary[];
};

export type ItemTemplateFilters = {
  page: number;
  pageSize: number;
  search: string;
  tag: string;
  rarity: string;
  layer: string;
  abstract: "all" | "true" | "false";
};

export type PagedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};
```

- [ ] **Step 2: Add REST client**

Create `ui/src/lib/adminItemTemplatesClient.ts`:

```typescript
import { readJson } from "./authClient";
import type { ItemTemplateDetail, ItemTemplateFilters, ItemTemplateSummary, PagedResult } from "../types/itemTemplates";

function authHeaders(accessToken: string): HeadersInit {
  return { Authorization: `Bearer ${accessToken}` };
}

export async function listItemTemplates(
  accessToken: string,
  filters: ItemTemplateFilters
): Promise<PagedResult<ItemTemplateSummary>> {
  const params = new URLSearchParams({
    page: String(filters.page),
    pageSize: String(filters.pageSize)
  });

  if (filters.search.trim().length > 0) {
    params.set("search", filters.search.trim());
  }

  if (filters.tag.trim().length > 0) {
    params.set("tag", filters.tag.trim());
  }

  if (filters.rarity.trim().length > 0) {
    params.set("rarity", filters.rarity.trim());
  }

  if (filters.layer.trim().length > 0) {
    params.set("layer", filters.layer.trim());
  }

  if (filters.abstract !== "all") {
    params.set("abstract", filters.abstract);
  }

  const response = await fetch(`/api/admin/item-templates?${params.toString()}`, {
    headers: authHeaders(accessToken)
  });

  return readJson<PagedResult<ItemTemplateSummary>>(response);
}

export async function getItemTemplate(accessToken: string, id: string): Promise<ItemTemplateDetail> {
  const response = await fetch(`/api/admin/item-templates/${encodeURIComponent(id)}`, {
    headers: authHeaders(accessToken)
  });

  return readJson<ItemTemplateDetail>(response);
}
```

- [ ] **Step 3: Run TypeScript build**

Run:

```bash
npm --prefix ui run build
```

Expected: PASS.

- [ ] **Step 4: Commit**

```bash
git add ui/src/types/itemTemplates.ts ui/src/lib/adminItemTemplatesClient.ts
git commit -m "Add item template admin client"
```

---

### Task 7: HueSwatch and Item Image Components

**Files:**
- Create: `ui/src/components/admin/itemTemplates/HueSwatch.tsx`
- Create: `ui/src/components/admin/itemTemplates/ItemImageCell.tsx`

- [ ] **Step 1: Add HueSwatch component**

Create `ui/src/components/admin/itemTemplates/HueSwatch.tsx`:

```tsx
import type { HueSummary } from "../../../types/itemTemplates";

type HueSwatchProps = {
  hue: HueSummary;
  mode?: "compact" | "detail";
};

export function HueSwatch({ hue, mode = "compact" }: HueSwatchProps) {
  const label = hue.isNone ? "None" : hue.isKnown ? hue.hex : "Unknown";

  if (mode === "compact") {
    return (
      <span className="inline-flex min-w-[86px] items-center gap-2 text-xs text-fg-muted" title={`${hue.hex} ${hue.name}`}>
        <span className="h-3 w-10 overflow-hidden rounded-sm border border-border bg-muted" aria-hidden>
          {hue.colors.length > 0 && (
            <span
              className="block h-full w-full"
              style={{ background: `linear-gradient(90deg, ${hue.colors.map((color) => color.hex).join(", ")})` }}
            />
          )}
        </span>
        <span className="font-mono">{label}</span>
      </span>
    );
  }

  return (
    <div className="grid gap-2">
      <div className="flex flex-wrap items-center gap-2 text-sm">
        <span className="font-mono font-semibold text-fg">{hue.hex}</span>
        <span className="text-fg-muted">{hue.name}</span>
        {hue.isNone && <span className="rounded-full bg-muted px-2 py-0.5 text-[11px] font-semibold text-fg-muted">None</span>}
        {!hue.isKnown && <span className="rounded-full bg-warning/10 px-2 py-0.5 text-[11px] font-semibold text-warning">Unknown hue</span>}
      </div>
      {hue.colors.length > 0 && (
        <div
          className="grid overflow-hidden rounded-md border border-border"
          style={{ gridTemplateColumns: "repeat(32, minmax(0, 1fr))" }}
        >
          {hue.colors.map((color) => (
            <span
              key={color.index}
              className="h-5"
              title={`${color.index}: ${color.hex}`}
              style={{ backgroundColor: color.hex }}
            />
          ))}
        </div>
      )}
    </div>
  );
}
```

- [ ] **Step 2: Add item image component**

Create `ui/src/components/admin/itemTemplates/ItemImageCell.tsx`:

```tsx
import { useState } from "react";
import { ImageOff } from "lucide-react";

type ItemImageCellProps = {
  src: string;
  alt: string;
  size?: "small" | "large";
};

export function ItemImageCell({ src, alt, size = "small" }: ItemImageCellProps) {
  const [failed, setFailed] = useState(false);
  const boxClass = size === "large" ? "h-24 w-24" : "h-10 w-10";

  return (
    <div className={`${boxClass} inline-flex shrink-0 items-center justify-center rounded-md border border-border bg-muted`}>
      {failed ? (
        <ImageOff size={size === "large" ? 24 : 16} aria-hidden className="text-fg-subtle" />
      ) : (
        <img
          src={src}
          alt={alt}
          loading="lazy"
          onError={() => setFailed(true)}
          className="max-h-full max-w-full object-contain"
        />
      )}
    </div>
  );
}
```

- [ ] **Step 3: Run TypeScript build**

Run:

```bash
npm --prefix ui run build
```

Expected: PASS.

- [ ] **Step 4: Commit**

```bash
git add ui/src/components/admin/itemTemplates/HueSwatch.tsx ui/src/components/admin/itemTemplates/ItemImageCell.tsx
git commit -m "Add item template display primitives"
```

---

### Task 8: Item Template Table and Detail Panel

**Files:**
- Create: `ui/src/components/admin/itemTemplates/ItemTemplateTable.tsx`
- Create: `ui/src/components/admin/itemTemplates/ItemTemplateDetailPanel.tsx`

- [ ] **Step 1: Add table component**

Create `ui/src/components/admin/itemTemplates/ItemTemplateTable.tsx`:

```tsx
import type { ItemTemplateSummary } from "../../../types/itemTemplates";
import { HueSwatch } from "./HueSwatch";
import { ItemImageCell } from "./ItemImageCell";

type ItemTemplateTableProps = {
  templates: ItemTemplateSummary[];
  selectedId: string | null;
  onSelect: (template: ItemTemplateSummary) => void;
};

const rarityClass: Record<string, string> = {
  Common: "bg-muted text-fg-muted",
  Uncommon: "bg-info/10 text-info",
  Rare: "bg-warning/10 text-warning",
  None: "bg-muted text-fg-muted"
};

export function ItemTemplateTable({ templates, selectedId, onSelect }: ItemTemplateTableProps) {
  if (templates.length === 0) {
    return <p className="m-0 rounded-md bg-muted p-4 text-[13px] leading-relaxed text-fg-muted">No item templates match this search.</p>;
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full border-collapse text-sm">
        <thead>
          <tr className="border-b border-border text-left text-[11px] font-bold uppercase tracking-wide text-fg-subtle">
            <th className="px-3 py-2">Art</th>
            <th className="px-3 py-2">ID</th>
            <th className="px-3 py-2">Name</th>
            <th className="px-3 py-2">Item</th>
            <th className="px-3 py-2">Hue</th>
            <th className="px-3 py-2">Rarity</th>
            <th className="px-3 py-2">Layer</th>
            <th className="px-3 py-2">Tags</th>
            <th className="px-3 py-2">Abstract</th>
          </tr>
        </thead>
        <tbody>
          {templates.map((template) => (
            <tr
              key={template.id}
              onClick={() => onSelect(template)}
              className={`cursor-pointer border-b border-border/60 transition-colors duration-150 hover:bg-muted/70 ${selectedId === template.id ? "bg-muted" : ""}`}
            >
              <td className="px-3 py-2">
                <ItemImageCell src={template.imageUrl} alt={template.name || template.id} />
              </td>
              <td className="px-3 py-2 font-mono text-xs font-semibold text-fg">{template.id}</td>
              <td className="px-3 py-2 font-semibold text-fg">{template.name}</td>
              <td className="px-3 py-2 font-mono text-xs text-fg-muted">{template.itemIdHex}</td>
              <td className="px-3 py-2"><HueSwatch hue={template.hue} /></td>
              <td className="px-3 py-2">
                <span className={`inline-flex rounded-full px-2 py-0.5 text-[11px] font-semibold ${rarityClass[template.rarity] ?? rarityClass.None}`}>
                  {template.rarity}
                </span>
              </td>
              <td className="px-3 py-2 text-xs text-fg-muted">{template.layer ?? "-"}</td>
              <td className="px-3 py-2">
                <div className="flex max-w-[220px] flex-wrap gap-1">
                  {template.tags.map((tag) => (
                    <span key={tag} className="rounded-full bg-muted px-2 py-0.5 text-[11px] font-semibold text-fg-muted">{tag}</span>
                  ))}
                </div>
              </td>
              <td className="px-3 py-2 text-xs font-semibold text-fg-muted">{template.isAbstract ? "Yes" : "No"}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
```

- [ ] **Step 2: Add detail panel component**

Create `ui/src/components/admin/itemTemplates/ItemTemplateDetailPanel.tsx`:

```tsx
import { Box } from "lucide-react";
import type { ItemTemplateDetail } from "../../../types/itemTemplates";
import { DefinitionList } from "../DefinitionList";
import { HueSwatch } from "./HueSwatch";
import { ItemImageCell } from "./ItemImageCell";

type ItemTemplateDetailPanelProps = {
  template: ItemTemplateDetail | null;
  loading: boolean;
  error: string | null;
};

export function ItemTemplateDetailPanel({ template, loading, error }: ItemTemplateDetailPanelProps) {
  if (loading) {
    return <aside className="rounded-lg border border-border bg-surface p-5 text-sm font-semibold text-fg-muted shadow-card">Loading template…</aside>;
  }

  if (error) {
    return <aside className="rounded-lg border border-danger/20 bg-danger/10 p-5 text-sm font-semibold text-danger shadow-card">{error}</aside>;
  }

  if (!template) {
    return (
      <aside className="rounded-lg border border-border bg-surface p-5 text-sm text-fg-muted shadow-card">
        <div className="mb-3 inline-flex h-10 w-10 items-center justify-center rounded-md bg-muted">
          <Box size={20} aria-hidden />
        </div>
        <p className="m-0 font-semibold text-fg">Select an item template</p>
        <p className="m-0 mt-1 text-[13px] leading-relaxed">Choose a row to inspect its full read-only definition.</p>
      </aside>
    );
  }

  return (
    <aside className="rounded-lg border border-border bg-surface p-5 shadow-card">
      <div className="mb-4 flex items-start gap-4">
        <ItemImageCell src={template.imageUrl} alt={template.name || template.id} size="large" />
        <div className="min-w-0">
          <h3 className="m-0 text-base font-bold text-fg">{template.name || template.id}</h3>
          <p className="m-0 mt-1 font-mono text-xs text-fg-muted">{template.id} · {template.itemIdHex}</p>
          {template.comment && <p className="m-0 mt-2 text-[13px] leading-relaxed text-fg-muted">{template.comment}</p>}
        </div>
      </div>

      <div className="grid gap-4">
        <HueSwatch hue={template.hue} mode="detail" />
        <DefinitionList
          items={[
            { term: "Base item", value: template.baseItem ?? "-", mono: true },
            { term: "Script", value: template.scriptId || "-", mono: true },
            { term: "Visibility", value: template.visibility },
            { term: "Layer", value: template.layer ?? "-" },
            { term: "Rarity", value: template.rarity },
            { term: "Amount", value: String(template.amount), mono: true },
            { term: "Weight", value: String(template.weight), mono: true },
            { term: "Movable", value: template.isMovable ? "Yes" : "No" },
            { term: "Stackable", value: template.isStackable ? "Yes" : "No" },
            { term: "Gump", value: template.gumpId?.toString() ?? "-", mono: true },
            { term: "Abstract", value: template.isAbstract ? "Yes" : "No" }
          ]}
        />
        <section>
          <h4 className="mb-2 text-xs font-bold uppercase tracking-wide text-fg-subtle">Tags</h4>
          <div className="flex flex-wrap gap-1.5">
            {template.tags.length === 0 ? <span className="text-xs text-fg-muted">No tags</span> : template.tags.map((tag) => (
              <span key={tag} className="rounded-full bg-muted px-2 py-0.5 text-[11px] font-semibold text-fg-muted">{tag}</span>
            ))}
          </div>
        </section>
        <section>
          <h4 className="mb-2 text-xs font-bold uppercase tracking-wide text-fg-subtle">Params</h4>
          {template.params.length === 0 ? (
            <span className="text-xs text-fg-muted">No params</span>
          ) : (
            <div className="grid gap-2">
              {template.params.map((param) => (
                <div key={param.key} className="rounded-md border border-border bg-muted p-2">
                  <div className="flex items-center justify-between gap-2">
                    <span className="font-mono text-xs font-semibold text-fg">{param.key}</span>
                    <span className="text-[11px] font-semibold text-fg-muted">{param.type}</span>
                  </div>
                  <p className="m-0 mt-1 break-all font-mono text-xs text-fg-muted">{param.value}</p>
                </div>
              ))}
            </div>
          )}
        </section>
      </div>
    </aside>
  );
}
```

- [ ] **Step 3: Run TypeScript build**

Run:

```bash
npm --prefix ui run build
```

Expected: PASS.

- [ ] **Step 4: Commit**

```bash
git add ui/src/components/admin/itemTemplates/ItemTemplateTable.tsx ui/src/components/admin/itemTemplates/ItemTemplateDetailPanel.tsx
git commit -m "Add item template table and detail panel"
```

---

### Task 9: Item Template Catalog Panel

**Files:**
- Create: `ui/src/components/admin/itemTemplates/ItemTemplateCatalogPanel.tsx`

- [ ] **Step 1: Add catalog panel**

Create `ui/src/components/admin/itemTemplates/ItemTemplateCatalogPanel.tsx`:

```tsx
import { useCallback, useEffect, useState } from "react";
import { RefreshCw, Search } from "lucide-react";
import { getItemTemplate, listItemTemplates } from "../../../lib/adminItemTemplatesClient";
import type { ItemTemplateDetail, ItemTemplateFilters, ItemTemplateSummary } from "../../../types/itemTemplates";
import { Panel } from "../Panel";
import { ItemTemplateDetailPanel } from "./ItemTemplateDetailPanel";
import { ItemTemplateTable } from "./ItemTemplateTable";

type ItemTemplateCatalogPanelProps = {
  accessToken: string;
};

const PAGE_SIZE = 50;

const defaultFilters: ItemTemplateFilters = {
  page: 1,
  pageSize: PAGE_SIZE,
  search: "",
  tag: "",
  rarity: "",
  layer: "",
  abstract: "all"
};

export function ItemTemplateCatalogPanel({ accessToken }: ItemTemplateCatalogPanelProps) {
  const [filters, setFilters] = useState<ItemTemplateFilters>(defaultFilters);
  const [search, setSearch] = useState("");
  const [templates, setTemplates] = useState<ItemTemplateSummary[]>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [detail, setDetail] = useState<ItemTemplateDetail | null>(null);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [detailLoading, setDetailLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [detailError, setDetailError] = useState<string | null>(null);

  useEffect(() => {
    const timer = window.setTimeout(() => {
      setFilters((current) => ({ ...current, search, page: 1 }));
    }, 300);

    return () => window.clearTimeout(timer);
  }, [search]);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const result = await listItemTemplates(accessToken, filters);
      setTemplates(result.items);
      setTotalPages(Math.max(1, result.totalPages));
      setTotalCount(result.totalCount);

      if (selectedId && !result.items.some((item) => item.id === selectedId)) {
        setSelectedId(null);
        setDetail(null);
      }
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : "Failed to load item templates");
      setTemplates([]);
    } finally {
      setLoading(false);
    }
  }, [accessToken, filters, selectedId]);

  useEffect(() => {
    void load();
  }, [load]);

  async function selectTemplate(template: ItemTemplateSummary) {
    setSelectedId(template.id);
    setDetailLoading(true);
    setDetailError(null);

    try {
      setDetail(await getItemTemplate(accessToken, template.id));
    } catch (caught) {
      setDetailError(caught instanceof Error ? caught.message : "Failed to load template detail");
    } finally {
      setDetailLoading(false);
    }
  }

  function updateFilter<K extends keyof ItemTemplateFilters>(key: K, value: ItemTemplateFilters[K]) {
    setFilters((current) => ({ ...current, [key]: value, page: 1 }));
  }

  return (
    <Panel
      title="Item Templates"
      action={
        <button
          type="button"
          onClick={() => void load()}
          className="inline-flex min-h-[34px] items-center gap-1.5 rounded-md border border-border bg-surface px-3 text-[13px] font-semibold text-fg transition-colors duration-150 hover:bg-muted"
        >
          <RefreshCw size={15} aria-hidden />
          Refresh
        </button>
      }
    >
      <div className="grid gap-4">
        <div className="grid gap-3 xl:grid-cols-[minmax(240px,1fr)_160px_160px_160px_140px]">
          <label className="relative">
            <Search size={16} aria-hidden className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-fg-subtle" />
            <input
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Search id, name, comment, tag, script or item id…"
              aria-label="Search item templates"
              className="h-10 w-full rounded-md border border-border bg-surface pl-9 pr-3 text-sm text-fg outline-none focus:border-accent"
            />
          </label>
          <input value={filters.tag} onChange={(event) => updateFilter("tag", event.target.value)} placeholder="Tag" className="h-10 rounded-md border border-border bg-surface px-3 text-sm text-fg outline-none focus:border-accent" />
          <input value={filters.rarity} onChange={(event) => updateFilter("rarity", event.target.value)} placeholder="Rarity" className="h-10 rounded-md border border-border bg-surface px-3 text-sm text-fg outline-none focus:border-accent" />
          <input value={filters.layer} onChange={(event) => updateFilter("layer", event.target.value)} placeholder="Layer" className="h-10 rounded-md border border-border bg-surface px-3 text-sm text-fg outline-none focus:border-accent" />
          <select value={filters.abstract} onChange={(event) => updateFilter("abstract", event.target.value as ItemTemplateFilters["abstract"])} className="h-10 rounded-md border border-border bg-surface px-3 text-sm text-fg outline-none focus:border-accent">
            <option value="all">All</option>
            <option value="false">Concrete</option>
            <option value="true">Abstract</option>
          </select>
        </div>

        {error && <p className="m-0 rounded-md bg-danger/10 p-3 text-[13px] font-semibold text-danger">{error}</p>}

        <div className="grid gap-4 xl:grid-cols-[minmax(0,1fr)_360px]">
          <div className="min-w-0">
            {loading ? (
              <p className="m-0 rounded-md bg-muted p-4 text-[13px] font-semibold text-fg-muted">Loading item templates…</p>
            ) : (
              <ItemTemplateTable templates={templates} selectedId={selectedId} onSelect={selectTemplate} />
            )}

            <div className="mt-4 flex items-center justify-between text-xs text-fg-muted">
              <span className="font-mono">{totalCount} templates</span>
              <div className="flex items-center gap-2">
                <button type="button" disabled={filters.page <= 1} onClick={() => setFilters((current) => ({ ...current, page: Math.max(1, current.page - 1) }))} className="inline-flex min-h-[32px] items-center rounded-md border border-border bg-surface px-2.5 font-semibold text-fg transition-colors duration-150 hover:bg-muted disabled:opacity-50">Prev</button>
                <span className="font-mono">Page {filters.page} of {totalPages}</span>
                <button type="button" disabled={filters.page >= totalPages} onClick={() => setFilters((current) => ({ ...current, page: Math.min(totalPages, current.page + 1) }))} className="inline-flex min-h-[32px] items-center rounded-md border border-border bg-surface px-2.5 font-semibold text-fg transition-colors duration-150 hover:bg-muted disabled:opacity-50">Next</button>
              </div>
            </div>
          </div>

          <ItemTemplateDetailPanel template={detail} loading={detailLoading} error={detailError} />
        </div>
      </div>
    </Panel>
  );
}
```

- [ ] **Step 2: Run TypeScript build**

Run:

```bash
npm --prefix ui run build
```

Expected: PASS.

- [ ] **Step 3: Commit**

```bash
git add ui/src/components/admin/itemTemplates/ItemTemplateCatalogPanel.tsx
git commit -m "Add item template catalog panel"
```

---

### Task 10: Wire Admin Navigation

**Files:**
- Modify: `ui/src/types/admin.ts`
- Modify: `ui/src/data/navigation.ts`
- Modify: `ui/src/pages/AdminDashboard.tsx`

- [ ] **Step 1: Add nav id**

In `ui/src/types/admin.ts`, update `AdminNavId`:

```typescript
export type AdminNavId = "overview" | "runtime" | "metrics" | "persistence" | "security" | "users" | "itemTemplates" | "console";
```

- [ ] **Step 2: Add navigation item**

In `ui/src/data/navigation.ts`, import `PackageSearch` and add the item before Console:

```typescript
import { Activity, ChartSpline, Gauge, KeyRound, PackageSearch, ScrollText, Sparkles, TerminalSquare, Users, UserRound } from "lucide-react";
```

```typescript
  {
    id: "itemTemplates",
    label: "Item Templates",
    icon: PackageSearch
  },
```

- [ ] **Step 3: Render the admin view**

In `ui/src/pages/AdminDashboard.tsx`, add import:

```typescript
import { ItemTemplateCatalogPanel } from "../components/admin/itemTemplates/ItemTemplateCatalogPanel";
```

Render it near the users branch:

```tsx
        {activeView === "users" && <UserManagementPanel accessToken={accessToken} />}

        {activeView === "itemTemplates" && <ItemTemplateCatalogPanel accessToken={accessToken} />}
```

- [ ] **Step 4: Run TypeScript build**

Run:

```bash
npm --prefix ui run build
```

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add ui/src/types/admin.ts ui/src/data/navigation.ts ui/src/pages/AdminDashboard.tsx
git commit -m "Add item template admin navigation"
```

---

### Task 11: Full Verification and PR Prep

**Files:**
- No new source files expected unless fixing failures revealed by verification.

- [ ] **Step 1: Run backend tests**

Run:

```bash
dotnet test
```

Expected: PASS. Existing analyzer warnings may appear, but no test failures.

- [ ] **Step 2: Run frontend build**

Run:

```bash
npm --prefix ui run build
```

Expected: PASS.

- [ ] **Step 3: Inspect diff**

Run:

```bash
git diff --check
git status --short
```

Expected: `git diff --check` prints nothing. `git status --short` should show only intended files if there are uncommitted fixes.

- [ ] **Step 4: Commit verification fixes**

Run:

```bash
git status --short
```

Expected: no output because each implementation task committed its own changes. If this prints files, return to the task that introduced those changes, finish its RED/GREEN cycle, and commit that task before continuing.

- [ ] **Step 5: Push branch**

```bash
git push
```

Expected: branch `feature/item-template-rest-ui` updates on origin.

- [ ] **Step 6: Open PR**

```bash
gh pr create --base develop --head feature/item-template-rest-ui --title "Expose item templates in admin UI" --body "## Summary
- add admin-only item template REST endpoints with server-side search and filters
- add hue descriptors backed by UO hue data
- add Admin UI item template catalog with images, hue swatches, and detail panel

## Tests
- dotnet test
- npm --prefix ui run build"
```

Expected: PR URL returned. Do not add a prerelease publish label unless explicitly requested.

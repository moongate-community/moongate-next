using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moongate.Persistence.Data;
using Moongate.Server.Data.Templates;
using Moongate.Server.Extensions.Endpoints;
using Moongate.UO.Data.Data.Hues;
using Moongate.UO.Data.Interfaces.Hues;

namespace Moongate.Tests.Server.Endpoints;

public sealed class AdminHueEndpointExtensionsTests
{
    private sealed class FakeHueStore : IHueStore
    {
        private readonly List<Hue> _hues;

        public FakeHueStore(params string[] names)
        {
            _hues = names.Select(name => new Hue(new ushort[32], 0, 31, name)).ToList();
        }

        public IReadOnlyList<Hue> Hues => _hues;

        public int Count => _hues.Count;

        public Hue? GetHue(int index)
            => index >= 0 && index < _hues.Count ? _hues[index] : null;
    }

    private static PagedResult<HueSummary> Ok(IResult result)
        => Assert.IsType<Ok<PagedResult<HueSummary>>>(result).Value!;

    [Fact]
    public void HandleGetHue_KnownHue_ReturnsColors()
    {
        var store = new FakeHueStore("White");

        var result = AdminHueEndpointExtensions.HandleGetHue(store, 1);

        var ok = Assert.IsType<Ok<HueSummary>>(result);
        Assert.True(ok.Value!.IsKnown);
        Assert.Equal(32, ok.Value!.Colors.Count);
    }

    [Fact]
    public void HandleGetHue_UnknownNonZero_ReturnsNotFound()
    {
        var result = AdminHueEndpointExtensions.HandleGetHue(new FakeHueStore(), 99);

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public void HandleGetHue_Zero_ReturnsNone()
    {
        var result = AdminHueEndpointExtensions.HandleGetHue(new FakeHueStore(), 0);

        var ok = Assert.IsType<Ok<HueSummary>>(result);
        Assert.True(ok.Value!.IsNone);
    }

    [Fact]
    public void HandleListHues_ProjectsValuesOneBased()
    {
        var page = Ok(AdminHueEndpointExtensions.HandleListHues(new FakeHueStore("Red", "Blue"), null, null, null));

        Assert.Equal(2, page.TotalCount);
        Assert.Equal(1, page.Items[0].Value); // packet id = index + 1
        Assert.Equal("Red", page.Items[0].Name);
        Assert.Equal(2, page.Items[1].Value);
    }

    [Fact]
    public void HandleListHues_Pagination_Bounds()
    {
        var store = new FakeHueStore("a", "b", "c", "d", "e");

        var page = Ok(AdminHueEndpointExtensions.HandleListHues(store, 2, 2, null));

        Assert.Equal(2, page.Items.Count);
        Assert.Equal(5, page.TotalCount);
        Assert.Equal(3, page.Items[0].Value); // page 2 size 2 → values 3,4
    }

    [Fact]
    public void HandleListHues_SearchByName_IsCaseInsensitive()
        => Assert.Equal(1, Ok(AdminHueEndpointExtensions.HandleListHues(new FakeHueStore("Crimson", "Azure"), null, null, "crim")).TotalCount);

    [Fact]
    public void HandleListHues_SearchByValue_Filters()
    {
        var page = Ok(AdminHueEndpointExtensions.HandleListHues(new FakeHueStore("a", "b", "c"), null, null, "2"));

        Assert.Equal(1, page.TotalCount);
        Assert.Equal(2, page.Items[0].Value);
    }
}

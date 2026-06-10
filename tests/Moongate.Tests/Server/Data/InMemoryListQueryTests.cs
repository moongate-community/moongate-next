using Moongate.Persistence.Data;
using Moongate.Server.Data.ListQueries;

namespace Moongate.Tests.Server.Data;

public sealed class InMemoryListQueryTests
{
    private sealed record Entry(string Id, string Name, string Tag, bool Enabled);

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
}

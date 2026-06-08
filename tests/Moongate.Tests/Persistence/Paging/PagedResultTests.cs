using Moongate.Persistence.Data;

namespace Moongate.Tests.Persistence.Paging;

public sealed class PagedResultTests
{
    [Fact]
    public void Select_ProjectsItems_AndKeepsPageMetadata()
    {
        var page = new PagedResult<int>([1, 2], 2, 20, 42);

        var mapped = page.Select(value => value.ToString());

        Assert.Equal(["1", "2"], mapped.Items);
        Assert.Equal(2, mapped.Page);
        Assert.Equal(20, mapped.PageSize);
        Assert.Equal(42, mapped.TotalCount);
    }

    [Fact]
    public void TotalPages_RoundsUp_AndIsZeroForEmpty()
    {
        var page = new PagedResult<int>([1, 2], 1, 20, 41);
        var empty = new PagedResult<int>([], 1, 20, 0);

        Assert.Equal(3, page.TotalPages);
        Assert.Equal(0, empty.TotalPages);
    }
}

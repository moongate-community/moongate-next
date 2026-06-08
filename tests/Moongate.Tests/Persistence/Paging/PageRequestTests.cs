using Moongate.Persistence.Data;

namespace Moongate.Tests.Persistence.Paging;

public sealed class PageRequestTests
{
    [Fact]
    public void Normalize_ClampsPageAndPageSizeAndTrimsSearch()
    {
        var request = PageRequest.Normalize(0, 9999, "  bob  ");

        Assert.Equal(1, request.Page);
        Assert.Equal(100, request.PageSize);
        Assert.Equal("bob", request.Search);
    }

    [Fact]
    public void Normalize_EmptySearchBecomesNull_AndDefaultsApply()
    {
        var request = PageRequest.Normalize(null, null, "   ");

        Assert.Equal(1, request.Page);
        Assert.Equal(20, request.PageSize);
        Assert.Null(request.Search);
    }
}

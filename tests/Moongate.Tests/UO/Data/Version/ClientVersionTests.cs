using Moongate.Abstractions.Data.Version;

namespace Moongate.Tests.UO.Data.Version;

public class ClientVersionTests
{
    [Fact]
    public void Compare_OrdersByComponents()
    {
        Assert.True(new ClientVersion("7.0.9.0") > new ClientVersion("7.0.0.0"));
        Assert.True(new ClientVersion("7.0.0.0") < new ClientVersion("7.0.9.0"));
        Assert.True(new ClientVersion("7.0.9.0") >= new ClientVersion("7.0.9.0"));
        Assert.Equal(new("7.0.9.0"), new ClientVersion("7.0.9.0"));
        Assert.NotEqual(new("7.0.9.0"), new ClientVersion("7.0.8.0"));
    }

    [Fact]
    public void Parse_NumericVersion()
    {
        var v = new ClientVersion("7.0.9.0");

        Assert.Equal(7, v.Major);
        Assert.Equal(0, v.Minor);
        Assert.Equal(9, v.Revision);
        Assert.Equal(0, v.Patch);
    }

    [Fact]
    public void Parse_PatchLetter()
    {
        var v = new ClientVersion("5.0.2b");

        Assert.Equal(5, v.Major);
        Assert.Equal(0, v.Minor);
        Assert.Equal(2, v.Revision);
        Assert.Equal(2, v.Patch); // 'b' -> 2
    }
}

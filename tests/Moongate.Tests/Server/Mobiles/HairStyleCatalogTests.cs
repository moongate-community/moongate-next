using Moongate.Server.Data.Mobiles;

namespace Moongate.Tests.Server.Mobiles;

public sealed class HairStyleCatalogTests
{
    [Fact]
    public void Entries_HaveHexMatchingId_AndNonBlankName()
    {
        foreach (var entry in HairStyleCatalog.Hair.Concat(HairStyleCatalog.Facial))
        {
            Assert.Equal($"0x{entry.Style:X4}", entry.StyleHex);
            Assert.False(string.IsNullOrWhiteSpace(entry.Name));
        }
    }

    [Fact]
    public void Facial_HasSevenStyles_AllFacial()
    {
        Assert.Equal(7, HairStyleCatalog.Facial.Count);
        Assert.All(HairStyleCatalog.Facial, entry => Assert.True(entry.IsFacial));
    }

    [Fact]
    public void Hair_And_Facial_DoNotOverlap()
    {
        var hairIds = HairStyleCatalog.Hair.Select(entry => entry.Style).ToHashSet();

        Assert.DoesNotContain(HairStyleCatalog.Facial, entry => hairIds.Contains(entry.Style));
    }

    [Fact]
    public void Hair_HasTenHumanStyles_AllNonFacial()
    {
        Assert.Equal(10, HairStyleCatalog.Hair.Count);
        Assert.All(HairStyleCatalog.Hair, entry => Assert.False(entry.IsFacial));
    }
}

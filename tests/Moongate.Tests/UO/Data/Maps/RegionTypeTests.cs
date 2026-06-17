using Moongate.Server.Data.World;
using Moongate.UO.Data.Types.Maps;

namespace Moongate.Tests.UO.Data.Maps;

public sealed class RegionTypeTests
{
    [Theory]
    [InlineData("BaseRegion", RegionType.Base)]
    [InlineData("DungeonRegion", RegionType.Dungeon)]
    [InlineData("GreenAcresRegion", RegionType.GreenAcres)]
    [InlineData("GuardedRegion", RegionType.Guarded)]
    [InlineData("JailRegion", RegionType.Jail)]
    [InlineData("NoHousingRegion", RegionType.NoHousing)]
    [InlineData("TownRegion", RegionType.Town)]
    [InlineData("dungeonregion", RegionType.Dungeon)]
    [InlineData("Nonsense", RegionType.Unknown)]
    [InlineData("", RegionType.Unknown)]
    [InlineData(null, RegionType.Unknown)]
    public void FromAssetType_MapsKnownStrings(string? assetType, RegionType expected)
    {
        Assert.Equal(expected, RegionTypeParser.FromAssetType(assetType));
    }

    [Fact]
    public void RegionAreaEntry_Contains_IsInclusiveAndOrderRobust()
    {
        var area = new RegionAreaEntry(0, 0, 10, 10);

        Assert.True(area.Contains(5, 5));
        Assert.True(area.Contains(0, 10)); // inclusive
        Assert.False(area.Contains(11, 5));
        Assert.True(new RegionAreaEntry(10, 10, 0, 0).Contains(5, 5)); // order-robust
    }

    [Fact]
    public void RegionEntry_Kind_ReflectsType()
    {
        var entry = new RegionEntry(
            "JailRegion",
            0,
            "Felucca",
            "Jail",
            100,
            new[] { new RegionAreaEntry(0, 0, 10, 10) },
            "",
            null,
            null
        );

        Assert.Equal(RegionType.Jail, entry.Kind);
    }
}

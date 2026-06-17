using Moongate.Server.Data.World;
using Moongate.Server.Data.World.Internal;
using Moongate.UO.Data.Utils;

namespace Moongate.Tests.Server.World;

public sealed class RegionSectorIndexTests
{
    [Fact]
    public void Build_DifferentMaps_AreSeparateBuckets()
    {
        var fel = Region("f", 0, 1, new RegionAreaEntry(0, 0, 5, 5));
        var tram = Region("t", 1, 1, new RegionAreaEntry(0, 0, 5, 5));
        var index = RegionSectorIndex.Build(new[] { fel, tram });

        Assert.Single(index[(0, 0, 0)]);
        Assert.Single(index[(1, 0, 0)]);
    }

    [Fact]
    public void Build_MultipleRectsSameSector_DedupesRegion()
    {
        var region = Region(
            "R",
            0,
            1,
            new RegionAreaEntry(0, 0, 5, 5),
            new RegionAreaEntry(6, 6, 10, 10)
        ); // both in sector (0,0)
        var index = RegionSectorIndex.Build(new[] { region });

        Assert.Single(index[(0, 0, 0)]);
    }

    [Fact]
    public void Build_RegionSpanningSectors_LandsInEveryCoveredSector()
    {
        var region = Region("R", 0, 1, new RegionAreaEntry(0, 0, 40, 40)); // 40>>4==2 => sectors 0..2
        var index = RegionSectorIndex.Build(new[] { region });

        for (var sx = 0; sx <= 2; sx++)
        {
            for (var sy = 0; sy <= 2; sy++)
            {
                Assert.True(index.ContainsKey((0, sx, sy)), $"missing sector {sx},{sy}");
                Assert.Single(index[(0, sx, sy)]);
            }
        }
    }

    [Fact]
    public void Build_SameSector_OrdersByPriorityDescending()
    {
        var low = Region("low", 0, 1, new RegionAreaEntry(0, 0, 5, 5));
        var high = Region("high", 0, 9, new RegionAreaEntry(0, 0, 5, 5));
        var index = RegionSectorIndex.Build(new[] { low, high });

        var bucket = index[(0, 0, 0)];
        Assert.Equal("high", bucket[0].Name);
        Assert.Equal("low", bucket[1].Name);
    }

    [Fact]
    public void Build_UnnormalizedRect_IsBucketedByMinMax()
    {
        // X1>X2 and Y1>Y2: must span sectors 0..2 just like the normalized form
        var region = Region("R", 0, 1, new RegionAreaEntry(40, 40, 0, 0));
        var index = RegionSectorIndex.Build(new[] { region });

        for (var sx = 0; sx <= 2; sx++)
        {
            for (var sy = 0; sy <= 2; sy++)
            {
                Assert.True(index.ContainsKey((0, sx, sy)), $"missing sector {sx},{sy}");
            }
        }
    }

    [Fact]
    public void SectorShift_IsLog2OfSectorSize()
    {
        Assert.Equal(4, MapSectorConsts.SectorShift);
        // 16 == 2^4
    }

    private static RegionEntry Region(string name, int mapId, int priority, params RegionAreaEntry[] area)
    {
        return new RegionEntry("BaseRegion", mapId, "Map", name, priority, area, "", null, null);
    }
}

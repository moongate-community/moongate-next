using Moongate.Server.Data.World;
using Moongate.Server.Interfaces.Services.World;
using Moongate.Server.Services.World;
using Moongate.UO.Data.Types.Maps;

namespace Moongate.Tests.Server.World;

public sealed class RegionResolverServiceTests
{
    private sealed class FakeRegionDataService : IRegionDataService
    {
        private readonly List<RegionEntry> _entries;

        public FakeRegionDataService(params RegionEntry[] entries)
        {
            _entries = entries.ToList();
        }

        public bool IsLazy => true;

        public bool IsLoaded => true;

        public void EnsureLoaded() { }

        public IReadOnlyList<RegionEntry> GetAllEntries()
            => _entries;

        public void Reload() { }

        public void SetEntries(IReadOnlyList<RegionEntry> entries)
            => throw new NotSupportedException();
    }

    [Fact]
    public void GetMusic_ReturnsResolvedRegionMusicType()
    {
        var data = new FakeRegionDataService(RegionWithMusic("withMusic", 0, 1, new(0, 0, 10, 10), "Britain1"));
        var resolver = new RegionResolverService(data);

        Assert.Equal(MusicType.Britain1, resolver.GetMusic(0, new(5, 5, 0)));
        Assert.Equal(MusicType.NoMusic, resolver.GetMusic(0, new(999, 999, 0)));
    }

    [Fact]
    public void GetRegionByName_NullName_Throws()
    {
        var resolver = new RegionResolverService(new FakeRegionDataService());

        Assert.Throws<ArgumentNullException>(() => resolver.GetRegionByName(null!));
    }

    [Fact]
    public void GetRegionByName_ReturnsFirstCaseInsensitiveMatch()
    {
        var data = new FakeRegionDataService(Region("Britain", 0, 1, new RegionAreaEntry(0, 0, 10, 10)));
        var resolver = new RegionResolverService(data);

        Assert.Equal("Britain", resolver.GetRegionByName("britain")!.Value.Name);
        Assert.Null(resolver.GetRegionByName("Nowhere"));
    }

    [Fact]
    public void ResolveRegion_MatchesBruteForceScan()
    {
        var regions = new[]
        {
            Region("a", 0, 10, new RegionAreaEntry(0, 0, 80, 80)),
            Region("b", 0, 20, new RegionAreaEntry(30, 30, 50, 50)),
            Region("c", 0, 5, new RegionAreaEntry(70, 0, 120, 40)),
            Region("d", 1, 99, new RegionAreaEntry(0, 0, 200, 200))
        };
        var resolver = new RegionResolverService(new FakeRegionDataService(regions));

        for (var mapId = 0; mapId <= 1; mapId++)
        {
            for (var x = 0; x <= 130; x += 4) // step 4 hits every sector boundary (multiples of 16) plus interior tiles
            {
                for (var y = 0; y <= 130; y += 4)
                {
                    var expected = BruteForce(regions, mapId, x, y);
                    var actual = resolver.ResolveRegion(mapId, new(x, y, 0));

                    Assert.Equal(expected?.Name, actual?.Name);
                }
            }
        }
    }

    [Fact]
    public void ResolveRegion_NoContainmentOrWrongMap_ReturnsNull()
    {
        var data = new FakeRegionDataService(Region("r", 0, 1, new RegionAreaEntry(0, 0, 10, 10)));
        var resolver = new RegionResolverService(data);

        Assert.Null(resolver.ResolveRegion(0, new(999, 999, 0)));
        Assert.Null(resolver.ResolveRegion(1, new(5, 5, 0)));
    }

    [Fact]
    public void ResolveRegion_PicksHighestPriorityContainingRegion()
    {
        var data = new FakeRegionDataService(
            Region("broad", 0, 1, new RegionAreaEntry(0, 0, 100, 100)),
            Region("inner", 0, 50, new RegionAreaEntry(40, 40, 60, 60))
        );
        var resolver = new RegionResolverService(data);

        Assert.Equal("inner", resolver.ResolveRegion(0, new(50, 50, 0))!.Value.Name); // not "broad"
        Assert.Equal("broad", resolver.ResolveRegion(0, new(10, 10, 0))!.Value.Name);
    }

    private static RegionEntry? BruteForce(IReadOnlyList<RegionEntry> regions, int mapId, int x, int y)
        => regions
           .Where(r => r.MapId == mapId && r.Area.Any(a => a.Contains(x, y)))
           .OrderByDescending(r => r.Priority)
           .Select(r => (RegionEntry?)r)
           .FirstOrDefault();

    private static RegionEntry Region(string name, int mapId, int priority, params RegionAreaEntry[] area)
        => new("BaseRegion", mapId, "Map", name, priority, area, "", null, null);

    private static RegionEntry RegionWithMusic(string name, int mapId, int priority, RegionAreaEntry area, string music)
        => new("BaseRegion", mapId, "Map", name, priority, new[] { area }, music, null, null);
}

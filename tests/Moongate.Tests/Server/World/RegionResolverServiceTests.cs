using Moongate.Core.Geometry;
using Moongate.Server.Data.World;
using Moongate.Server.Interfaces.Services.World;
using Moongate.Server.Services.World;
using Xunit;

namespace Moongate.Tests.Server.World;

public sealed class RegionResolverServiceTests
{
    [Fact]
    public void ResolveRegion_PicksHighestPriorityContainingRegion()
    {
        var data = new FakeRegionDataService(
            Region("broad", 0, 1, new RegionAreaEntry(0, 0, 100, 100)),
            Region("inner", 0, 50, new RegionAreaEntry(40, 40, 60, 60)));
        var resolver = new RegionResolverService(data);

        Assert.Equal("inner", resolver.ResolveRegion(0, new Point3D(50, 50, 0))!.Value.Name); // not "broad"
        Assert.Equal("broad", resolver.ResolveRegion(0, new Point3D(10, 10, 0))!.Value.Name);
    }

    [Fact]
    public void GetRegionByName_NullName_Throws()
    {
        var resolver = new RegionResolverService(new FakeRegionDataService());

        Assert.Throws<ArgumentNullException>(() => resolver.GetRegionByName(null!));
    }

    [Fact]
    public void ResolveRegion_NoContainmentOrWrongMap_ReturnsNull()
    {
        var data = new FakeRegionDataService(Region("r", 0, 1, new RegionAreaEntry(0, 0, 10, 10)));
        var resolver = new RegionResolverService(data);

        Assert.Null(resolver.ResolveRegion(0, new Point3D(999, 999, 0)));
        Assert.Null(resolver.ResolveRegion(1, new Point3D(5, 5, 0)));
    }

    [Fact]
    public void GetMusic_ReturnsResolvedRegionMusicOrEmpty()
    {
        var data = new FakeRegionDataService(
            RegionWithMusic("withMusic", 0, 1, new RegionAreaEntry(0, 0, 10, 10), "Britain"));
        var resolver = new RegionResolverService(data);

        Assert.Equal("Britain", resolver.GetMusic(0, new Point3D(5, 5, 0)));
        Assert.Equal("", resolver.GetMusic(0, new Point3D(999, 999, 0)));
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
            Region("c", 0, 5,  new RegionAreaEntry(70, 0, 120, 40)),
            Region("d", 1, 99, new RegionAreaEntry(0, 0, 200, 200))
        };
        var resolver = new RegionResolverService(new FakeRegionDataService(regions));

        for (var mapId = 0; mapId <= 1; mapId++)
        {
            for (var x = 0; x <= 130; x += 7)
            {
                for (var y = 0; y <= 130; y += 7)
                {
                    var expected = BruteForce(regions, mapId, x, y);
                    var actual = resolver.ResolveRegion(mapId, new Point3D(x, y, 0));

                    Assert.Equal(expected?.Name, actual?.Name);
                }
            }
        }
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

    private sealed class FakeRegionDataService : IRegionDataService
    {
        private readonly List<RegionEntry> _entries;

        public FakeRegionDataService(params RegionEntry[] entries)
        {
            _entries = entries.ToList();
        }

        public bool IsLazy => true;

        public bool IsLoaded => true;

        public void EnsureLoaded()
        {
        }

        public void Reload()
        {
        }

        public IReadOnlyList<RegionEntry> GetAllEntries()
            => _entries;

        public void SetEntries(IReadOnlyList<RegionEntry> entries)
            => throw new NotSupportedException();
    }
}

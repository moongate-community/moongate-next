using Moongate.UO.Data.Files;
using Moongate.UO.Data.Maps;
using Moongate.UO.Data.Types.Maps;

namespace Moongate.Tests.UO.Data.Maps;

public class MapServiceTests
{
    [Fact]
    public void GetMap_StandardFacet_ReturnsExpectedDimensions()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            var service = new MapService(new UoFileResolver(dir.FullName));

            var felucca = service.GetMap(0);

            Assert.NotNull(felucca);
            Assert.Equal("Felucca", felucca!.Name);
            Assert.Equal(7168, felucca.Width);
            Assert.Equal(4096, felucca.Height);
            Assert.Equal(7, service.Maps.Count);
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void GetMap_UnknownId_ReturnsNull()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            var service = new MapService(new UoFileResolver(dir.FullName));

            Assert.Null(service.GetMap(999));
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void GetMap_ExposesSeasonRulesAndInternalFacet()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            var service = new MapService(new UoFileResolver(dir.FullName));

            Assert.Equal(SeasonType.Desolation, service.GetMap(0)!.Season);
            Assert.Equal(MapRulesType.FeluccaRules, service.GetMap(0)!.Rules);
            Assert.Equal(SeasonType.Spring, service.GetMap(1)!.Season);
            Assert.Equal(MapRulesType.TrammelRules, service.GetMap(1)!.Rules);
            Assert.Equal(SeasonType.Summer, service.GetMap(2)!.Season);

            var internalMap = service.GetMap(0x7F);
            Assert.NotNull(internalMap);
            Assert.Equal("Internal", internalMap!.Name);
            Assert.Equal(SeasonType.Spring, internalMap.Season);
            Assert.Equal(MapRulesType.Internal, internalMap.Rules);
        }
        finally
        {
            dir.Delete(true);
        }
    }
}

using Moongate.UO.Data.Files;
using Moongate.UO.Data.Maps;

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
            Assert.Equal(6, service.Maps.Count);
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
}

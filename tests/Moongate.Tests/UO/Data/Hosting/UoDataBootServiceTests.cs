using Moongate.Server.Services.UoData;
using Moongate.Tests.UO.Data.Support;
using Moongate.UO.Data.Art;
using Moongate.UO.Data.Bodies;
using Moongate.UO.Data.Expansions;
using Moongate.UO.Data.Files;
using Moongate.UO.Data.Hues;
using Moongate.UO.Data.Localization;
using Moongate.UO.Data.Maps;
using Moongate.UO.Data.Multi;
using Moongate.UO.Data.Races;
using Moongate.UO.Data.Skills;
using Moongate.UO.Data.Textures;
using Moongate.UO.Data.Tiles;

namespace Moongate.Tests.UO.Data.Hosting;

public class UoDataBootServiceTests
{
    [Fact]
    public async Task StartAsync_EagerLoadsAndLogs_WithoutThrowing()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            TileDataFixture.Write(
                dir.FullName,
                land: [new TileDataFixture.LandEntry(0, 0u, "void")],
                items: []
            );
            var resolver = new UoFileResolver(dir.FullName);

            var service = new UoDataBootService(
                new TileDataStore(resolver),
                new MapService(resolver),
                new LocalizationService(resolver),
                new MultiDataStore(resolver),
                new ArtService(resolver),
                new SkillDataStore(dir.FullName),
                new RaceStore(dir.FullName),
                new BodyDataStore(dir.FullName),
                new HueStore(resolver),
                new RadarColorStore(resolver),
                new TextureStore(resolver),
                new ExpansionStore(dir.FullName)
            );

            await service.StartAsync(CancellationToken.None);
            await service.StopAsync(CancellationToken.None);
        }
        finally
        {
            dir.Delete(true);
        }
    }
}

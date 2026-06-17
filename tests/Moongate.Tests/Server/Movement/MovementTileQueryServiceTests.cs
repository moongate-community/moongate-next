using Moongate.Server.Services.Movement;
using Moongate.UO.Data.Files;
using Moongate.UO.Data.Maps;
using Xunit;

namespace Moongate.Tests.Server.Movement;

public sealed class MovementTileQueryServiceTests
{
    [Fact]
    public void TryGetMapBounds_KnownMap_ReturnsDimensions()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            var maps = new MapService(new UoFileResolver(dir.FullName));
            var query = new MovementTileQueryService(maps);

            Assert.True(query.TryGetMapBounds(0, out var w, out var h));
            Assert.Equal(7168, w);
            Assert.Equal(4096, h);
            Assert.False(query.TryGetMapBounds(999, out _, out _));
        }
        finally
        {
            dir.Delete(true);
        }
    }
}

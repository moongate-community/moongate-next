using Moongate.UO.Data.Data.Tiles;

namespace Moongate.Tests.UO.Data.Tiles;

public class TilePrimitivesTests
{
    [Fact]
    public void LandTile_StoresIdAndZ_AndDetectsIgnored()
    {
        var tile = new LandTile(0x0A, 5);

        Assert.Equal(0x0A, tile.ID);
        Assert.Equal(5, tile.Z);
        Assert.Equal(0, tile.Height);
        Assert.False(tile.Ignored);
        Assert.True(new LandTile(2, 0).Ignored);
    }

    [Fact]
    public void StaticTile_StoresAllFields()
    {
        var tile = new StaticTile(0x4000, 3, 2, 10, 0x1F);

        Assert.Equal(0x4000, tile.ID);
        Assert.Equal(3, tile.X);
        Assert.Equal(2, tile.Y);
        Assert.Equal(10, tile.Z);
        Assert.Equal(0x1F, tile.Hue);
    }

    [Fact]
    public void HuedTile_RoundTripsViaSet()
    {
        var tile = new HuedTile(0x12, 0x20, -5);
        tile.Set(0x34, 0x21, 7);

        Assert.Equal(0x34, tile.ID);
        Assert.Equal(0x21, tile.Hue);
        Assert.Equal(7, tile.Z);
    }
}

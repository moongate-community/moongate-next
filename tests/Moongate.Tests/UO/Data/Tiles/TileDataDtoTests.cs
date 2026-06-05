using Moongate.UO.Data.Data.Tiles;
using Moongate.UO.Data.Types.Tiles;

namespace Moongate.Tests.UO.Data.Tiles;

public class TileDataDtoTests
{
    [Fact]
    public void ItemData_CalcHeight_HalvesWhenBridge()
    {
        var bridge = new ItemData("stairs", UoTileFlag.Bridge, 0, 0, 0, 0, 0, 10);
        var flat = new ItemData("rock", UoTileFlag.Surface, 0, 0, 0, 0, 0, 10);

        Assert.Equal(5, bridge.CalcHeight);
        Assert.Equal(10, flat.CalcHeight);
    }

    [Fact]
    public void ItemData_FlagAccessors_ReflectFlags()
    {
        var data = new ItemData("dagger", UoTileFlag.Weapon | UoTileFlag.Wearable, 1, 0, 0, 0, 0, 0);

        Assert.True(data[UoTileFlag.Weapon]);
        Assert.True(data.Wearable);
        Assert.False(data.Door);
        Assert.Equal(1, data.Weight);
        Assert.Equal("dagger", data.Name);
    }

    [Fact]
    public void LandData_StoresNameAndFlags()
    {
        var land = new LandData("grass", UoTileFlag.Impassable);

        Assert.Equal("grass", land.Name);
        Assert.Equal(UoTileFlag.Impassable, land.Flags);
    }
}

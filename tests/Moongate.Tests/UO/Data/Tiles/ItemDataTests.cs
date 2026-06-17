using Moongate.UO.Data.Data.Tiles;
using Moongate.UO.Data.Types.Tiles;
using Xunit;

namespace Moongate.Tests.UO.Data.Tiles;

public sealed class ItemDataTests
{
    [Fact]
    public void ImpassableSurface_TrueWhenEitherFlagSet()
    {
        // ImpassableSurface is the Impassable|Surface bitmask test (true if EITHER bit is set),
        // matching the ModernUO/v2 movement semantic.
        var both = new ItemData { Flags = UoTileFlag.Impassable | UoTileFlag.Surface };
        Assert.True(both.ImpassableSurface);

        var onlyImpassable = new ItemData { Flags = UoTileFlag.Impassable };
        Assert.True(onlyImpassable.ImpassableSurface);

        var onlySurface = new ItemData { Flags = UoTileFlag.Surface };
        Assert.True(onlySurface.ImpassableSurface);
    }

    [Fact]
    public void ImpassableSurface_FalseWhenNeitherFlagSet()
    {
        var neither = new ItemData { Flags = UoTileFlag.Wall };
        Assert.False(neither.ImpassableSurface);
    }
}

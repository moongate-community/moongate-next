using Moongate.UO.Data.Types.Maps;

namespace Moongate.Tests.UO.Data.Maps;

public sealed class MusicTypeTests
{
    [Theory]
    [InlineData("Britain1", MusicType.Britain1)]
    [InlineData("Dungeon9", MusicType.Dungeon9)]
    [InlineData("Vesper", MusicType.Vesper)]
    [InlineData("Mountn_a", MusicType.Mountn_a)]
    [InlineData("dungeon9", MusicType.Dungeon9)]
    [InlineData("Nonsense", MusicType.NoMusic)]
    [InlineData("", MusicType.NoMusic)]
    [InlineData(null, MusicType.NoMusic)]
    public void FromName_MapsKnownNames(string? name, MusicType expected)
    {
        Assert.Equal(expected, MusicTypeParser.FromName(name));
    }

    [Fact]
    public void Enum_HasExpectedAnchorValues()
    {
        Assert.Equal(-1, (int)MusicType.Invalid);
        Assert.Equal(0, (int)MusicType.OldUlt01);
        Assert.Equal(0x1FFF, (int)MusicType.NoMusic);
    }
}

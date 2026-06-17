using Moongate.Server.FileLoaders;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Races;
using Moongate.UO.Data.Races.Base;
using Moongate.UO.Data.Types.Mobiles;

namespace Moongate.Tests.UO.Data.Races;

public sealed class RaceTests
{
    public RaceTests()
    {
        // The race registry is process-global; registration is idempotent.
        RaceLoader.RegisterDefaultRaces();
    }

    [Fact]
    public void Elf_And_Gargoyle_AliveBodies()
    {
        Assert.Equal(605, Race.Elf.AliveBody(false));
        Assert.Equal(606, Race.Elf.AliveBody(true));
        Assert.Equal(666, Race.Gargoyle.AliveBody(false));
        Assert.Equal(667, Race.Gargoyle.AliveBody(true));
    }

    [Fact]
    public void Elf_ClipSkinHue_SnapsToAllowedSet()
    {
        Assert.Equal(0x24D, Race.Elf.ClipSkinHue(0x24D));  // in the table → kept
        Assert.Equal(0x0BF, Race.Elf.ClipSkinHue(0x1234)); // not in table → first entry
    }

    [Fact]
    public void GetRace_And_Parse()
    {
        Assert.Same(Race.Elf, Race.GetRace(1));
        Assert.Null(Race.GetRace(99));
        Assert.Same(Race.Human, Race.Parse("0"));
        Assert.Same(Race.Gargoyle, Race.Parse("gargoyle"));
        Assert.Same(Race.Elf, Race.Parse("Elves"));
        Assert.True(Race.TryParse("Human", null, out var human));
        Assert.Same(Race.Human, human);
        Assert.False(Race.TryParse("orc", null, out _));
    }

    [Theory]
    [InlineData(GenderType.Male, true, 400)]
    [InlineData(GenderType.Female, true, 401)]
    [InlineData(GenderType.Male, false, 402)]
    [InlineData(GenderType.Female, false, 403)]
    public void Human_Body_MatchesGenderAndAliveState(GenderType gender, bool alive, int expected)
    {
        var mobile = new MobileEntity { Gender = gender, IsAlive = alive };

        Assert.Equal(expected, Race.Human.Body(mobile));
    }

    [Fact]
    public void Human_ClipSkinHue_ClampsToRange()
    {
        Assert.Equal(1002, Race.Human.ClipSkinHue(500));
        Assert.Equal(1058, Race.Human.ClipSkinHue(2000));
        Assert.Equal(1030, Race.Human.ClipSkinHue(1030));
    }

    [Fact]
    public void RaceFlags_And_IsAllowedRace()
    {
        Assert.Equal(0x1, Race.Human.RaceFlag);
        Assert.Equal(0x2, Race.Elf.RaceFlag);
        Assert.Equal(0x4, Race.Gargoyle.RaceFlag);
        Assert.True(Race.IsAllowedRace(Race.Elf, Race.AllowElvesOnly));
        Assert.False(Race.IsAllowedRace(Race.Human, Race.AllowElvesOnly));
        Assert.True(Race.IsAllowedRace(Race.Gargoyle, Race.AllowAllRaces));
    }

    [Fact]
    public void RandomSkinHue_StaysWithinExpectedShape()
    {
        // Human skin hue is Random(1002,57) | 0x8000 → high bit set, base in [1002,1058].
        var hue = Race.Human.RandomSkinHue();

        Assert.True((hue & 0x8000) != 0);
        var baseHue = hue & 0x7FFF;
        Assert.InRange(baseHue, 1002, 1058);
    }

    [Fact]
    public void RegisterDefaultRaces_PopulatesRegistry()
    {
        Assert.IsType<Human>(Race.Human);
        Assert.IsType<Elf>(Race.Elf);
        Assert.IsType<Gargoyle>(Race.Gargoyle);
        Assert.Contains(Race.Human, Race.AllRaces);
        Assert.Contains(Race.Elf, Race.AllRaces);
        Assert.Contains(Race.Gargoyle, Race.AllRaces);
    }
}

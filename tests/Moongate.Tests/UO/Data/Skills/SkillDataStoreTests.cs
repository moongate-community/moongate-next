using Moongate.UO.Data.Skills;
using Moongate.UO.Data.Types.Skills;

namespace Moongate.Tests.UO.Data.Skills;

public class SkillDataStoreTests
{
    private const string Yaml =
        """
        skill:
          - id: 0
            name: Alchemy
            title: Alchemist
            str_scale: 0.0
            dex_scale: 0.05
            int_scale: 0.05
            stat_total: 10.0
            str_gain: 0.0
            dex_gain: 0.5
            int_gain: 0.5
            gain_factor: 1.0
            profession_skill_name: Alchemy
            primary_stat: Intelligence
            secondary_stat: Dexterity
          - id: 1
            name: Anatomy
            title: Biologist
            str_scale: 0.0
            dex_scale: 0.0
            int_scale: 0.0
            stat_total: 0.0
            str_gain: 0.15
            dex_gain: 0.15
            int_gain: 0.7
            gain_factor: 1.0
            profession_skill_name: Anatomy
            primary_stat: Intelligence
            secondary_stat: Strength
        """;

    [Fact]
    public void Load_ParsesSkills_AndLooksUp()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "skills.yaml"), Yaml);
            var store = new SkillDataStore(dir.FullName);

            Assert.Equal(2, store.Count);
            Assert.Equal("Alchemy", store.GetById(0)!.Name);
            Assert.Equal(StatType.Intelligence, store.GetById(0)!.PrimaryStat);
            Assert.Equal(0.5, store.GetById(0)!.DexGain);
            Assert.Equal("Biologist", store.GetByName("anatomy")!.Title);
            Assert.Null(store.GetById(999));
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void MissingFile_YieldsEmptyStore()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            var store = new SkillDataStore(dir.FullName);

            Assert.Equal(0, store.Count);
            Assert.Null(store.GetById(0));
        }
        finally
        {
            dir.Delete(true);
        }
    }
}

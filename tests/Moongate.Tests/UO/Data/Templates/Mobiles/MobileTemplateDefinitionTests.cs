using Moongate.Core.Yaml;
using Moongate.UO.Data.Templates.Mobiles;
using Moongate.UO.Data.Types.Mobiles;

namespace Moongate.Tests.UO.Data.Templates.Mobiles;

public sealed class MobileTemplateDefinitionTests
{
    [Fact]
    public void Deserialize_FullSchema_MapsAllFields()
    {
        const string yaml =
            """
            mobile_templates:
              - id: town_guard
                base_mobile: base_humanoid
                name: a guard
                title: the guard
                body: 400
                gender: Male
                race_index: 0
                skin_hue: 1002
                hair_hue: 1109
                hair_style: 8251
                brain: guard_brain
                notoriety: Criminal
                karma: -500
                fame: 1200
                faction_id: town_britannia
                stats: { strength: 100, dexterity: 90, intelligence: 50 }
                resources: { hits: 120, mana: 50, stamina: 90 }
                resistances: { physical: 40, fire: 20 }
                skills:
                  Swordsmanship: 90
                  Tactics: 80
                equipment:
                  - item: leather_chest
                  - item: katana
                backpack_template: backpack
                loot_tables: [common]
                tags: [npc, guard]
                params:
                  faction_rank: { type: Integer, value: "3" }
            """;

        var table = YamlUtils.Deserialize<MobileTemplateTable>(yaml);

        var t = Assert.Single(table.MobileTemplates);
        Assert.Equal("town_guard", t.Id);
        Assert.Equal("base_humanoid", t.BaseMobile);
        Assert.Equal("a guard", t.Name);
        Assert.Equal("the guard", t.Title);
        Assert.Equal(400, t.Body);
        Assert.Equal(GenderType.Male, t.Gender);
        Assert.Equal(1002, t.SkinHue);
        Assert.Equal(8251, t.HairStyle);
        Assert.Equal("guard_brain", t.Brain);
        Assert.Equal(NotorietyType.Criminal, t.Notoriety);
        Assert.Equal(-500, t.Karma);
        Assert.Equal(1200, t.Fame);
        Assert.Equal("town_britannia", t.FactionId);
        Assert.NotNull(t.Stats);
        Assert.Equal(100, t.Stats.Strength);
        Assert.NotNull(t.Resources);
        Assert.Equal(120, t.Resources.Hits);
        Assert.NotNull(t.Resistances);
        Assert.Equal(40, t.Resistances.Physical);
        Assert.Equal(90, t.Skills["Swordsmanship"]);
        Assert.Equal(2, t.Equipment.Count);
        Assert.Equal("katana", t.Equipment[1].Item);
        Assert.Equal("backpack", t.BackpackTemplate);
        Assert.Equal(new[] { "common" }, t.LootTables);
        Assert.Equal(new[] { "npc", "guard" }, t.Tags);
        Assert.Equal("3", t.Params["faction_rank"].Value);
    }

    [Fact]
    public void Deserialize_Minimal_AppliesDefaults()
    {
        const string yaml =
            """
            mobile_templates:
              - id: bare
            """;

        var t = Assert.Single(YamlUtils.Deserialize<MobileTemplateTable>(yaml).MobileTemplates);
        Assert.False(t.IsAbstract);
        Assert.Null(t.BaseMobile);
        Assert.Equal(NotorietyType.Innocent, t.Notoriety);
        Assert.Null(t.Stats);
        Assert.Null(t.Resources);
        Assert.Null(t.Resistances);
        Assert.Empty(t.Skills);
        Assert.Empty(t.Equipment);
        Assert.Empty(t.LootTables);
        Assert.Empty(t.Tags);
        Assert.Empty(t.Params);
        Assert.Null(t.BackpackTemplate);
        Assert.Null(t.Brain);
    }
}

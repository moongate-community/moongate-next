using Moongate.Server.Data.Templates;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Templates.Mobiles;
using Moongate.UO.Data.Types.Items;
using Moongate.UO.Data.Types.Mobiles;

namespace Moongate.Tests.Server.Data;

public sealed class MobileTemplateDtoTests
{
    private static MobileTemplateDefinition Guard()
    {
        var m = new MobileTemplateDefinition
        {
            Id = "town_guard",
            Name = "a guard",
            Title = "the guard",
            Body = 400,
            Gender = GenderType.Male,
            RaceIndex = 0,
            SkinHue = 1002,
            HairHue = 1109,
            HairStyle = 8251,
            Brain = "guard_brain",
            Notoriety = NotorietyType.Criminal,
            Karma = -500,
            Fame = 1200,
            FactionId = "town_britannia",
            BaseMobile = "base_humanoid",
            Stats = new MobileStatsTemplate { Strength = 100, Dexterity = 90, Intelligence = 50 },
            Resources = new MobileResourcesTemplate { Hits = 120, Mana = 50, Stamina = 90 },
            Resistances = new MobileResistancesTemplate { Physical = 40, Fire = 20 },
            BackpackTemplate = "backpack",
            IsAbstract = false
        };
        m.Skills["Swords"] = 90;
        m.Skills["Tactics"] = 80;
        m.Equipment.Add(new MobileEquipmentEntry { Item = "leather_chest" });
        m.Equipment.Add(new MobileEquipmentEntry { Item = "katana" });
        m.LootTables.Add("common");
        m.Tags.Add("npc");
        m.Tags.Add("guard");
        m.Params["faction_rank"] = new ItemTemplateParamDefinition { Type = ItemTemplateParamType.Integer, Value = "3" };

        return m;
    }

    [Fact]
    public void Summary_FromDefinition_MapsFields()
    {
        var s = MobileTemplateSummary.FromDefinition(Guard());

        Assert.Equal("town_guard", s.Id);
        Assert.Equal("a guard", s.Name);
        Assert.Equal("the guard", s.Title);
        Assert.Equal(400, s.Body);
        Assert.Equal("0x0190", s.BodyHex);
        Assert.Equal("/api/mobiles/400.png", s.ImageUrl);
        Assert.Equal("Male", s.Gender);
        Assert.Equal("Criminal", s.Notoriety);
        Assert.Equal(-500, s.Karma);
        Assert.Equal(1200, s.Fame);
        Assert.Equal("town_britannia", s.FactionId);
        Assert.Equal("guard_brain", s.Brain);
        Assert.False(s.IsAbstract);
        Assert.Equal(new[] { "npc", "guard" }, s.Tags);
        Assert.Equal(2, s.EquipmentCount);
        Assert.Equal(1, s.LootTablesCount);
    }

    [Fact]
    public void Summary_NullStrings_BecomeEmpty()
    {
        var s = MobileTemplateSummary.FromDefinition(new MobileTemplateDefinition { Id = "bare" });

        Assert.Equal("", s.Name);
        Assert.Equal("", s.Title);
        Assert.Equal("", s.Brain);
        Assert.Equal("", s.FactionId);
        Assert.Equal("Innocent", s.Notoriety);
    }

    [Fact]
    public void Detail_FromDefinition_MapsBlocksSkillsEquipmentLootParams()
    {
        var d = MobileTemplateDetail.FromDefinition(Guard());

        Assert.Equal("base_humanoid", d.BaseMobile);
        Assert.Equal(0, d.RaceIndex);
        Assert.Equal(1002, d.SkinHue);
        Assert.Equal(8251, d.HairStyle);
        Assert.NotNull(d.Stats);
        Assert.Equal(100, d.Stats!.Strength);
        Assert.NotNull(d.Resources);
        Assert.Equal(120, d.Resources!.Hits);
        Assert.NotNull(d.Resistances);
        Assert.Equal(40, d.Resistances!.Physical);
        Assert.Equal(2, d.Skills.Count);
        Assert.Equal("Swords", d.Skills[0].Name); // ordered by name -> Swords before Tactics
        Assert.Equal(90, d.Skills[0].Value);
        Assert.Equal(new[] { "leather_chest", "katana" }, d.Equipment);
        Assert.Equal("backpack", d.BackpackTemplate);
        Assert.Equal(new[] { "common" }, d.LootTables);
        Assert.Single(d.Params);
        Assert.Equal("faction_rank", d.Params[0].Key);
        Assert.Equal("Integer", d.Params[0].Type);
        Assert.Equal("3", d.Params[0].Value);
    }

    [Fact]
    public void Detail_NullBlocks_AreNull()
    {
        var d = MobileTemplateDetail.FromDefinition(new MobileTemplateDefinition { Id = "bare" });

        Assert.Null(d.Stats);
        Assert.Null(d.Resources);
        Assert.Null(d.Resistances);
        Assert.Empty(d.Skills);
        Assert.Empty(d.Equipment);
        Assert.Empty(d.LootTables);
        Assert.Empty(d.Params);
    }
}

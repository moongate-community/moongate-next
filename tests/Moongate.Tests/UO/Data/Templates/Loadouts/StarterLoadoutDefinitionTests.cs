using Moongate.Core.Yaml;
using Moongate.UO.Data.Templates.Loadouts;
using Moongate.UO.Data.Types.Loadouts;

namespace Moongate.Tests.UO.Data.Templates.Loadouts;

public sealed class StarterLoadoutDefinitionTests
{
    [Fact]
    public void Deserialize_FullSchema_MapsAllFields()
    {
        const string yaml =
            """
            starter_loadout:
                backpack_template: backpack
                base:
                    backpack_items:
                        - template: gold_coin
                          amount: 1000
                        - template: dagger
                races:
                    human:
                        equip_items:
                            - template: plain_shirt
                              packet_hue: Shirt
                            - template: plain_pants
                              packet_hue: Pants
                professions:
                    warrior:
                        backpack_items:
                            - template: broadsword
            """;

        var table = YamlUtils.Deserialize<StarterLoadoutTable>(yaml);

        var definition = table.StarterLoadout;
        Assert.NotNull(definition);
        Assert.Equal("backpack", definition.BackpackTemplate);

        Assert.Equal(2, definition.Base.BackpackItems.Count);
        Assert.Equal("gold_coin", definition.Base.BackpackItems[0].Template);
        Assert.Equal(1000, definition.Base.BackpackItems[0].Amount);
        Assert.Null(definition.Base.BackpackItems[1].Amount);

        var human = definition.Races["human"];
        Assert.Equal(2, human.EquipItems.Count);
        Assert.Equal(PacketHueSource.Shirt, human.EquipItems[0].PacketHue);
        Assert.Equal(PacketHueSource.Pants, human.EquipItems[1].PacketHue);

        Assert.Equal("broadsword", definition.Professions["warrior"].BackpackItems[0].Template);
    }

    [Fact]
    public void Deserialize_MinimalSchema_AppliesDefaults()
    {
        const string yaml =
            """
            starter_loadout:
                backpack_template: backpack
            """;

        var table = YamlUtils.Deserialize<StarterLoadoutTable>(yaml);

        var definition = table.StarterLoadout;
        Assert.NotNull(definition);
        Assert.Empty(definition.Base.BackpackItems);
        Assert.Empty(definition.Base.EquipItems);
        Assert.Empty(definition.Races);
        Assert.Empty(definition.Professions);
    }

    [Fact]
    public void Deserialize_EntryDefaults_AmountNullAndPacketHueNone()
    {
        const string yaml =
            """
            starter_loadout:
                base:
                    equip_items:
                        - template: leather_shoes
            """;

        var table = YamlUtils.Deserialize<StarterLoadoutTable>(yaml);

        var entry = Assert.Single(table.StarterLoadout!.Base.EquipItems);
        Assert.Equal("leather_shoes", entry.Template);
        Assert.Null(entry.Amount);
        Assert.Equal(PacketHueSource.None, entry.PacketHue);
    }
}

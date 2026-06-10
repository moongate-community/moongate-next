using Moongate.Core.Yaml;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Types.Items;

namespace Moongate.Tests.UO.Data.Templates;

public sealed class ItemTemplateDefinitionTests
{
    [Fact]
    public void Deserialize_FullTemplate_MapsAllFields()
    {
        const string yaml =
            """
            item_templates:
                - id: plain_shirt
                  base_item: base_clothing
                  name: Shirt
                  comment: Starter clothing template
                  item_id: 5399
                  hue: 33
                  weight: 1
                  is_stackable: false
                  is_movable: true
                  layer: Shirt
                  script_id: shirt_script
                  rarity: Common
                  tags: [clothing, starter]
                  params:
                      dyeable: { type: String, value: "true" }
            """;

        var table = YamlUtils.Deserialize<ItemTemplateTable>(yaml);

        var template = Assert.Single(table.ItemTemplates);
        Assert.Equal("plain_shirt", template.Id);
        Assert.Equal("base_clothing", template.BaseItem);
        Assert.Equal("Shirt", template.Name);
        Assert.Equal("Starter clothing template", template.Comment);
        Assert.Equal(5399, template.ItemId);
        Assert.Equal(33, template.Hue);
        Assert.Equal(1, template.Weight);
        Assert.False(template.IsStackable);
        Assert.True(template.IsMovable);
        Assert.Equal(ItemLayerType.Shirt, template.Layer);
        Assert.Equal("shirt_script", template.ScriptId);
        Assert.Equal(ItemRarity.Common, template.Rarity);
        Assert.Equal(new[] { "clothing", "starter" }, template.Tags);
        Assert.Equal(ItemTemplateParamType.String, template.Params["dyeable"].Type);
        Assert.Equal("true", template.Params["dyeable"].Value);
    }

    [Fact]
    public void Deserialize_MinimalTemplate_AppliesDefaults()
    {
        const string yaml =
            """
            item_templates:
                - id: bare
            """;

        var table = YamlUtils.Deserialize<ItemTemplateTable>(yaml);

        var template = Assert.Single(table.ItemTemplates);
        Assert.False(template.IsAbstract);
        Assert.Null(template.BaseItem);
        Assert.Equal("", template.Comment);
        Assert.Equal(1, template.Amount);
        Assert.Equal(0, template.ItemId);
        Assert.Null(template.Layer);
        Assert.Null(template.GumpId);
        Assert.Equal(ItemRarity.None, template.Rarity);
        Assert.Empty(template.Tags);
        Assert.Empty(template.Params);
    }

    [Fact]
    public void Deserialize_AbstractTemplate_SetsIsAbstract()
    {
        const string yaml =
            """
            item_templates:
                - id: base_clothing
                  is_abstract: true
            """;

        var table = YamlUtils.Deserialize<ItemTemplateTable>(yaml);

        Assert.True(Assert.Single(table.ItemTemplates).IsAbstract);
    }
}

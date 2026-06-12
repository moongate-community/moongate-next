using Moongate.Server.Services.Templates;
using Moongate.Tests.Support;
using Moongate.UO.Data.Types.Items;

namespace Moongate.Tests.Server.Templates;

public class ItemTemplateYamlLoaderTests
{
    [Fact]
    public void LoadAll_BaseItem_InheritsGraphicVariants()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "variants.yaml",
            """
            item_templates:
                - id: base_food
                  is_abstract: true
                  item_id: 4155
                  graphic_variants:
                      - item_id: 4156
                - id: bread_loaf
                  base_item: base_food
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var templates = loader.LoadAll();

        var bread = templates.Single(template => template.Id == "bread_loaf");
        var variant = Assert.Single(bread.GraphicVariants);
        Assert.Equal(4156, variant.ItemId);
    }

    [Fact]
    public void LoadAll_BaseItem_InheritsParentFields()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "clothing.yaml",
            """
            item_templates:
                - id: base_clothing
                  is_abstract: true
                  name: Clothing
                  weight: 2
                  is_movable: true
                  hue: 7
                  comment: Base clothing note
                  script_id: clothing_script
                  rarity: Uncommon
                  value:
                      buy: 20
                      sell: 10
                  tags:
                      - clothing
                - id: plain_shirt
                  base_item: base_clothing
                  item_id: 5399
                  layer: Shirt
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var templates = loader.LoadAll();

        var shirt = templates.Single(template => template.Id == "plain_shirt");
        Assert.Equal("Clothing", shirt.Name);
        Assert.Equal(2, shirt.Weight);
        Assert.True(shirt.IsMovable);
        Assert.Equal(7, shirt.Hue);
        Assert.Equal("Base clothing note", shirt.Comment);
        Assert.Equal("clothing_script", shirt.ScriptId);
        Assert.Equal(ItemRarity.Uncommon, shirt.Rarity);
        Assert.NotNull(shirt.Value);
        Assert.Equal(20, shirt.Value.Buy);
        Assert.Equal(10, shirt.Value.BaseSell);
        Assert.Equal(["clothing"], shirt.Tags);
        Assert.Equal(5399, shirt.ItemId);
        Assert.Equal(ItemLayerType.Shirt, shirt.Layer);
        Assert.False(shirt.IsAbstract);
    }

    [Fact]
    public void LoadAll_BlankContentsLootTemplateClearsContents()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "containers.yaml",
            """
            item_templates:
                - id: wooden_chest
                  item_id: 3651
                  contents:
                      loot_template: ""
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var template = Assert.Single(loader.LoadAll());

        Assert.Null(template.Contents);
    }

    [Fact]
    public void LoadAll_ChildContentsOverridesBaseTemplate()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "containers.yaml",
            """
            item_templates:
                - id: base_container
                  is_abstract: true
                  item_id: 3651
                  contents:
                      loot_template: common
                      refill_every: 6h
                - id: fancy_chest
                  base_item: base_container
                  contents:
                      loot_template: rare
                      refill_every: 12h
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var templates = loader.LoadAll();

        var chest = templates.Single(template => template.Id == "fancy_chest");
        Assert.NotNull(chest.Contents);
        Assert.Equal("rare", chest.Contents.LootTemplate);
        Assert.Equal(TimeSpan.FromHours(12), chest.Contents.RefillEvery);
    }

    [Fact]
    public void LoadAll_ChildValues_WinOverParent()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "clothing.yaml",
            """
            item_templates:
                - id: base_clothing
                  name: Clothing
                  hue: 7
                  weight: 2
                - id: fancy_shirt
                  base_item: base_clothing
                  name: Fancy Shirt
                  hue: 44
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var templates = loader.LoadAll();

        var shirt = templates.Single(template => template.Id == "fancy_shirt");
        Assert.Equal("Fancy Shirt", shirt.Name);
        Assert.Equal(44, shirt.Hue);
        Assert.Equal(2, shirt.Weight);
    }

    [Fact]
    public void LoadAll_CircularBaseItem_Throws()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "cycle.yaml",
            """
            item_templates:
                - id: a
                  base_item: b
                - id: b
                  base_item: a
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var exception = Assert.Throws<InvalidOperationException>(() => loader.LoadAll());

        Assert.Contains("Circular", exception.Message);
    }

    [Fact]
    public void LoadAll_Comment_ChildValueWinsOverParent()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "comments.yaml",
            """
            item_templates:
                - id: parent
                  comment: Parent note
                - id: child_without_comment
                  base_item: parent
                - id: child_with_comment
                  base_item: parent
                  comment: Child note
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var templates = loader.LoadAll();

        var withoutComment = templates.Single(template => template.Id == "child_without_comment");
        var withComment = templates.Single(template => template.Id == "child_with_comment");
        Assert.Equal("Parent note", withoutComment.Comment);
        Assert.Equal("Child note", withComment.Comment);
    }

    [Fact]
    public void LoadAll_DuplicateGraphicVariant_Throws()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "variants.yaml",
            """
            item_templates:
                - id: bread_loaf
                  item_id: 4155
                  graphic_variants:
                      - item_id: 4156
                      - item_id: 4156
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var exception = Assert.Throws<InvalidOperationException>(() => loader.LoadAll());

        Assert.Contains("duplicate graphic variant", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LoadAll_DuplicateId_Throws()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "one.yaml",
            """
            item_templates:
                - id: shirt
            """
        );
        dir.WriteFile(
            "two.yaml",
            """
            item_templates:
                - id: SHIRT
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var exception = Assert.Throws<InvalidOperationException>(() => loader.LoadAll());

        Assert.Contains("SHIRT", exception.Message);
    }

    [Fact]
    public void LoadAll_EmptyDirectory_ReturnsEmptyList()
    {
        using var dir = new TempTemplateDirectory();
        var loader = new ItemTemplateYamlLoader(dir.Path);

        Assert.Empty(loader.LoadAll());
    }

    [Fact]
    public void LoadAll_EmptyId_Throws()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "bad.yaml",
            """
            item_templates:
                - id: ""
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        Assert.Throws<InvalidOperationException>(() => loader.LoadAll());
    }

    [Fact]
    public void LoadAll_HexParamValue_IsAccepted()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "params.yaml",
            """
            item_templates:
                - id: shirt
                  params:
                      special_hue:
                          type: Hue
                          value: "0x21"
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var templates = loader.LoadAll();

        var shirt = Assert.Single(templates);
        Assert.Equal("0x21", shirt.Params["special_hue"].Value);
    }

    [Fact]
    public void LoadAll_InheritedName_WinsOverTileDataName()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "inheritance.yaml",
            """
            item_templates:
                - id: parent
                  name: Parent Name
                - id: child
                  base_item: parent
                  item_id: 100
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path, new TestTileDataStore((100, "Tile Name")));

        var templates = loader.LoadAll();

        var child = templates.Single(template => template.Id == "child");
        Assert.Equal("Parent Name", child.Name);
    }

    [Fact]
    public void LoadAll_InheritsContentsFromBaseTemplate()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "containers.yaml",
            """
            item_templates:
                - id: base_container
                  is_abstract: true
                  item_id: 3651
                  contents:
                      loot_template: common
                      refill_every: 6h
                - id: wooden_chest
                  base_item: base_container
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var templates = loader.LoadAll();

        var parent = templates.Single(template => template.Id == "base_container");
        var child = templates.Single(template => template.Id == "wooden_chest");
        Assert.NotNull(parent.Contents);
        Assert.NotNull(child.Contents);
        Assert.NotSame(parent.Contents, child.Contents);
        Assert.Equal("common", child.Contents.LootTemplate);
        Assert.Equal(TimeSpan.FromHours(6), child.Contents.RefillEvery);
    }

    [Fact]
    public void LoadAll_InvalidGraphicVariant_Throws()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "variants.yaml",
            """
            item_templates:
                - id: bread_loaf
                  item_id: 4155
                  graphic_variants:
                      - item_id: 0
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var exception = Assert.Throws<InvalidOperationException>(() => loader.LoadAll());

        Assert.Contains("invalid graphic variant", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LoadAll_InvalidIntegerParam_Throws()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "params.yaml",
            """
            item_templates:
                - id: wand
                  params:
                      charges:
                          type: Integer
                          value: not_a_number
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var exception = Assert.Throws<InvalidOperationException>(() => loader.LoadAll());

        Assert.Contains("charges", exception.Message);
    }

    [Fact]
    public void LoadAll_MalformedYaml_Throws()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile("broken.yaml", "item_templates:\n  - id: [unclosed");
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var exception = Assert.Throws<InvalidOperationException>(() => loader.LoadAll());

        Assert.Contains("broken.yaml", exception.Message);
    }

    [Fact]
    public void LoadAll_MapsContents()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "containers.yaml",
            """
            item_templates:
                - id: wooden_chest
                  item_id: 3651
                  contents:
                      loot_template: common
                      generate: on_open
                      refill_every: 6h
                      refill_policy: when_empty
                      refill_scope: world_only
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var template = Assert.Single(loader.LoadAll());

        Assert.NotNull(template.Contents);
        Assert.Equal("common", template.Contents.LootTemplate);
        Assert.Equal(ItemTemplateContentGenerateType.OnOpen, template.Contents.Generate);
        Assert.Equal(TimeSpan.FromHours(6), template.Contents.RefillEvery);
        Assert.Equal(ItemTemplateContentRefillPolicy.WhenEmpty, template.Contents.RefillPolicy);
        Assert.Equal(ItemTemplateContentRefillScope.WorldOnly, template.Contents.RefillScope);
    }

    [Fact]
    public void LoadAll_MissingDirectory_ReturnsEmptyList()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), "moongate-template-tests", Guid.NewGuid().ToString("N"));
        var loader = new ItemTemplateYamlLoader(missingPath);

        Assert.Empty(loader.LoadAll());
    }

    [Fact]
    public void LoadAll_MissingNullOrEmptyName_UsesTileDataName()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "tile_names.yaml",
            """
            item_templates:
                - id: missing_name
                  item_id: 100
                - id: null_name
                  name:
                  item_id: 101
                - id: empty_name
                  name: ""
                  item_id: 102
            """
        );
        var tileData = new TestTileDataStore(
            (100, "Missing Name Tile"),
            (101, "Null Name Tile"),
            (102, "Empty Name Tile")
        );
        var loader = new ItemTemplateYamlLoader(dir.Path, tileData);

        var templates = loader.LoadAll();

        Assert.Equal("Missing Name Tile", templates.Single(template => template.Id == "missing_name").Name);
        Assert.Equal("Null Name Tile", templates.Single(template => template.Id == "null_name").Name);
        Assert.Equal("Empty Name Tile", templates.Single(template => template.Id == "empty_name").Name);
    }

    [Fact]
    public void LoadAll_NullGraphicVariantsKey_LoadsWithEmptyVariants()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "null_variants.yaml",
            """
            item_templates:
                - id: shirt
                  graphic_variants:
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var shirt = Assert.Single(loader.LoadAll());

        Assert.Empty(shirt.GraphicVariants);
    }

    [Fact]
    public void LoadAll_NullParamsKey_LoadsWithEmptyParams()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "null_params.yaml",
            """
            item_templates:
                - id: shirt
                  params:
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var shirt = Assert.Single(loader.LoadAll());

        Assert.Empty(shirt.Params);
    }

    [Fact]
    public void LoadAll_NullTagsKey_LoadsWithEmptyTags()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "null_tags.yaml",
            """
            item_templates:
                - id: parent_t
                  is_abstract: true
                  tags: [a]
                - id: child_t
                  base_item: parent_t
                  tags:
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var templates = loader.LoadAll();

        var child = templates.Single(t => t.Id == "child_t");
        Assert.Equal(new[] { "a" }, child.Tags);
    }

    [Fact]
    public void LoadAll_NullTemplateList_ReturnsEmptyList()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile("empty_list.yaml", "item_templates:");
        var loader = new ItemTemplateYamlLoader(dir.Path);

        Assert.Empty(loader.LoadAll());
    }

    [Fact]
    public void LoadAll_ParamKeys_AreCaseInsensitiveAfterLoad()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "params.yaml",
            """
            item_templates:
                - id: shirt
                  params:
                      dyeable:
                          type: String
                          value: "true"
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var templates = loader.LoadAll();

        var shirt = Assert.Single(templates);
        Assert.True(shirt.Params.ContainsKey("DYEABLE"));
    }

    [Fact]
    public void LoadAll_Params_MergeChildOverridesByKey()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "params.yaml",
            """
            item_templates:
                - id: parent
                  params:
                      shared:
                          type: String
                          value: from_parent
                      only_parent:
                          type: String
                          value: keep
                - id: child
                  base_item: parent
                  params:
                      shared:
                          type: String
                          value: from_child
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var templates = loader.LoadAll();

        var child = templates.Single(template => template.Id == "child");
        Assert.Equal("from_child", child.Params["shared"].Value);
        Assert.Equal("keep", child.Params["only_parent"].Value);
    }

    [Fact]
    public void LoadAll_PrimaryGraphicVariant_Throws()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "variants.yaml",
            """
            item_templates:
                - id: bread_loaf
                  item_id: 4155
                  graphic_variants:
                      - item_id: 4155
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var exception = Assert.Throws<InvalidOperationException>(() => loader.LoadAll());

        Assert.Contains("matches primary item_id", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LoadAll_ReservedIsMovableParamKey_Throws()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "reserved.yaml",
            """
            item_templates:
                - id: shirt
                  params:
                      is_movable: { type: String, value: "true" }
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var exception = Assert.Throws<InvalidOperationException>(() => loader.LoadAll());
        Assert.Contains("is_movable", exception.Message);
    }

    [Theory, InlineData("item_template_id"), InlineData("contents.generated_at"), InlineData("contents.next_refill_at")]
    public void LoadAll_ReservedRuntimeItemParamKeys_Throws(string key)
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "reserved.yaml",
            $$"""
              item_templates:
                  - id: shirt
                    params:
                        {{key}}: { type: String, value: "reserved" }
              """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var exception = Assert.Throws<InvalidOperationException>(() => loader.LoadAll());

        Assert.Contains(key, exception.Message);
    }

    [Fact]
    public void LoadAll_SingleFile_LoadsTemplates()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "clothing.yaml",
            """
            item_templates:
                - id: plain_shirt
                  name: Shirt
                  item_id: 5399
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var templates = loader.LoadAll();

        var template = Assert.Single(templates);
        Assert.Equal("plain_shirt", template.Id);
        Assert.Equal(5399, template.ItemId);
    }

    [Fact]
    public void LoadAll_Subdirectories_AreScannedRecursively()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            Path.Combine("base", "bases.yaml"),
            """
            item_templates:
                - id: nested_template
                  name: Nested
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var templates = loader.LoadAll();

        var template = Assert.Single(templates);
        Assert.Equal("nested_template", template.Id);
    }

    [Fact]
    public void LoadAll_Tags_InheritedOnlyWhenChildHasNone()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "tags.yaml",
            """
            item_templates:
                - id: parent
                  tags:
                      - a
                      - b
                - id: child_with_tags
                  base_item: parent
                  tags:
                      - own
                - id: child_without_tags
                  base_item: parent
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var templates = loader.LoadAll();

        var withTags = templates.Single(template => template.Id == "child_with_tags");
        var withoutTags = templates.Single(template => template.Id == "child_without_tags");
        Assert.Equal(["own"], withTags.Tags);
        Assert.Equal(["a", "b"], withoutTags.Tags);
    }

    [Fact]
    public void LoadAll_TwoLevelChain_ResolvesTransitively()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "chain.yaml",
            """
            item_templates:
                - id: level0
                  weight: 9
                - id: level1
                  base_item: level0
                  name: MidName
                - id: level2
                  base_item: level1
                  item_id: 100
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var templates = loader.LoadAll();

        var leaf = templates.Single(template => template.Id == "level2");
        Assert.Equal(9, leaf.Weight);
        Assert.Equal("MidName", leaf.Name);
        Assert.Equal(100, leaf.ItemId);
    }

    [Fact]
    public void LoadAll_UnknownBaseItem_Throws()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "bad.yaml",
            """
            item_templates:
                - id: orphan
                  base_item: does_not_exist
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var exception = Assert.Throws<InvalidOperationException>(() => loader.LoadAll());

        Assert.Contains("does_not_exist", exception.Message);
    }

    [Fact]
    public void LoadAll_Value_ChildValueWinsOverParent()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "values.yaml",
            """
            item_templates:
                - id: parent
                  value:
                      buy: 20
                      sell: 10
                - id: child_without_value
                  base_item: parent
                - id: child_with_value
                  base_item: parent
                  value:
                      buy: 40
                      sell: 18
            """
        );
        var loader = new ItemTemplateYamlLoader(dir.Path);

        var templates = loader.LoadAll();

        var inherited = templates.Single(template => template.Id == "child_without_value");
        var own = templates.Single(template => template.Id == "child_with_value");
        Assert.NotNull(inherited.Value);
        Assert.Equal(20, inherited.Value.Buy);
        Assert.Equal(10, inherited.Value.BaseSell);
        Assert.NotNull(own.Value);
        Assert.Equal(40, own.Value.Buy);
        Assert.Equal(18, own.Value.BaseSell);
    }

    [Fact]
    public void ParseLong_DecimalAndHex_Parse()
    {
        Assert.Equal(33, ItemTemplateYamlLoader.ParseLong("33"));
        Assert.Equal(0x21, ItemTemplateYamlLoader.ParseLong("0x21"));
        Assert.Equal(0x21, ItemTemplateYamlLoader.ParseLong("0X21"));
    }
}

using Moongate.Server.Services.Loadouts;
using Moongate.Tests.Support;

namespace Moongate.Tests.Server.Loadouts;

public sealed class StarterLoadoutYamlLoaderTests
{
    [Fact]
    public void Load_CaseDuplicateSectionKeys_Throws()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            StarterLoadoutYamlLoader.StarterLoadoutFileName,
            """
            starter_loadout:
                races:
                    human:
                        equip_items:
                            - template: plain_shirt
                    Human:
                        equip_items:
                            - template: plain_pants
            """
        );
        var loader = new StarterLoadoutYamlLoader(dir.Path);

        var exception = Assert.Throws<InvalidOperationException>(() => loader.Load());
        Assert.Contains("Human", exception.Message);
    }

    [Fact]
    public void Load_EmptyRootKey_ReturnsNull()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(StarterLoadoutYamlLoader.StarterLoadoutFileName, "starter_loadout:");
        var loader = new StarterLoadoutYamlLoader(dir.Path);

        Assert.Null(loader.Load());
    }

    [Fact]
    public void Load_MalformedYaml_ThrowsWithFilePath()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(StarterLoadoutYamlLoader.StarterLoadoutFileName, "starter_loadout:\n  base: [unclosed");
        var loader = new StarterLoadoutYamlLoader(dir.Path);

        var exception = Assert.Throws<InvalidOperationException>(() => loader.Load());
        Assert.Contains(StarterLoadoutYamlLoader.StarterLoadoutFileName, exception.Message);
    }

    [Fact]
    public void Load_MissingDirectory_ReturnsNull()
    {
        var loader = new StarterLoadoutYamlLoader(
            Path.Combine(Path.GetTempPath(), "moongate-loadout-tests", Guid.NewGuid().ToString("N"))
        );

        Assert.Null(loader.Load());
    }

    [Fact]
    public void Load_MissingFile_ReturnsNull()
    {
        using var dir = new TempTemplateDirectory();
        var loader = new StarterLoadoutYamlLoader(dir.Path);

        Assert.Null(loader.Load());
    }

    [Fact]
    public void Load_NullCollections_AreNormalizedToEmpty()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            StarterLoadoutYamlLoader.StarterLoadoutFileName,
            """
            starter_loadout:
                backpack_template: backpack
                base:
                    backpack_items:
                    equip_items:
                races:
                professions:
            """
        );
        var loader = new StarterLoadoutYamlLoader(dir.Path);

        var definition = loader.Load();

        Assert.NotNull(definition);
        Assert.Empty(definition.Base.BackpackItems);
        Assert.Empty(definition.Base.EquipItems);
        Assert.Empty(definition.Races);
        Assert.Empty(definition.Professions);
    }

    [Fact]
    public void Load_NullListEntry_ThrowsWithSectionContext()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            StarterLoadoutYamlLoader.StarterLoadoutFileName,
            """
            starter_loadout:
                base:
                    backpack_items:
                        - template: gold_coin
                        -
            """
        );
        var loader = new StarterLoadoutYamlLoader(dir.Path);

        var exception = Assert.Throws<InvalidOperationException>(() => loader.Load());
        Assert.Contains("base", exception.Message);
        Assert.Contains("empty list entry", exception.Message);
    }

    [Fact]
    public void Load_SectionKeys_AreCaseInsensitiveAfterLoad()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            StarterLoadoutYamlLoader.StarterLoadoutFileName,
            """
            starter_loadout:
                races:
                    Human:
                        equip_items:
                            - template: plain_shirt
            """
        );
        var loader = new StarterLoadoutYamlLoader(dir.Path);

        var definition = loader.Load();

        Assert.NotNull(definition);
        Assert.True(definition.Races.ContainsKey("human"));
    }

    [Fact]
    public void Load_ValidFile_ReturnsDefinition()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            StarterLoadoutYamlLoader.StarterLoadoutFileName,
            """
            starter_loadout:
                backpack_template: backpack
                base:
                    backpack_items:
                        - template: gold_coin
                          amount: 1000
            """
        );
        var loader = new StarterLoadoutYamlLoader(dir.Path);

        var definition = loader.Load();

        Assert.NotNull(definition);
        Assert.Equal("backpack", definition.BackpackTemplate);
        Assert.Single(definition.Base.BackpackItems);
    }
}

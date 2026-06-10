using Moongate.Server.Services.Mobiles;
using Moongate.Tests.Support;

namespace Moongate.Tests.Server.Mobiles;

public sealed class MobileTemplateYamlLoaderTests
{
    [Fact]
    public void LoadAll_MissingDirectory_ReturnsEmpty()
    {
        var loader = new MobileTemplateYamlLoader(
            Path.Combine(Path.GetTempPath(), "moongate-mobile-tests", Guid.NewGuid().ToString("N"))
        );

        Assert.Empty(loader.LoadAll());
    }

    [Fact]
    public void LoadAll_SingleFile_Loads()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile("a.yaml", "mobile_templates:\n  - id: guard\n    body: 400\n");
        var loader = new MobileTemplateYamlLoader(dir.Path);

        var t = Assert.Single(loader.LoadAll());
        Assert.Equal("guard", t.Id);
        Assert.Equal(400, t.Body);
    }

    [Fact]
    public void LoadAll_BaseMobile_InheritsScalarsBlocksAndLists()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "a.yaml",
            """
            mobile_templates:
              - id: base_humanoid
                is_abstract: true
                race_index: 2
                stats: { strength: 50, dexterity: 50, intelligence: 30 }
                tags: [humanoid]
                skills:
                  Tactics: 40
              - id: guard
                base_mobile: base_humanoid
                name: a guard
                body: 400
                skills:
                  Swordsmanship: 90
            """
        );
        var loader = new MobileTemplateYamlLoader(dir.Path);

        var guard = loader.LoadAll().Single(t => t.Id == "guard");
        Assert.Equal(2, guard.RaceIndex);
        Assert.NotNull(guard.Stats);
        Assert.Equal(50, guard.Stats.Strength);
        Assert.Equal(new[] { "humanoid" }, guard.Tags);
        Assert.Equal(90, guard.Skills["Swordsmanship"]);
        Assert.Equal(40, guard.Skills["Tactics"]);
        Assert.False(guard.IsAbstract);
        Assert.Equal("a guard", guard.Name);
    }

    [Fact]
    public void LoadAll_ChildBlock_WinsWhole()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "a.yaml",
            """
            mobile_templates:
              - id: base_h
                is_abstract: true
                stats: { strength: 50, dexterity: 50, intelligence: 30 }
              - id: brute
                base_mobile: base_h
                stats: { strength: 100, dexterity: 40, intelligence: 10 }
            """
        );
        var loader = new MobileTemplateYamlLoader(dir.Path);

        var brute = loader.LoadAll().Single(t => t.Id == "brute");
        Assert.Equal(100, brute.Stats!.Strength);
        Assert.Equal(40, brute.Stats.Dexterity);
    }

    [Fact]
    public void LoadAll_DuplicateId_Throws()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile("one.yaml", "mobile_templates:\n  - id: guard\n");
        dir.WriteFile("two.yaml", "mobile_templates:\n  - id: GUARD\n");
        var loader = new MobileTemplateYamlLoader(dir.Path);

        var exception = Assert.Throws<InvalidOperationException>(() => loader.LoadAll());
        Assert.Contains("GUARD", exception.Message);
    }

    [Fact]
    public void LoadAll_UnknownBaseMobile_Throws()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile("a.yaml", "mobile_templates:\n  - id: orphan\n    base_mobile: nope\n");
        var loader = new MobileTemplateYamlLoader(dir.Path);

        var exception = Assert.Throws<InvalidOperationException>(() => loader.LoadAll());
        Assert.Contains("nope", exception.Message);
    }

    [Fact]
    public void LoadAll_CircularBaseMobile_Throws()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile("a.yaml", "mobile_templates:\n  - id: a\n    base_mobile: b\n  - id: b\n    base_mobile: a\n");
        var loader = new MobileTemplateYamlLoader(dir.Path);

        var exception = Assert.Throws<InvalidOperationException>(() => loader.LoadAll());
        Assert.Contains("Circular", exception.Message);
    }

    [Fact]
    public void LoadAll_EmptyId_Throws()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile("a.yaml", "mobile_templates:\n  - id: \"\"\n    name: x\n");
        var loader = new MobileTemplateYamlLoader(dir.Path);

        Assert.Throws<InvalidOperationException>(() => loader.LoadAll());
    }

    [Fact]
    public void LoadAll_MalformedYaml_ThrowsWithFilePath()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile("broken.yaml", "mobile_templates:\n  - id: [unclosed");
        var loader = new MobileTemplateYamlLoader(dir.Path);

        var exception = Assert.Throws<InvalidOperationException>(() => loader.LoadAll());
        Assert.Contains("broken.yaml", exception.Message);
    }

    [Fact]
    public void LoadAll_NullCollections_Normalized()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile("a.yaml", "mobile_templates:\n  - id: t\n    equipment:\n    loot_tables:\n    tags:\n    skills:\n    params:\n");
        var loader = new MobileTemplateYamlLoader(dir.Path);

        var t = Assert.Single(loader.LoadAll());
        Assert.Empty(t.Equipment);
        Assert.Empty(t.LootTables);
        Assert.Empty(t.Tags);
        Assert.Empty(t.Skills);
        Assert.Empty(t.Params);
    }
}

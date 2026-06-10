using Moongate.Server.Services.Loot;
using Moongate.Tests.Support;

namespace Moongate.Tests.Server.Loot;

public sealed class LootTableYamlLoaderTests
{
    [Fact]
    public void LoadAll_EmptyDirectory_ReturnsEmpty()
    {
        using var dir = new TempTemplateDirectory();
        var loader = new LootTableYamlLoader(dir.Path);

        Assert.Empty(loader.LoadAll());
    }

    [Fact]
    public void LoadAll_MalformedYaml_ThrowsWithFilePath()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile("broken.yaml", "loot_tables:\n  - id: [unclosed");
        var loader = new LootTableYamlLoader(dir.Path);

        var exception = Assert.Throws<InvalidOperationException>(() => loader.LoadAll());
        Assert.Contains("broken.yaml", exception.Message);
    }

    [Fact]
    public void LoadAll_MissingDirectory_ReturnsEmpty()
    {
        var loader = new LootTableYamlLoader(
            Path.Combine(Path.GetTempPath(), "moongate-loot-tests", Guid.NewGuid().ToString("N"))
        );

        Assert.Empty(loader.LoadAll());
    }

    [Fact]
    public void LoadAll_MultipleFiles_MergedAndOrdered()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile("a.yaml", "loot_tables:\n  - id: alpha\n    content:\n      - item: gold_coin\n");
        dir.WriteFile("b.yaml", "loot_tables:\n  - id: beta\n    content:\n      - item: apple\n");
        var loader = new LootTableYamlLoader(dir.Path);

        var tables = loader.LoadAll();

        Assert.Equal(2, tables.Count);
        Assert.Contains(tables, t => t.Id == "alpha");
        Assert.Contains(tables, t => t.Id == "beta");
    }

    [Fact]
    public void LoadAll_NestedNullChild_ThrowsWithContext()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "a.yaml",
            "loot_tables:\n  - id: t\n    content:\n      - pick_one_of:\n          - item: apple\n          -\n"
        );
        var loader = new LootTableYamlLoader(dir.Path);

        Assert.Throws<InvalidOperationException>(() => loader.LoadAll());
    }

    [Fact]
    public void LoadAll_NullContent_NormalizedToEmpty()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile("a.yaml", "loot_tables:\n  - id: empty\n    content:\n");
        var loader = new LootTableYamlLoader(dir.Path);

        var table = Assert.Single(loader.LoadAll());
        Assert.Empty(table.Content);
    }

    [Fact]
    public void LoadAll_NullNodeEntry_ThrowsWithContext()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile("a.yaml", "loot_tables:\n  - id: t\n    content:\n      - item: gold_coin\n      -\n");
        var loader = new LootTableYamlLoader(dir.Path);

        var exception = Assert.Throws<InvalidOperationException>(() => loader.LoadAll());
        Assert.Contains("t", exception.Message);
        Assert.Contains("empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}

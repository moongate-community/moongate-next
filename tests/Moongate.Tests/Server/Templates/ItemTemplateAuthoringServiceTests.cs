using Moongate.Core.Yaml;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.Data.Templates;
using Moongate.Server.Services.Templates;
using Moongate.Tests.Support;
using Moongate.UO.Data.Data.Hues;
using Moongate.UO.Data.Interfaces.Hues;
using Moongate.UO.Data.Templates.Items;

namespace Moongate.Tests.Server.Templates;

public sealed class ItemTemplateAuthoringServiceTests
{
    [Fact]
    public void FindSource_ReturnsOwningYamlFile()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "containers.yaml",
            """
            item_templates:
                - id: wooden_box
                  item_id: 3651
            """
        );
        var store = new ItemTemplateYamlDocumentStore(dir.Path);

        var source = store.ResolveSourceFile("wooden_box");

        Assert.Equal(System.IO.Path.Combine(dir.Path, "containers.yaml"), source);
    }

    [Fact]
    public void LoadOrCreateManagedFile_CreatesWebYamlWhenMissing()
    {
        using var dir = new TempTemplateDirectory();
        var store = new ItemTemplateYamlDocumentStore(dir.Path);
        var managedPath = System.IO.Path.Combine(dir.Path, ItemTemplateYamlDocumentStore.ManagedFileName);

        var table = store.LoadTable(managedPath);

        Assert.Empty(table.ItemTemplates);
        Assert.False(File.Exists(managedPath));
    }

    [Fact]
    public void Upsert_ReplacesExistingTemplateInOwningFile()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "weapons.yaml",
            """
            item_templates:
                - id: longsword
                  name: Old Sword
                  item_id: 3937
            """
        );
        var store = new ItemTemplateYamlDocumentStore(dir.Path);
        var source = store.ResolveSourceFile("longsword");

        store.Upsert(
            source,
            new ItemTemplateDefinition
            {
                Id = "longsword",
                Name = "Longsword",
                ItemId = 0x0F61
            }
        );

        var table = YamlUtils.DeserializeFromFile<ItemTemplateTable>(source);
        var template = Assert.Single(table.ItemTemplates);
        Assert.Equal("longsword", template.Id);
        Assert.Equal("Longsword", template.Name);
        Assert.Equal(0x0F61, template.ItemId);
    }

    [Fact]
    public void Upsert_AddsNewTemplateToManagedFile()
    {
        using var dir = new TempTemplateDirectory();
        var store = new ItemTemplateYamlDocumentStore(dir.Path);
        var source = store.ResolveSourceFile("new_crate");

        store.Upsert(
            source,
            new ItemTemplateDefinition
            {
                Id = "new_crate",
                Name = "New Crate",
                ItemId = 0x0E3F
            }
        );

        Assert.Equal(System.IO.Path.Combine(dir.Path, ItemTemplateYamlDocumentStore.ManagedFileName), source);
        Assert.True(File.Exists(source));
        var table = YamlUtils.DeserializeFromFile<ItemTemplateTable>(source);
        var template = Assert.Single(table.ItemTemplates);
        Assert.Equal("new_crate", template.Id);
        Assert.Equal("New Crate", template.Name);
        Assert.Equal(0x0E3F, template.ItemId);
    }

    [Fact]
    public async Task CreateAsync_WritesManagedYamlAndRefreshesRegistry()
    {
        using var dir = new TempTemplateDirectory();
        var context = NewContext(dir);

        var result = await context.Service.CreateAsync(
            new()
            {
                Id = "web_crate",
                Name = "Web Crate",
                ItemId = 0x0E3F,
                Weight = 10,
                Tags = [" container ", "", "storage"]
            }
        );

        Assert.Equal("web_crate", result.Template.Id);
        Assert.Equal("_web.yaml", result.SourceFile);
        Assert.True(File.Exists(System.IO.Path.Combine(context.ItemsPath, "_web.yaml")));
        Assert.True(context.Templates.TryGet("web_crate", out var template));
        Assert.Equal("Web Crate", template.Name);
        Assert.Equal(["container", "storage"], template.Tags);
    }

    [Fact]
    public async Task CreateAsync_DuplicateId_ThrowsInvalidOperationException()
    {
        using var dir = new TempTemplateDirectory();
        var context = NewContext(dir);
        WriteItemTemplates(
            context.ItemsPath,
            "items.yaml",
            """
            item_templates:
                - id: existing
                  item_id: 3821
            """
        );
        context.ReloadTemplates();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Service.CreateAsync(new() { Id = "existing", ItemId = 3821 }).AsTask()
        );

        Assert.Contains("existing", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_MissingTemplate_ReturnsNull()
    {
        using var dir = new TempTemplateDirectory();
        var context = NewContext(dir);

        var result = await context.Service.UpdateAsync("missing", new() { Id = "missing", ItemId = 3821 });

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_IdMismatch_ThrowsInvalidOperationException()
    {
        using var dir = new TempTemplateDirectory();
        var context = NewContext(dir);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Service.UpdateAsync("crate", new() { Id = "other", ItemId = 0x0E3F }).AsTask()
        );

        Assert.Contains("id", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateAsync_RewritesSourceYamlAndRefreshesRegistry()
    {
        using var dir = new TempTemplateDirectory();
        var context = NewContext(dir);
        WriteItemTemplates(
            context.ItemsPath,
            "containers.yaml",
            """
            item_templates:
                - id: crate
                  name: Old Crate
                  item_id: 3647
            """
        );
        context.ReloadTemplates();

        var result = await context.Service.UpdateAsync(
            "crate",
            new()
            {
                Id = "crate",
                Name = "Edited Crate",
                Comment = "Edited from web",
                ItemId = 0x0E3F,
                Tags = ["container"]
            }
        );

        Assert.NotNull(result);
        Assert.Equal("containers.yaml", result.SourceFile);
        Assert.True(context.Templates.TryGet("crate", out var template));
        Assert.Equal("Edited Crate", template.Name);
        Assert.Equal("Edited from web", template.Comment);
        var table = YamlUtils.DeserializeFromFile<ItemTemplateTable>(System.IO.Path.Combine(context.ItemsPath, "containers.yaml"));
        Assert.Equal("Edited Crate", Assert.Single(table.ItemTemplates).Name);
    }

    [Fact]
    public async Task SaveAsync_InvalidBaseItem_DoesNotModifyRealFile()
    {
        using var dir = new TempTemplateDirectory();
        var context = NewContext(dir);
        var sourcePath = WriteItemTemplates(
            context.ItemsPath,
            "containers.yaml",
            """
            item_templates:
                - id: crate
                  name: Original Crate
                  item_id: 3647
            """
        );
        context.ReloadTemplates();
        var before = File.ReadAllText(sourcePath);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Service.UpdateAsync(
                "crate",
                new()
                {
                    Id = "crate",
                    BaseItem = "missing_base",
                    Name = "Broken Crate",
                    ItemId = 0x0E3F
                }
            ).AsTask()
        );

        Assert.Equal(before, File.ReadAllText(sourcePath));
        Assert.True(context.Templates.TryGet("crate", out var template));
        Assert.Equal("Original Crate", template.Name);
    }

    private static AuthoringContext NewContext(TempTemplateDirectory dir)
    {
        var directories = new DirectoriesConfig(dir.Path, DirectoryType.Templates_Items);
        var itemsPath = directories[DirectoryType.Templates_Items];
        var tileData = new TestTileDataStore((0x0E3F, "Tile Crate"), (3821, "Gold Coin"));
        var templates = new ItemTemplateService();
        var service = new ItemTemplateAuthoringService(
            directories,
            tileData,
            templates,
            new FakeHueStore()
        );

        return new(itemsPath, tileData, templates, service);
    }

    private static string WriteItemTemplates(string itemsPath, string fileName, string yaml)
    {
        var path = System.IO.Path.Combine(itemsPath, fileName);
        var directory = System.IO.Path.GetDirectoryName(path);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, yaml);

        return path;
    }

    private sealed record AuthoringContext(
        string ItemsPath,
        TestTileDataStore TileData,
        ItemTemplateService Templates,
        ItemTemplateAuthoringService Service
    )
    {
        public void ReloadTemplates()
        {
            Templates.Clear();
            Templates.UpsertRange(new ItemTemplateYamlLoader(ItemsPath, TileData).LoadAll());
        }
    }

    private sealed class FakeHueStore : IHueStore
    {
        public IReadOnlyList<Hue> Hues => [];

        public int Count => 0;

        public Hue? GetHue(int index)
            => null;
    }
}

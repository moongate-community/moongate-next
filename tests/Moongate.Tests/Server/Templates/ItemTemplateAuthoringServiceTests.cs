using Moongate.Core.Yaml;
using Moongate.Server.Services.Templates;
using Moongate.Tests.Support;
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
}

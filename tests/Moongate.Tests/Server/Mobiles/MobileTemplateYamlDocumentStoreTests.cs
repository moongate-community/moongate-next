using Moongate.Server.Services.Mobiles;
using Moongate.Tests.Support;
using Moongate.UO.Data.Templates.Mobiles;

namespace Moongate.Tests.Server.Mobiles;

public sealed class MobileTemplateYamlDocumentStoreTests
{
    [Fact]
    public void LoadTable_MissingFile_ReturnsEmptyTable()
    {
        using var dir = new TempTemplateDirectory();
        var store = new MobileTemplateYamlDocumentStore(dir.Path);
        var managedPath = Path.Combine(dir.Path, MobileTemplateYamlDocumentStore.ManagedFileName);

        var table = store.LoadTable(managedPath);

        Assert.Empty(table.MobileTemplates);
        Assert.False(File.Exists(managedPath));
    }

    [Fact]
    public void LoadTable_PathOutsideTemplatesDirectory_ThrowsInvalidOperationException()
    {
        using var dir = new TempTemplateDirectory();
        var store = new MobileTemplateYamlDocumentStore(dir.Path);
        var outsidePath = Path.Combine(Path.GetTempPath(), "outside.yaml");

        var exception = Assert.Throws<InvalidOperationException>(() => store.LoadTable(outsidePath));
        Assert.Contains("outside", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ResolveSourceFile_KnownId_ReturnsOwningYamlFile()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "humans.yaml",
            """
            mobile_templates:
              - id: peasant
                name: Peasant
                body: 400
            """
        );
        var store = new MobileTemplateYamlDocumentStore(dir.Path);

        var source = store.ResolveSourceFile("peasant");

        Assert.Equal(Path.Combine(dir.Path, "humans.yaml"), source);
    }

    [Fact]
    public void ResolveSourceFile_UnknownId_ReturnsManagedFilePath()
    {
        using var dir = new TempTemplateDirectory();
        var store = new MobileTemplateYamlDocumentStore(dir.Path);

        var source = store.ResolveSourceFile("does_not_exist");

        Assert.Equal(Path.Combine(dir.Path, MobileTemplateYamlDocumentStore.ManagedFileName), source);
    }

    [Fact]
    public void Upsert_ExistingId_ReplacesEntry()
    {
        using var dir = new TempTemplateDirectory();
        var store = new MobileTemplateYamlDocumentStore(dir.Path);
        var first = new MobileTemplateDefinition { Id = "mob_orc", Name = "Orc", Body = 17 };
        var second = new MobileTemplateDefinition { Id = "mob_orc", Name = "Orc Lord", Body = 33 };

        var sourceFile = store.ResolveSourceFile(first.Id);
        store.Upsert(sourceFile, first);
        store.Upsert(sourceFile, second);

        var table = store.LoadTable(sourceFile);

        var loaded = Assert.Single(table.MobileTemplates);
        Assert.Equal("mob_orc", loaded.Id);
        Assert.Equal(33, loaded.Body);
        Assert.Equal("Orc Lord", loaded.Name);
    }

    [Fact]
    public void Upsert_NewTemplate_WritesManagedFile_AndLoadTableRoundTrips()
    {
        using var dir = new TempTemplateDirectory();
        var store = new MobileTemplateYamlDocumentStore(dir.Path);
        var definition = new MobileTemplateDefinition
        {
            Id = "web_guard",
            Name = "Web Guard",
            Body = 400
        };

        var sourceFile = store.ResolveSourceFile(definition.Id);
        store.Upsert(sourceFile, definition);

        var resolvedFile = store.ResolveSourceFile(definition.Id);
        var table = store.LoadTable(resolvedFile);

        Assert.Equal(Path.Combine(dir.Path, MobileTemplateYamlDocumentStore.ManagedFileName), sourceFile);
        Assert.True(File.Exists(sourceFile));
        var loaded = Assert.Single(table.MobileTemplates);
        Assert.Equal("web_guard", loaded.Id);
        Assert.Equal("Web Guard", loaded.Name);
        Assert.Equal(400, loaded.Body);
    }
}

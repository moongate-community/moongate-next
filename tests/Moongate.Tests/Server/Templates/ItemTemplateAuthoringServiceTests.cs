using Moongate.Core.Geometry;
using Moongate.Core.Yaml;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.Data.Templates;
using Moongate.Server.Services.Loot;
using Moongate.Server.Services.Templates;
using Moongate.Tests.Support;
using Moongate.UO.Data.Data.Hues;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Interfaces.Hues;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Templates.Loot;
using ShaiRandom.Generators;
using Serial = Moongate.Core.Ids.Serial;

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
    public void Upsert_DoesNotSerializeCalculatedBaseSell()
    {
        using var dir = new TempTemplateDirectory();
        var store = new ItemTemplateYamlDocumentStore(dir.Path);
        var source = store.ResolveSourceFile("priced_item");

        store.Upsert(
            source,
            new ItemTemplateDefinition
            {
                Id = "priced_item",
                Name = "Priced Item",
                ItemId = 0x0E3F,
                Value = new()
                {
                    Buy = 100,
                    Sell = 40
                }
            }
        );

        var yaml = File.ReadAllText(source);
        Assert.DoesNotContain("base_sell", yaml, StringComparison.OrdinalIgnoreCase);
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
    public async Task CreateAsync_WithBaseItem_ThrowsInvalidOperationException()
    {
        using var dir = new TempTemplateDirectory();
        var context = NewContext(dir);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Service.CreateAsync(
                new()
                {
                    Id = "child_crate",
                    BaseItem = "crate_base",
                    ItemId = 0x0E3F
                }
            ).AsTask()
        );

        Assert.Contains("base_item", exception.Message);
        Assert.False(File.Exists(System.IO.Path.Combine(context.ItemsPath, "_web.yaml")));
    }

    [Fact]
    public async Task CreateAsync_UnknownContentsLootTemplate_DoesNotModifyManagedFile()
    {
        using var dir = new TempTemplateDirectory();
        var context = NewContext(dir);
        var source = System.IO.Path.Combine(context.ItemsPath, "_web.yaml");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Service.CreateAsync(
                new()
                {
                    Id = "broken_chest",
                    Name = "Broken Chest",
                    ItemId = 0x0E3F,
                    Contents = new()
                    {
                        LootTemplate = "missing",
                        RefillEvery = TimeSpan.FromHours(6)
                    }
                }
            ).AsTask()
        );

        Assert.Contains("missing", exception.Message);
        Assert.False(File.Exists(source));
        Assert.False(context.Templates.TryGet("broken_chest", out _));
    }

    [Fact]
    public async Task CreateAsync_NonContainerContents_ThrowsInvalidOperationException()
    {
        using var dir = new TempTemplateDirectory();
        var context = NewContext(dir, containerItemIds: []);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Service.CreateAsync(
                new()
                {
                    Id = "bad_sword_chest",
                    Name = "Bad Sword Chest",
                    ItemId = 0x0F61,
                    Contents = new()
                    {
                        LootTemplate = "common",
                        RefillEvery = TimeSpan.FromHours(6)
                    }
                }
            ).AsTask()
        );

        Assert.Contains("container", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(context.Templates.TryGet("bad_sword_chest", out _));
    }

    [Fact]
    public async Task UpdateAsync_LootReferencedTemplateBecomingAbstract_DoesNotModifyRealFile()
    {
        using var dir = new TempTemplateDirectory();
        var context = NewContext(dir);
        var sourcePath = WriteItemTemplates(
            context.ItemsPath,
            "loot_items.yaml",
            """
            item_templates:
                - id: crate
                  name: Loot Crate
                  item_id: 3647
            """
        );
        context.ReloadTemplates();
        context.RegistryStore.SetRegistry(
            new LootTableRegistry(
                [
                    new()
                    {
                        Id = "crate_loot",
                        Content = [new() { Item = "crate" }]
                    }
                ],
                context.Templates.GetAll()
            )
        );
        var before = File.ReadAllText(sourcePath);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Service.UpdateAsync(
                "crate",
                new()
                {
                    Id = "crate",
                    Name = "Loot Crate",
                    ItemId = 0x0E3F,
                    IsAbstract = true
                }
            ).AsTask()
        );

        Assert.Contains("abstract", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(before, File.ReadAllText(sourcePath));
        Assert.True(context.Templates.TryGet("crate", out var template));
        Assert.False(template.IsAbstract);
    }

    [Fact]
    public async Task UpdateAsync_RebuildsLootRegistryAfterTagChange()
    {
        using var dir = new TempTemplateDirectory();
        var context = NewContext(dir);
        WriteItemTemplates(
            context.ItemsPath,
            "loot_items.yaml",
            """
            item_templates:
                - id: apple
                  name: Apple
                  item_id: 2512
                  tags: [food]
                - id: bread_loaf
                  name: Bread Loaf
                  item_id: 4155
                  tags: [food]
            """
        );
        context.ReloadTemplates();
        context.RegistryStore.SetRegistry(
            new LootTableRegistry(
                [
                    new()
                    {
                        Id = "food_loot",
                        Content = [new() { Category = "food" }]
                    }
                ],
                context.Templates.GetAll()
            )
        );
        context.LootService.SetRegistry(context.RegistryStore.Registry);

        await context.Service.UpdateAsync(
            "bread_loaf",
            new()
            {
                Id = "bread_loaf",
                Name = "Bread Loaf",
                ItemId = 4155,
                Tags = ["bakery"]
            }
        );

        Assert.True(context.RegistryStore.Registry.TryGetByTag("food", out var foodTemplates));
        Assert.Equal("apple", Assert.Single(foodTemplates).Id);
        Assert.True(context.RegistryStore.Registry.TryGetByTag("bakery", out var bakeryTemplates));
        Assert.Equal("bread_loaf", Assert.Single(bakeryTemplates).Id);
        Assert.True(context.LootService.Has("food_loot"));
    }

    [Fact]
    public async Task CreateAsync_ConcurrentCreates_PreservesAllManagedTemplates()
    {
        using var dir = new TempTemplateDirectory();
        var context = NewContext(dir);
        var requests = Enumerable.Range(0, 24)
                                 .Select(
                                     index => new ItemTemplateEditRequest
                                     {
                                         Id = $"web_item_{index:D2}",
                                         Name = $"Web Item {index:D2}",
                                         ItemId = 0x0E3F
                                     }
                                 )
                                 .ToArray();

        await Task.WhenAll(
            requests.Select(
                request => Task.Run(async () => await context.Service.CreateAsync(request))
            )
        );

        var table = YamlUtils.DeserializeFromFile<ItemTemplateTable>(
            System.IO.Path.Combine(context.ItemsPath, "_web.yaml")
        );

        Assert.Equal(requests.Length, table.ItemTemplates.Count);
        Assert.All(requests, request => Assert.Contains(table.ItemTemplates, template => template.Id == request.Id));
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
    public async Task UpdateAsync_InheritedTemplate_DoesNotModifyRealFile()
    {
        using var dir = new TempTemplateDirectory();
        var context = NewContext(dir);
        var sourcePath = WriteItemTemplates(
            context.ItemsPath,
            "inherited.yaml",
            """
            item_templates:
                - id: base_crate
                  name: Base Crate
                  item_id: 3647
                  tags: [container]
                - id: child_crate
                  base_item: base_crate
                  name: Child Crate
                  item_id: 3647
            """
        );
        context.ReloadTemplates();
        var before = File.ReadAllText(sourcePath);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Service.UpdateAsync(
                "child_crate",
                new()
                {
                    Id = "child_crate",
                    BaseItem = "base_crate",
                    Name = "Edited Child",
                    ItemId = 0x0E3F
                }
            ).AsTask()
        );

        Assert.Contains("base_item", exception.Message);
        Assert.Equal(before, File.ReadAllText(sourcePath));
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

    private static AuthoringContext NewContext(TempTemplateDirectory dir, int[]? containerItemIds = null)
    {
        var directories = new DirectoriesConfig(dir.Path, DirectoryType.Templates_Items);
        var itemsPath = directories[DirectoryType.Templates_Items];
        var tileData = new TestTileDataStore((0x0E3F, "Tile Crate"), (0x0F61, "Longsword"), (3821, "Gold Coin"));
        var templates = new ItemTemplateService();
        var registryStore = new LootTableRegistryStore();
        registryStore.SetRegistry(Registry("common", templates.GetAll()));
        var lootService = new LootService(
            templates,
            new(() => new FakeItemFactory(templates)),
            new MizuchiRandom(1UL, 1UL)
        );
        lootService.SetRegistry(registryStore.Registry);
        var service = new ItemTemplateAuthoringService(
            directories,
            tileData,
            templates,
            new FakeHueStore(),
            registryStore,
            new FakeItemService(containerItemIds ?? [0x0E3F]),
            lootService
        );

        return new(itemsPath, tileData, templates, registryStore, lootService, service);
    }

    private static LootTableRegistry Registry(string id, IEnumerable<ItemTemplateDefinition> templates)
        => new(
            [
                new()
                {
                    Id = id,
                    Content = []
                }
            ],
            templates
        );

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
        LootTableRegistryStore RegistryStore,
        LootService LootService,
        ItemTemplateAuthoringService Service
    )
    {
        public void ReloadTemplates()
        {
            Templates.ReplaceAll(new ItemTemplateYamlLoader(ItemsPath, TileData).LoadAll());
        }
    }

    private sealed class FakeHueStore : IHueStore
    {
        public IReadOnlyList<Hue> Hues => [];

        public int Count => 0;

        public Hue? GetHue(int index)
            => null;
    }

    private sealed class FakeItemService : IItemService
    {
        private readonly HashSet<int> _containerItemIds;

        public FakeItemService(params int[] containerItemIds)
        {
            _containerItemIds = [.. containerItemIds];
        }

        public ValueTask<bool> AddItemAsync(
            ItemEntity container,
            ItemEntity child,
            Point2D position,
            CancellationToken cancellationToken = default
        )
            => throw new NotSupportedException();

        public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public ValueTask<ItemEntity> CreateAsync(ItemEntity item, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public ValueTask<bool> DeleteAsync(Serial id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public ValueTask<ItemEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public bool IsContainer(ItemEntity item)
            => IsContainer(item.ItemId);

        public bool IsContainer(int itemId)
            => _containerItemIds.Contains(itemId);

        public bool IsDoor(ItemEntity item)
            => throw new NotSupportedException();

        public bool IsDoor(int itemId)
            => throw new NotSupportedException();

        public ValueTask<bool> RemoveItemAsync(
            ItemEntity container,
            Serial itemId,
            CancellationToken cancellationToken = default
        )
            => throw new NotSupportedException();

        public ValueTask<int> TotalWeightAsync(ItemEntity item, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FakeItemFactory : IItemFactoryService
    {
        private readonly IItemTemplateService _templates;
        private uint _next = Serial.ItemOffset + 1;

        public FakeItemFactory(IItemTemplateService templates)
        {
            _templates = templates;
        }

        public ValueTask<ItemEntity> CreateFromTemplateAsync(
            string templateId,
            CancellationToken cancellationToken = default
        )
            => CreateFromTemplateAsync(templateId, 1, cancellationToken);

        public ValueTask<ItemEntity> CreateFromTemplateAsync(
            string templateId,
            int amount,
            CancellationToken cancellationToken = default
        )
        {
            if (!_templates.TryGet(templateId, out var template))
            {
                throw new InvalidOperationException($"Item template '{templateId}' not found.");
            }

            if (template.IsAbstract)
            {
                throw new InvalidOperationException($"Item template '{templateId}' is abstract.");
            }

            return ValueTask.FromResult(
                new ItemEntity
                {
                    Id = new(_next++),
                    ItemId = template.ItemId,
                    Amount = amount,
                    IsStackable = template.IsStackable
                }
            );
        }
    }
}

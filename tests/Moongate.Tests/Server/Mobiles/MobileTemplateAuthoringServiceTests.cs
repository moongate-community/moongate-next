using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.Data.Templates;
using Moongate.Server.Services.Loot;
using Moongate.Server.Services.Mobiles;
using Moongate.Server.Services.Templates;
using Moongate.Tests.Support;

namespace Moongate.Tests.Server.Mobiles;

public sealed class MobileTemplateAuthoringServiceTests
{
    private sealed record AuthoringContext(
        string MobilesPath,
        MobileTemplateService Templates,
        ItemTemplateService ItemTemplates,
        LootTableRegistryStore RegistryStore,
        MobileTemplateAuthoringService Service
    );

    [Fact]
    public async Task CreateAsync_DuplicateId_Throws()
    {
        using var dir = new TempTemplateDirectory();
        var ctx = NewContext(dir);

        await ctx.Service.CreateAsync(CreateRequest("orc", 7));

        await Assert.ThrowsAsync<InvalidOperationException>(() => ctx.Service.CreateAsync(CreateRequest("orc", 7)).AsTask());
    }

    [Fact]
    public async Task CreateAsync_WritesManagedYaml_AndReturnsDetail()
    {
        using var dir = new TempTemplateDirectory();
        var ctx = NewContext(dir);

        var result = await ctx.Service.CreateAsync(CreateRequest("orc", 7));

        Assert.Equal("_web.yaml", result.SourceFile);
        Assert.Equal("orc", result.Template.Id);
        Assert.True(ctx.Templates.TryGet("orc", out _));
        Assert.True(File.Exists(Path.Combine(ctx.MobilesPath, "_web.yaml")));
    }

    [Fact]
    public async Task UpdateAsync_BaseMobile_Throws()
    {
        using var dir = new TempTemplateDirectory();
        var ctx = NewContext(dir);

        // Seed a template that has BaseMobile set directly into the registry
        ctx.Templates.UpsertRange(
            [
                new()
                {
                    Id = "orc_base",
                    Body = 7,
                    BaseMobile = "some_parent"
                }
            ]
        );

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => ctx.Service.UpdateAsync("orc_base", CreateRequest("orc_base", 7)).AsTask()
        );
    }

    [Fact]
    public async Task UpdateAsync_Existing_Persists()
    {
        using var dir = new TempTemplateDirectory();
        var ctx = NewContext(dir);

        await ctx.Service.CreateAsync(CreateRequest("orc", 7));
        var result = await ctx.Service.UpdateAsync("orc", CreateRequest("orc", 42));

        Assert.NotNull(result);
        Assert.Equal(42, result.Template.Body);
        Assert.True(ctx.Templates.TryGet("orc", out var updated));
        Assert.Equal(42, updated.Body);
    }

    [Fact]
    public async Task UpdateAsync_IdMismatch_Throws()
    {
        using var dir = new TempTemplateDirectory();
        var ctx = NewContext(dir);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => ctx.Service.UpdateAsync("orc", CreateRequest("goblin", 7)).AsTask()
        );
    }

    [Fact]
    public async Task UpdateAsync_MissingId_ReturnsNull()
    {
        using var dir = new TempTemplateDirectory();
        var ctx = NewContext(dir);

        var result = await ctx.Service.UpdateAsync("ghost", CreateRequest("ghost", 1));

        Assert.Null(result);
    }

    [Fact]
    public async Task Validate_BlankId_Throws()
    {
        using var dir = new TempTemplateDirectory();
        var ctx = NewContext(dir);

        await Assert.ThrowsAsync<InvalidOperationException>(() => ctx.Service.CreateAsync(CreateRequest("   ", 7)).AsTask());
    }

    [Fact]
    public async Task Validate_BodyZero_Throws()
    {
        using var dir = new TempTemplateDirectory();
        var ctx = NewContext(dir);

        await Assert.ThrowsAsync<InvalidOperationException>(() => ctx.Service.CreateAsync(CreateRequest("orc", 0)).AsTask());
    }

    [Fact]
    public async Task Validate_KnownEquipmentItemId_DoesNotThrow()
    {
        using var dir = new TempTemplateDirectory();
        var ctx = NewContext(dir);

        var request = CreateRequest("orc", 7);
        request.Equipment = ["leather_chest"];

        var result = await ctx.Service.CreateAsync(request);

        Assert.Equal("orc", result.Template.Id);
    }

    [Fact]
    public async Task Validate_KnownLootTableId_DoesNotThrow()
    {
        using var dir = new TempTemplateDirectory();
        var ctx = NewContext(dir);

        var request = CreateRequest("orc", 7);
        request.LootTables = ["basic_loot"];

        var result = await ctx.Service.CreateAsync(request);

        Assert.Equal("orc", result.Template.Id);
    }

    [Fact]
    public async Task Validate_UnknownEquipmentItemId_Throws()
    {
        using var dir = new TempTemplateDirectory();
        var ctx = NewContext(dir);

        var request = CreateRequest("orc", 7);
        request.Equipment = ["nope"];

        await Assert.ThrowsAsync<InvalidOperationException>(() => ctx.Service.CreateAsync(request).AsTask());
    }

    [Fact]
    public async Task Validate_UnknownLootTableId_Throws()
    {
        using var dir = new TempTemplateDirectory();
        var ctx = NewContext(dir);

        var request = CreateRequest("orc", 7);
        request.LootTables = ["nope"];

        await Assert.ThrowsAsync<InvalidOperationException>(() => ctx.Service.CreateAsync(request).AsTask());
    }

    private static MobileTemplateEditRequest CreateRequest(string id, int body)
        => new() { Id = id, Body = body };

    private static AuthoringContext NewContext(TempTemplateDirectory dir)
    {
        var directories = new DirectoriesConfig(dir.Path, DirectoryType.Templates_Mobiles);
        var mobilesPath = directories[DirectoryType.Templates_Mobiles];
        var templates = new MobileTemplateService();
        var itemTemplates = new ItemTemplateService();
        itemTemplates.ReplaceAll(
            [
                new() { Id = "leather_chest", ItemId = 5131, Name = "Leather Chest" }
            ]
        );
        var registryStore = new LootTableRegistryStore();
        registryStore.SetRegistry(
            new(
                [new() { Id = "basic_loot", Content = [] }],
                []
            )
        );
        var service = new MobileTemplateAuthoringService(directories, templates, itemTemplates, registryStore);

        return new(mobilesPath, templates, itemTemplates, registryStore, service);
    }
}

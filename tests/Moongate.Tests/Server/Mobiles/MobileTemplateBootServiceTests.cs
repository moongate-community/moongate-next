using Moongate.Server.Services.Mobiles;
using Moongate.Server.Services.Templates;
using Moongate.Tests.Support;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Types.Items;

namespace Moongate.Tests.Server.Mobiles;

public sealed class MobileTemplateBootServiceTests
{
    private sealed class FakeLoot : ILootService
    {
        public ValueTask<IReadOnlyList<ItemEntity>> GenerateAsync(
            string lootTableId,
            CancellationToken cancellationToken = default
        )
            => throw new NotSupportedException();

        public bool Has(string lootTableId)
            => true;
    }

    [Fact]
    public async Task StartAsync_InvalidFile_Throws()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile("a.yaml", "mobile_templates:\n  - id: g\n    equipment:\n      - item: does_not_exist\n");
        var boot = new MobileTemplateBootService(new(dir.Path), new MobileTemplateService(), Items(), new FakeLoot());

        await Assert.ThrowsAsync<InvalidOperationException>(() => boot.StartAsync(CancellationToken.None));
    }

    [Fact]
    public async Task StartAsync_MissingDirectory_NoThrow()
    {
        using var dir = new TempTemplateDirectory();
        var templates = new MobileTemplateService();
        var boot = new MobileTemplateBootService(new(dir.Path), templates, Items(), new FakeLoot());

        await boot.StartAsync(CancellationToken.None);

        Assert.Equal(0, templates.Count);
    }

    [Fact]
    public async Task StartAsync_ValidFile_PopulatesRegistry()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile("a.yaml", "mobile_templates:\n  - id: guard\n    body: 400\n    equipment:\n      - item: katana\n");
        var templates = new MobileTemplateService();
        var boot = new MobileTemplateBootService(new(dir.Path), templates, Items(), new FakeLoot());

        await boot.StartAsync(CancellationToken.None);

        Assert.Equal(1, templates.Count);
        Assert.True(templates.TryGet("guard", out _));
    }

    private static ItemTemplateService Items()
    {
        var registry = new ItemTemplateService();
        registry.UpsertRange([new() { Id = "katana", ItemId = 5119, Layer = ItemLayerType.OneHanded }]);

        return registry;
    }
}

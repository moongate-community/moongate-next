using Moongate.Server.Services.Templates;
using Moongate.Tests.Support;

namespace Moongate.Tests.Server.Templates;

public sealed class ItemTemplateBootServiceTests
{
    [Fact]
    public async Task StartAsync_ClearsPreviousRegistryEntries()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "clothing.yaml",
            """
            item_templates:
                - id: shirt
            """
        );
        var registry = new ItemTemplateService();
        registry.UpsertRange([new() { Id = "stale" }]);
        var bootService = new ItemTemplateBootService(new(dir.Path), registry);

        await bootService.StartAsync(CancellationToken.None);

        Assert.False(registry.TryGet("stale", out _));
        Assert.Equal(1, registry.Count);
    }

    [Fact]
    public async Task StartAsync_InvalidTemplates_Throws()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            "bad.yaml",
            """
            item_templates:
                - id: a
                  base_item: missing
            """
        );
        var bootService = new ItemTemplateBootService(new(dir.Path), new ItemTemplateService());

        await Assert.ThrowsAsync<InvalidOperationException>(() => bootService.StartAsync(CancellationToken.None));
    }

    [Fact]
    public async Task StartAsync_LoadsTemplatesIntoRegistry()
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
        var registry = new ItemTemplateService();
        var bootService = new ItemTemplateBootService(new(dir.Path), registry);

        await bootService.StartAsync(CancellationToken.None);

        Assert.Equal(1, registry.Count);
        Assert.True(registry.TryGet("plain_shirt", out _));
    }
}

using Moongate.Server.Services.Loadouts;
using Moongate.Server.Services.Templates;
using Moongate.Server.Services.World;
using Moongate.Tests.Support;
using Moongate.UO.Data.Types.Items;

namespace Moongate.Tests.Server.Loadouts;

public sealed class StarterLoadoutBootServiceTests
{
    [Fact]
    public async Task StartAsync_InvalidFile_Throws()
    {
        using var dir = new TempTemplateDirectory();
        dir.WriteFile(
            StarterLoadoutYamlLoader.StarterLoadoutFileName,
            """
            starter_loadout:
                backpack_template: backpack
                base:
                    backpack_items:
                        - template: does_not_exist
            """
        );
        var templates = NewTemplates();
        var bootService = new StarterLoadoutBootService(
            new(dir.Path),
            NewLoadoutService(templates),
            templates,
            NewProfessions()
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() => bootService.StartAsync(CancellationToken.None));
    }

    [Fact]
    public async Task StartAsync_MissingFile_NoThrowAndEmptyLoadout()
    {
        using var dir = new TempTemplateDirectory();
        var templates = NewTemplates();
        var loadouts = NewLoadoutService(templates);
        var bootService = new StarterLoadoutBootService(
            new(dir.Path),
            loadouts,
            templates,
            NewProfessions()
        );

        await bootService.StartAsync(CancellationToken.None);

        Assert.True(loadouts.Compose(0, null).IsEmpty);
    }

    [Fact]
    public async Task StartAsync_ValidFile_PopulatesService()
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
                          amount: 500
            """
        );
        var templates = NewTemplates();
        var loadouts = NewLoadoutService(templates);
        var bootService = new StarterLoadoutBootService(
            new(dir.Path),
            loadouts,
            templates,
            NewProfessions()
        );

        await bootService.StartAsync(CancellationToken.None);

        var composed = loadouts.Compose(0, null);
        Assert.False(composed.IsEmpty);
        Assert.Single(composed.BackpackItems);
    }

    private static StarterLoadoutService NewLoadoutService(ItemTemplateService templates)
        => new(
            templates,
            new(static () => new ThrowingItemFactory()),
            new(static () => new ThrowingMobileService()),
            new(static () => new ThrowingItemService())
        );

    private static ProfessionDataService NewProfessions()
    {
        var service = new ProfessionDataService();
        service.SetProfessions([new("Warrior", "Warrior", 0, 0, 0, true, 0, "Profession", [], [])]);

        return service;
    }

    private static ItemTemplateService NewTemplates()
    {
        var registry = new ItemTemplateService();
        registry.UpsertRange(
            [
                new() { Id = "backpack", Name = "Backpack", ItemId = 3701, Layer = ItemLayerType.Backpack },
                new() { Id = "gold_coin", Name = "Gold", ItemId = 3821, IsStackable = true }
            ]
        );

        return registry;
    }
}

using Moongate.Server.Data.World;
using Moongate.Server.Services.Loadouts;
using Moongate.Server.Services.Templates;
using Moongate.Server.Services.World;
using Moongate.Tests.Support;
using Moongate.UO.Data.Templates.Items;
using Moongate.UO.Data.Types.Items;

namespace Moongate.Tests.Server.Loadouts;

public sealed class StarterLoadoutBootServiceTests
{
    private static ItemTemplateService NewTemplates()
    {
        var registry = new ItemTemplateService();
        registry.UpsertRange(
            [
                new ItemTemplateDefinition { Id = "backpack", Name = "Backpack", ItemId = 3701, Layer = ItemLayerType.Backpack },
                new ItemTemplateDefinition { Id = "gold_coin", Name = "Gold", ItemId = 3821, IsStackable = true }
            ]
        );

        return registry;
    }

    private static ProfessionDataService NewProfessions()
    {
        var service = new ProfessionDataService();
        service.SetProfessions([new ProfessionEntry("Warrior", "Warrior", 0, 0, 0, true, 0, "Profession", [], [])]);

        return service;
    }

    private static StarterLoadoutService NewLoadoutService(ItemTemplateService templates)
        => new(
            templates,
            new ThrowingItemFactory(),
            new ThrowingMobileService(),
            new ThrowingItemService()
        );

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
            new StarterLoadoutYamlLoader(dir.Path),
            loadouts,
            templates,
            NewProfessions()
        );

        await bootService.StartAsync(CancellationToken.None);

        var composed = loadouts.Compose(0, null);
        Assert.False(composed.IsEmpty);
        Assert.Single(composed.BackpackItems);
    }

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
            new StarterLoadoutYamlLoader(dir.Path),
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
            new StarterLoadoutYamlLoader(dir.Path),
            loadouts,
            templates,
            NewProfessions()
        );

        await bootService.StartAsync(CancellationToken.None);

        Assert.True(loadouts.Compose(0, null).IsEmpty);
    }
}

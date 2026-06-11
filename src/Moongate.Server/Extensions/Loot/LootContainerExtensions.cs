using DryIoc;
using Moongate.Core.Data.Directories;
using Moongate.Core.Random;
using Moongate.Core.Types;
using Moongate.Server.Extensions.Hosting;
using Moongate.Server.Services.Loot;
using Moongate.UO.Data.Interfaces.Services;
using ShaiRandom.Generators;

namespace Moongate.Server.Extensions.Loot;

/// <summary>
/// DryIoc-native registration helpers for loot table services.
/// </summary>
public static class LootContainerExtensions
{
    private const int LootTablesBootPriority = 14;

    /// <summary>
    /// Registers the loot service, the YAML loader and the fail-fast boot service
    /// (priority 14: after starter loadouts, before persistence/network).
    /// </summary>
    public static IContainer AddMoongateLootTables(this IContainer container)
    {
        ArgumentNullException.ThrowIfNull(container);

        container.RegisterInstance<IEnhancedRandom>(BuiltInRng.Generator, IfAlreadyRegistered.Keep);
        container.Register<ILootService, LootService>(Reuse.Singleton);
        container.Register<LootTableRegistryStore>(Reuse.Singleton);
        container.RegisterDelegate<LootTemplateProjectionService>(
            resolver => new(resolver.Resolve<IItemTemplateService>().GetAll()),
            Reuse.Singleton
        );
        container.RegisterDelegate(
            static resolver => new LootTableYamlLoader(resolver.Resolve<DirectoriesConfig>()[DirectoryType.Templates_Loot]),
            Reuse.Singleton
        );
        container.AddMoongateHosting();
        container.AddMoongateService<LootTableBootService>(LootTablesBootPriority);

        return container;
    }
}

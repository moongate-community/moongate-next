using DryIoc;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.Extensions.Hosting;
using Moongate.Server.Services.Loadouts;
using Moongate.UO.Data.Interfaces.Services;

namespace Moongate.Server.Extensions.Loadouts;

/// <summary>
///     DryIoc-native registration helpers for starter loadout services.
/// </summary>
public static class LoadoutContainerExtensions
{
    private const int StarterLoadoutsBootPriority = 13;

    /// <summary>
    ///     Registers the starter loadout service, the YAML loader and the fail-fast
    ///     boot service (priority 13: after item templates, before persistence/network).
    /// </summary>
    public static IContainer AddMoongateStarterLoadouts(this IContainer container)
    {
        ArgumentNullException.ThrowIfNull(container);

        container.Register<IStarterLoadoutService, StarterLoadoutService>(Reuse.Singleton);
        container.RegisterDelegate(
            static resolver => new StarterLoadoutYamlLoader(
                resolver.Resolve<DirectoriesConfig>()[DirectoryType.Templates_Loadouts]
            ),
            Reuse.Singleton
        );
        container.AddMoongateHosting();
        container.AddMoongateService<StarterLoadoutBootService>(StarterLoadoutsBootPriority);

        return container;
    }
}

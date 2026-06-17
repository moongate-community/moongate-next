using DryIoc;
using Moongate.Core.Data.Directories;
using Moongate.Plugins.Interfaces.Plugins;
using Moongate.Plugins.Services;
using Moongate.Server.Extensions.Configuration;

namespace Moongate.Server.Extensions.Plugins;

/// <summary>
///     DryIoc-native registration helper for boot-time .NET plugins.
/// </summary>
public static class PluginContainerExtensions
{
    /// <summary>
    ///     Loads trusted .NET plugins from the configured plugins directory and lets them register into the container.
    ///     Must run before <see cref="ConfigContainerExtensions.AddMoongateConfig" />.
    /// </summary>
    public static IContainer AddMoongatePlugins(this IContainer container, DirectoriesConfig directoriesConfig)
    {
        return container.AddMoongatePlugins(directoriesConfig, []);
    }

    /// <summary>
    ///     Loads trusted .NET plugins from the configured plugins directory and registers embedded plugins.
    ///     Must run before <see cref="ConfigContainerExtensions.AddMoongateConfig" />.
    /// </summary>
    public static IContainer AddMoongatePlugins(
        this IContainer container,
        DirectoriesConfig directoriesConfig,
        params IMoongatePlugin[] embeddedPlugins
    )
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(directoriesConfig);
        ArgumentNullException.ThrowIfNull(embeddedPlugins);

        var loader = new PluginLoaderService();
        var loaded = loader.LoadAndConfigure(container, directoriesConfig, embeddedPlugins);
        container.RegisterInstance<IPluginCatalogService>(new PluginCatalogService(loaded));

        return container;
    }
}

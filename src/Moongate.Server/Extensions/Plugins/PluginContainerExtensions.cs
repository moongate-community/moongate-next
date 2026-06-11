using DryIoc;
using Moongate.Core.Data.Directories;
using Moongate.Plugins.Interfaces.Plugins;
using Moongate.Plugins.Services;
using Moongate.Server.Extensions.Configuration;

namespace Moongate.Server.Extensions.Plugins;

/// <summary>
/// DryIoc-native registration helper for boot-time .NET plugins.
/// </summary>
public static class PluginContainerExtensions
{
    /// <summary>
    /// Loads trusted .NET plugins from the configured plugins directory and lets them register into the container.
    /// Must run before <see cref="ConfigContainerExtensions.AddMoongateConfig" />.
    /// </summary>
    public static IContainer AddMoongatePlugins(this IContainer container, DirectoriesConfig directoriesConfig)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(directoriesConfig);

        var loader = new PluginLoaderService();
        var loaded = loader.LoadAndConfigure(container, directoriesConfig);
        container.RegisterInstance<IPluginCatalogService>(new PluginCatalogService(loaded));

        return container;
    }
}

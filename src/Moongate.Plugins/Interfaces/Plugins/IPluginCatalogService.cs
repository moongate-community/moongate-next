using Moongate.Plugins.Data;

namespace Moongate.Plugins.Interfaces.Plugins;

/// <summary>Provides sanitized runtime visibility into loaded plugins.</summary>
public interface IPluginCatalogService
{
    /// <summary>Returns metadata for every plugin loaded at boot.</summary>
    IReadOnlyList<PluginCatalogEntry> GetLoadedPlugins();

    /// <summary>Returns sanitized runtime config for a loaded plugin, or null when the plugin is unknown.</summary>
    ValueTask<PluginConfigView?> GetConfigAsync(string pluginId, CancellationToken cancellationToken = default);
}

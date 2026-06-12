using Moongate.Plugins.Data;

namespace Moongate.Plugins.Interfaces.Plugins;

/// <summary>Provides sanitized runtime visibility into loaded plugins.</summary>
public interface IPluginCatalogService
{
    /// <summary>Returns sanitized runtime config for a loaded plugin, or null when the plugin is unknown.</summary>
    ValueTask<PluginConfigView?> GetConfigAsync(string pluginId, CancellationToken cancellationToken = default);

    /// <summary>Returns an editable config form for configurable plugins, or null when unsupported.</summary>
    ValueTask<PluginConfigForm?> GetConfigFormAsync(string pluginId, CancellationToken cancellationToken = default);

    /// <summary>Returns metadata for every plugin loaded at boot.</summary>
    IReadOnlyList<PluginCatalogEntry> GetLoadedPlugins();

    /// <summary>Saves plugin config values for configurable plugins, or null when unsupported.</summary>
    ValueTask<PluginConfigSaveResult?> SaveConfigAsync(
        string pluginId,
        PluginConfigSaveRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>Runs a plugin-specific config test, or null when unsupported.</summary>
    ValueTask<PluginTestResult?> TestAsync(string pluginId, CancellationToken cancellationToken = default);
}

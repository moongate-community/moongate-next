using Moongate.Plugins.Data;

namespace Moongate.Plugins.Interfaces.Plugins;

/// <summary>Optional capability for plugins exposing a small admin-editable config form.</summary>
public interface IConfigurablePlugin
{
    /// <summary>Returns the current form descriptor and field values.</summary>
    ValueTask<PluginConfigForm> GetConfigFormAsync(CancellationToken cancellationToken = default);

    /// <summary>Saves validated plugin configuration values.</summary>
    ValueTask<PluginConfigSaveResult> SaveConfigAsync(
        PluginConfigSaveRequest request,
        CancellationToken cancellationToken = default
    );
}

using Moongate.Plugins.Data;
using Moongate.Plugins.Interfaces.Plugins;

namespace Moongate.Plugins.Configuration;

/// <summary>
///     Base class for plugins exposing an admin-editable config form. It binds the flat request values
///     onto the plugin's existing typed config (preserving fields the request omits) and hands the
///     merged, strongly-typed object to <see cref="SaveTypedConfigAsync" />, so subclasses never deal
///     with the stringly-typed value dictionary.
/// </summary>
/// <typeparam name="TConfig">The plugin's strongly-typed configuration object.</typeparam>
public abstract class ConfigurablePlugin<TConfig> : IConfigurablePlugin
    where TConfig : class
{
    public virtual async ValueTask<PluginConfigForm> GetConfigFormAsync(CancellationToken cancellationToken = default)
    {
        return ConfigFormScanner.BuildForm(await LoadConfigAsync(cancellationToken));
    }

    public async ValueTask<PluginConfigSaveResult> SaveConfigAsync(
        PluginConfigSaveRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Values is null)
        {
            return new PluginConfigSaveResult(false, false, ["Config values are required."], null);
        }

        TConfig merged;

        try
        {
            var existing = await LoadConfigAsync(cancellationToken);
            merged = PluginConfigBinder.Apply(existing, request.Values);
        }
        catch (Exception ex)
        {
            return new PluginConfigSaveResult(false, false, [$"Invalid configuration: {ex.Message}"], null);
        }

        return await SaveTypedConfigAsync(merged, cancellationToken);
    }

    /// <summary>Loads the plugin's current typed config, used as the base for the overlay.</summary>
    protected abstract ValueTask<TConfig> LoadConfigAsync(CancellationToken cancellationToken);

    /// <summary>Validates and persists the merged typed config, returning the save result.</summary>
    protected abstract ValueTask<PluginConfigSaveResult> SaveTypedConfigAsync(
        TConfig config,
        CancellationToken cancellationToken
    );
}

using Moongate.Plugins.Data;

namespace Moongate.Plugins.Interfaces.Plugins;

/// <summary>Optional capability for plugins that can test their runtime configuration.</summary>
public interface ITestablePlugin
{
    /// <summary>Runs a plugin-specific configuration test without exposing secrets.</summary>
    ValueTask<PluginTestResult> TestAsync(CancellationToken cancellationToken = default);
}

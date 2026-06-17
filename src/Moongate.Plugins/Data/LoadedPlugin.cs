using System.Reflection;
using Moongate.Plugins.Interfaces.Plugins;

namespace Moongate.Plugins.Data;

/// <summary>
///     A plugin instance loaded from a plugin package directory.
/// </summary>
public sealed class LoadedPlugin
{
    public LoadedPlugin(string pluginDirectory, IMoongatePlugin instance, Assembly assembly)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginDirectory);
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(assembly);

        PluginDirectory = Path.GetFullPath(pluginDirectory);
        Instance = instance;
        Assembly = assembly;
        Metadata = instance.Metadata ??
                   throw new InvalidOperationException($"Plugin {instance.GetType().FullName} returned null metadata.");
    }

    /// <summary>Absolute directory containing the plugin package.</summary>
    public string PluginDirectory { get; }

    /// <summary>The instantiated plugin.</summary>
    public IMoongatePlugin Instance { get; }

    /// <summary>The plugin metadata.</summary>
    public PluginMetadata Metadata { get; }

    /// <summary>The assembly containing the plugin type.</summary>
    public Assembly Assembly { get; }
}

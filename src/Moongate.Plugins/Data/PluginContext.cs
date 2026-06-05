using Moongate.Abstractions.Configuration;
using Moongate.Core.Data.Directories;
using Serilog;
using Tomlyn;

namespace Moongate.Plugins.Data;

/// <summary>
/// Per-plugin startup context passed to <see cref="Moongate.Plugins.Interfaces.Plugins.IMoongatePlugin" />.
/// </summary>
public sealed class PluginContext
{
    private readonly ILogger _logger = Log.ForContext<PluginContext>();

    public PluginContext(string pluginDirectory, DirectoriesConfig directories)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginDirectory);
        ArgumentNullException.ThrowIfNull(directories);

        PluginDirectory = Path.GetFullPath(pluginDirectory);
        PluginConfigPath = Path.Combine(PluginDirectory, "plugin.toml");
        Directories = directories;
    }

    /// <summary>Absolute directory containing the plugin package.</summary>
    public string PluginDirectory { get; }

    /// <summary>Absolute path to the optional plugin runtime TOML config.</summary>
    public string PluginConfigPath { get; }

    /// <summary>Global Moongate directory configuration.</summary>
    public DirectoriesConfig Directories { get; }

    /// <summary>
    /// Loads this plugin's <c>plugin.toml</c> into a typed config. Missing files are created from defaults.
    /// </summary>
    public TConfig LoadConfig<TConfig>(Func<TConfig> defaultFactory)
        where TConfig : class, new()
    {
        ArgumentNullException.ThrowIfNull(defaultFactory);
        Directory.CreateDirectory(PluginDirectory);

        if (!File.Exists(PluginConfigPath))
        {
            var defaults = defaultFactory() ??
                           throw new InvalidOperationException(
                               $"Default factory returned null for plugin config {typeof(TConfig).FullName}."
                           );

            File.WriteAllText(
                PluginConfigPath,
                TomlSerializer.Serialize(defaults, ConfigTomlOptions.Instance)
            );
            _logger.Information("Created default plugin config at {Path}", PluginConfigPath);

            return defaults;
        }

        try
        {
            var text = File.ReadAllText(PluginConfigPath);
            var config = TomlSerializer.Deserialize<TConfig>(text, ConfigTomlOptions.Instance);

            return config ??
                   throw new InvalidOperationException(
                       $"Plugin config '{PluginConfigPath}' could not be parsed as {typeof(TConfig).FullName}."
                   );
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Plugin config '{PluginConfigPath}' could not be parsed as {typeof(TConfig).FullName}.",
                ex
            );
        }
    }
}

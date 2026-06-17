using Moongate.Abstractions.Configuration;
using Moongate.Abstractions.Data.Commands;
using Moongate.Abstractions.Interfaces.Commands;
using Moongate.Abstractions.Types.Commands;
using Moongate.Core.Data.Directories;
using Serilog;

namespace Moongate.Plugins.Data;

/// <summary>
///     Per-plugin startup context passed to <see cref="Moongate.Plugins.Interfaces.Plugins.IMoongatePlugin" />.
/// </summary>
public sealed class PluginContext
{
    public const string PluginConfigFileName = "plugin.yaml";
    private readonly ICommandRegistry? _commandRegistry;

    private readonly ILogger _logger = Log.ForContext<PluginContext>();

    public PluginContext(
        string pluginDirectory,
        DirectoriesConfig directories,
        ICommandRegistry? commandRegistry = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginDirectory);
        ArgumentNullException.ThrowIfNull(directories);

        PluginDirectory = Path.GetFullPath(pluginDirectory);
        PluginConfigPath = Path.Combine(PluginDirectory, PluginConfigFileName);
        Directories = directories;
        _commandRegistry = commandRegistry;
    }

    /// <summary>Absolute directory containing the plugin package.</summary>
    public string PluginDirectory { get; }

    /// <summary>Absolute path to the optional plugin runtime YAML config.</summary>
    public string PluginConfigPath { get; }

    /// <summary>Global Moongate directory configuration.</summary>
    public DirectoriesConfig Directories { get; }

    /// <summary>
    ///     Loads this plugin's <c>plugin.yaml</c> into a typed config. Missing files are created from defaults.
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
                ConfigYamlOptions.Serializer.Serialize(defaults)
            );
            _logger.Information("Created default plugin config at {Path}", PluginConfigPath);

            return defaults;
        }

        try
        {
            var text = File.ReadAllText(PluginConfigPath);
            var config = ConfigYamlOptions.Deserializer.Deserialize<TConfig>(text);

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

    /// <summary>
    ///     Registers a command owned by this plugin.
    /// </summary>
    /// <param name="commandName">Primary command name or aliases separated by <c>|</c>.</param>
    /// <param name="handler">Command handler.</param>
    /// <param name="description">Help description.</param>
    /// <param name="source">Allowed command sources.</param>
    /// <param name="autocompleteProvider">Optional autocomplete provider.</param>
    public void RegisterCommand(
        string commandName,
        Func<CommandSystemContext, Task> handler,
        string description = "",
        CommandSourceType source = CommandSourceType.Console,
        Func<CommandAutocompleteContext, IReadOnlyList<string>>? autocompleteProvider = null
    )
    {
        if (_commandRegistry is null)
        {
            throw new InvalidOperationException("Command registration is not available for this plugin context.");
        }

        _commandRegistry.RegisterCommand(
            commandName,
            handler,
            description,
            source,
            autocompleteProvider
        );
    }
}

using System.Reflection;
using System.Runtime.Loader;
using DryIoc;
using Moongate.Abstractions.Interfaces.Commands;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Plugins.Data;
using Moongate.Plugins.Interfaces.Plugins;
using Moongate.Plugins.Internal;
using Serilog;

namespace Moongate.Plugins.Services;

/// <summary>
/// Boot-time loader for trusted .NET plugins.
/// </summary>
public sealed class PluginLoaderService
{
    private readonly ILogger _logger = Log.ForContext<PluginLoaderService>();

    public IReadOnlyList<LoadedPlugin> LoadAndConfigure(IContainer container, DirectoriesConfig directories)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(directories);

        var pluginsDirectory = directories[DirectoryType.Plugins];
        Directory.CreateDirectory(pluginsDirectory);

        var loaded = Directory.EnumerateDirectories(pluginsDirectory)
                              .Order(StringComparer.OrdinalIgnoreCase)
                              .Select(LoadPluginDirectory)
                              .ToArray();

        var sorted = PluginDependencySorter.ValidateAndSort(loaded);

        foreach (var plugin in sorted)
        {
            var commandRegistry = container.Resolve<ICommandRegistry>(IfUnresolved.ReturnDefault);
            var context = new PluginContext(plugin.PluginDirectory, directories, commandRegistry);

            try
            {
                _logger.Information(
                    "Configuring plugin {PluginId} ({PluginName})",
                    plugin.Metadata.Id,
                    plugin.Metadata.Name
                );
                plugin.Instance.Configure(container, context);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Plugin '{plugin.Metadata.Id}' failed during Configure.",
                    ex
                );
            }
        }

        return sorted;
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            var loaderErrors = string.Join(
                Environment.NewLine,
                ex.LoaderExceptions.Select(error => error?.Message).Where(message => message is not null)
            );

            throw new InvalidOperationException(
                $"Assembly '{assembly.FullName}' contains types that could not be loaded:{Environment.NewLine}{loaderErrors}",
                ex
            );
        }
    }

    private LoadedPlugin LoadPluginDirectory(string pluginDirectory)
    {
        var dlls = Directory.EnumerateFiles(pluginDirectory, "*.dll", SearchOption.TopDirectoryOnly)
                            .Order(StringComparer.OrdinalIgnoreCase)
                            .ToArray();

        if (dlls.Length == 0)
        {
            throw new InvalidOperationException($"Plugin directory '{pluginDirectory}' does not contain a plugin assembly.");
        }

        var loadContext = new PluginAssemblyLoadContext(Path.GetFullPath(dlls[0]));
        var assemblies = new List<Assembly>(dlls.Length);

        foreach (var dll in dlls)
        {
            var assemblyName = AssemblyName.GetAssemblyName(dll);
            var shared = AssemblyLoadContext.Default.Assemblies.FirstOrDefault(
                assembly => string.Equals(
                    assembly.GetName().Name,
                    assemblyName.Name,
                    StringComparison.OrdinalIgnoreCase
                )
            );

            if (shared is not null)
            {
                assemblies.Add(shared);

                continue;
            }

            try
            {
                assemblies.Add(loadContext.LoadFromAssemblyPath(Path.GetFullPath(dll)));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Plugin assembly '{dll}' could not be loaded.",
                    ex
                );
            }
        }

        var pluginTypes = assemblies.SelectMany(GetLoadableTypes)
                                    .Where(
                                        type =>
                                            type is { IsAbstract: false, IsInterface: false } &&
                                            typeof(IMoongatePlugin).IsAssignableFrom(type)
                                    )
                                    .ToArray();

        if (pluginTypes.Length == 0)
        {
            throw new InvalidOperationException(
                $"Plugin directory '{pluginDirectory}' does not contain a plugin implementation."
            );
        }

        if (pluginTypes.Length > 1)
        {
            throw new InvalidOperationException(
                $"Plugin directory '{pluginDirectory}' contains multiple plugin implementations."
            );
        }

        try
        {
            var instance = (IMoongatePlugin?)Activator.CreateInstance(pluginTypes[0]) ??
                           throw new InvalidOperationException(
                               $"Plugin type '{pluginTypes[0].FullName}' could not be instantiated."
                           );

            return new(pluginDirectory, instance, pluginTypes[0].Assembly);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Plugin type '{pluginTypes[0].FullName}' could not be instantiated.",
                ex
            );
        }
    }
}

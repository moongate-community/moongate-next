using System.Reflection;
using System.Runtime.Loader;

namespace Moongate.Plugins.Internal;

internal sealed class PluginAssemblyLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public PluginAssemblyLoadContext(string pluginAssemblyPath)
        : base($"Moongate.Plugin:{Path.GetFileNameWithoutExtension(pluginAssemblyPath)}")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginAssemblyPath);
        _resolver = new(pluginAssemblyPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var shared = Default.Assemblies.FirstOrDefault(
            assembly => string.Equals(
                assembly.GetName().Name,
                assemblyName.Name,
                StringComparison.OrdinalIgnoreCase
            )
        );

        if (shared is not null)
        {
            return shared;
        }

        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);

        return assemblyPath is null ? null : LoadFromAssemblyPath(assemblyPath);
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);

        return libraryPath is null ? IntPtr.Zero : LoadUnmanagedDllFromPath(libraryPath);
    }
}

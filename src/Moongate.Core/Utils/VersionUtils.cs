using System.Reflection;

namespace Moongate.Core.Utils;

/// <summary>
///     Provides utility methods for reading assembly version metadata.
/// </summary>
public static class VersionUtils
{
    /// <summary>
    ///     Gets the value of an <see cref="AssemblyMetadataAttribute" /> by key from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to read metadata from.</param>
    /// <param name="key">The metadata key to look up.</param>
    /// <returns>The metadata value, or an empty string when the key is absent.</returns>
    public static string GetMetadata(Assembly assembly, string key)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        return assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                   .FirstOrDefault(a => string.Equals(a.Key, key, StringComparison.Ordinal))
                   ?.Value ??
               "";
    }

    /// <summary>
    ///     Gets the informational version for the Moongate.Core assembly.
    /// </summary>
    /// <returns>The package version declared for Moongate.Core.</returns>
    public static string GetVersion()
    {
        return GetVersion(typeof(VersionUtils).Assembly);
    }

    /// <summary>
    ///     Gets the informational version for the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to read version metadata from.</param>
    /// <returns>The assembly informational version, or the assembly version when informational metadata is unavailable.</returns>
    public static string GetVersion(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informationalVersion))
        {
            var metadataIndex = informationalVersion.IndexOf('+', StringComparison.Ordinal);

            return metadataIndex == -1 ? informationalVersion : informationalVersion[..metadataIndex];
        }

        return assembly.GetName().Version?.ToString() ?? "";
    }
}

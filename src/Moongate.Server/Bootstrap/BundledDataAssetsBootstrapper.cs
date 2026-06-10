using System.Reflection;
using Moongate.Core.Utils;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Bootstrap;

/// <summary>
/// Seeds the runtime data directory with bundled embedded data assets.
/// </summary>
public static class BundledDataAssetsBootstrapper
{
    public const string DataResourcePrefix = "Moongate.Server.Assets.data/";
    public const string TemplatesResourcePrefix = "Moongate.Server.Assets.templates/";

    public static int EnsureDataAssets(string destinationDirectory, ILogger logger)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationDirectory);
        ArgumentNullException.ThrowIfNull(logger);

        return EnsureDataAssets(
            typeof(BundledDataAssetsBootstrapper).Assembly,
            DataResourcePrefix,
            destinationDirectory,
            logger
        );
    }

    public static int EnsureTemplateAssets(string destinationDirectory, ILogger logger)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationDirectory);
        ArgumentNullException.ThrowIfNull(logger);

        return EnsureDataAssets(
            typeof(BundledDataAssetsBootstrapper).Assembly,
            TemplatesResourcePrefix,
            destinationDirectory,
            logger
        );
    }

    public static int EnsureDataAssets(
        Assembly assembly,
        string resourcePrefix,
        string destinationDirectory,
        ILogger logger
    )
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourcePrefix);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationDirectory);
        ArgumentNullException.ThrowIfNull(logger);

        Directory.CreateDirectory(destinationDirectory);

        var normalizedPrefix = NormalizeResourcePrefix(resourcePrefix);
        var copied = 0;

        foreach (var resourceName in GetDataResourceNames(assembly, normalizedPrefix))
        {
            var destinationFile = Path.Combine(
                destinationDirectory,
                GetRelativePath(resourceName, normalizedPrefix)
            );
            var destinationFileDirectory = Path.GetDirectoryName(destinationFile);

            if (!string.IsNullOrWhiteSpace(destinationFileDirectory))
            {
                Directory.CreateDirectory(destinationFileDirectory);
            }

            if (File.Exists(destinationFile))
            {
                continue;
            }

            using var source = ResourceUtils.GetEmbeddedResourceStream(assembly, resourceName);
            using var destination = File.Create(destinationFile);
            source.CopyTo(destination);
            copied++;
        }

        logger.Information(
            "Bundled data assets synchronized: copied {Copied} missing files into {Destination}",
            copied,
            destinationDirectory
        );

        return copied;
    }

    private static IReadOnlyList<string> GetDataResourceNames(Assembly assembly, string normalizedPrefix)
        => assembly.GetManifestResourceNames()
                   .Where(name => name.StartsWith(normalizedPrefix, StringComparison.Ordinal))
                   .Order(StringComparer.Ordinal)
                   .ToArray();

    private static string GetRelativePath(string resourceName, string normalizedPrefix)
    {
        var relativePath = resourceName[normalizedPrefix.Length..].Replace('\\', '/');
        var segments = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0 || segments.Any(segment => segment is "." or ".."))
        {
            throw new InvalidOperationException($"Invalid embedded data asset path: {resourceName}");
        }

        return Path.Combine(segments);
    }

    private static string NormalizeResourcePrefix(string resourcePrefix)
    {
        var normalizedPrefix = resourcePrefix.Trim().Replace('\\', '/');

        return normalizedPrefix.EndsWith('/')
                   ? normalizedPrefix
                   : normalizedPrefix + "/";
    }
}

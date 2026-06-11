using System.Reflection;
using Moongate.Core.Utils;
using Moongate.Plugin.Email.Data;
using Serilog;

namespace Moongate.Plugin.Email.Services;

/// <summary>Copies bundled email templates into the plugin template directory.</summary>
internal static class EmailTemplateAssetsBootstrapper
{
    public const string TemplateResourcePrefix = "Moongate.Plugin.Email.Assets.templates/";

    public static int EnsureDefaultTemplates(
        EmailPluginConfig config,
        EmailPluginRuntimePaths paths,
        ILogger logger
    )
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(logger);

        return EnsureDefaultTemplates(
            typeof(EmailTemplateAssetsBootstrapper).Assembly,
            TemplateResourcePrefix,
            ResolveTemplatesRoot(config, paths),
            logger
        );
    }

    internal static int EnsureDefaultTemplates(
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

        foreach (var resourceName in GetTemplateResourceNames(assembly, normalizedPrefix))
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
            "Email templates synchronized: copied {Copied} missing files into {Destination}",
            copied,
            destinationDirectory
        );

        return copied;
    }

    internal static string ResolveTemplatesRoot(EmailPluginConfig config, EmailPluginRuntimePaths paths)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(paths);

        return Path.IsPathRooted(config.Templates.Directory)
                   ? config.Templates.Directory
                   : Path.Combine(paths.PluginDirectory, config.Templates.Directory);
    }

    private static IReadOnlyList<string> GetTemplateResourceNames(Assembly assembly, string normalizedPrefix)
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
            throw new InvalidOperationException($"Invalid embedded email template path: {resourceName}");
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

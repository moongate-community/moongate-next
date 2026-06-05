using ILogger = Serilog.ILogger;

namespace Moongate.Server.Bootstrap;

/// <summary>
/// Seeds the runtime data directory with bundled UO data files, copying only the files that are
/// missing at the destination. Existing files are never overwritten so operators can edit them.
/// </summary>
public static class UoDataAssetsBootstrapper
{
    public static int EnsureDataAssets(string sourceDirectory, string destinationDirectory, ILogger logger)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationDirectory);
        ArgumentNullException.ThrowIfNull(logger);

        if (!Directory.Exists(sourceDirectory))
        {
            logger.Warning("Bundled UO data directory not found: {SourceDirectory}", sourceDirectory);

            return 0;
        }

        Directory.CreateDirectory(destinationDirectory);

        var copied = 0;

        foreach (var sourceFile in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDirectory, sourceFile);
            var destinationFile = Path.Combine(destinationDirectory, relativePath);
            var destinationFileDirectory = Path.GetDirectoryName(destinationFile);

            if (!string.IsNullOrWhiteSpace(destinationFileDirectory))
            {
                Directory.CreateDirectory(destinationFileDirectory);
            }

            if (File.Exists(destinationFile))
            {
                continue;
            }

            File.Copy(sourceFile, destinationFile);
            copied++;
        }

        logger.Information(
            "UO data assets synchronized: copied {Copied} missing files into {Destination}",
            copied,
            destinationDirectory
        );

        return copied;
    }
}

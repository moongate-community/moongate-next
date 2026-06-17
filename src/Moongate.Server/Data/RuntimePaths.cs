using Moongate.Core.Data.Directories;
using Moongate.Core.Types;

namespace Moongate.Server.Data;

internal static class RuntimePaths
{
    public const string RootEnvironmentVariable = "MOONGATE_ROOT";
    public const string LegacyRootEnvironmentVariable = "NIGHTHEAVEN_ROOT";
    public const string DefaultRootDirectoryName = "moongate";
    public const string ConfigFileName = "moongate.yaml";
    public const string LegacyConfigFileName = "nightheaven.yaml";

    public static string ResolveConfigPath(DirectoriesConfig directories)
    {
        ArgumentNullException.ThrowIfNull(directories);

        var configDirectory = directories[DirectoryType.Config];
        var configPath = Path.Combine(configDirectory, ConfigFileName);
        var legacyConfigPath = Path.Combine(configDirectory, LegacyConfigFileName);

        return File.Exists(legacyConfigPath) && !File.Exists(configPath)
            ? legacyConfigPath
            : configPath;
    }

    public static string ResolveRootDirectory(string? commandLineRootDirectory)
    {
        // Normalized so relative roots (e.g. MOONGATE_ROOT=../../moongate_data
        // from launchSettings) resolve against the working directory once and
        // logs always show an absolute path.
        if (!string.IsNullOrWhiteSpace(commandLineRootDirectory))
        {
            return Path.GetFullPath(commandLineRootDirectory);
        }

        var primaryRoot = Environment.GetEnvironmentVariable(RootEnvironmentVariable);

        if (!string.IsNullOrWhiteSpace(primaryRoot))
        {
            return Path.GetFullPath(primaryRoot);
        }

        var legacyRoot = Environment.GetEnvironmentVariable(LegacyRootEnvironmentVariable);

        if (!string.IsNullOrWhiteSpace(legacyRoot))
        {
            return Path.GetFullPath(legacyRoot);
        }

        return Path.Combine(Directory.GetCurrentDirectory(), DefaultRootDirectoryName);
    }
}

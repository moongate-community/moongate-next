using Moongate.Core.Data.Directories;
using Moongate.Core.Types;

namespace Moongate.Server.Data;

internal static class RuntimePaths
{
    public const string RootEnvironmentVariable = "MOONGATE_ROOT";
    public const string LegacyRootEnvironmentVariable = "NIGHTHEAVEN_ROOT";
    public const string DefaultRootDirectoryName = "moongate";
    public const string ConfigFileName = "moongate.toml";
    public const string LegacyConfigFileName = "nightheaven.toml";

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
        if (!string.IsNullOrWhiteSpace(commandLineRootDirectory))
        {
            return commandLineRootDirectory;
        }

        var primaryRoot = Environment.GetEnvironmentVariable(RootEnvironmentVariable);

        if (!string.IsNullOrWhiteSpace(primaryRoot))
        {
            return primaryRoot;
        }

        var legacyRoot = Environment.GetEnvironmentVariable(LegacyRootEnvironmentVariable);

        if (!string.IsNullOrWhiteSpace(legacyRoot))
        {
            return legacyRoot;
        }

        return Path.Combine(Directory.GetCurrentDirectory(), DefaultRootDirectoryName);
    }
}

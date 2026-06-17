using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.Data;

namespace Moongate.Tests.Server;

public sealed class RuntimePathsTests : IDisposable
{
    private readonly string? _oldLegacy = Environment.GetEnvironmentVariable("NIGHTHEAVEN_ROOT");
    private readonly string? _oldPrimary = Environment.GetEnvironmentVariable("MOONGATE_ROOT");
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"nr-runtime-paths-{Guid.NewGuid():N}");

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("MOONGATE_ROOT", _oldPrimary);
        Environment.SetEnvironmentVariable("NIGHTHEAVEN_ROOT", _oldLegacy);

        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void ResolveConfigPath_NewConfigWinsOverLegacyConfig()
    {
        var directories = Directories();
        Directory.CreateDirectory(directories[DirectoryType.Config]);
        File.WriteAllText(Path.Combine(directories[DirectoryType.Config], "nightheaven.yaml"), "");
        File.WriteAllText(Path.Combine(directories[DirectoryType.Config], "moongate.yaml"), "");

        var configPath = RuntimePaths.ResolveConfigPath(directories);

        Assert.Equal(Path.Combine(directories[DirectoryType.Config], "moongate.yaml"), configPath);
    }

    [Fact]
    public void ResolveConfigPath_UsesLegacyConfigOnlyWhenNewConfigIsMissing()
    {
        var directories = Directories();
        Directory.CreateDirectory(directories[DirectoryType.Config]);
        File.WriteAllText(Path.Combine(directories[DirectoryType.Config], "nightheaven.yaml"), "");

        var configPath = RuntimePaths.ResolveConfigPath(directories);

        Assert.Equal(Path.Combine(directories[DirectoryType.Config], "nightheaven.yaml"), configPath);
    }

    [Fact]
    public void ResolveConfigPath_UsesNewConfigNameByDefault()
    {
        var directories = Directories();

        var configPath = RuntimePaths.ResolveConfigPath(directories);

        Assert.Equal(Path.Combine(directories[DirectoryType.Config], "moongate.yaml"), configPath);
    }

    [Fact]
    public void ResolveRootDirectory_CommandLineRoot_WinsOverEnvironment()
    {
        Environment.SetEnvironmentVariable("MOONGATE_ROOT", Path.Combine(_root, "env"));
        Environment.SetEnvironmentVariable("NIGHTHEAVEN_ROOT", Path.Combine(_root, "legacy"));

        var root = RuntimePaths.ResolveRootDirectory(Path.Combine(_root, "cli"));

        Assert.Equal(Path.Combine(_root, "cli"), root);
    }

    [Fact]
    public void ResolveRootDirectory_LegacyEnvironment_RemainsFallback()
    {
        Environment.SetEnvironmentVariable("MOONGATE_ROOT", null);
        Environment.SetEnvironmentVariable("NIGHTHEAVEN_ROOT", Path.Combine(_root, "old"));

        var root = RuntimePaths.ResolveRootDirectory(null);

        Assert.Equal(Path.Combine(_root, "old"), root);
    }

    [Fact]
    public void ResolveRootDirectory_PrimaryEnvironment_WinsOverLegacy()
    {
        Environment.SetEnvironmentVariable("MOONGATE_ROOT", Path.Combine(_root, "new"));
        Environment.SetEnvironmentVariable("NIGHTHEAVEN_ROOT", Path.Combine(_root, "old"));

        var root = RuntimePaths.ResolveRootDirectory(null);

        Assert.Equal(Path.Combine(_root, "new"), root);
    }

    private DirectoriesConfig Directories()
    {
        return new DirectoriesConfig(_root, Enum.GetNames<DirectoryType>());
    }
}

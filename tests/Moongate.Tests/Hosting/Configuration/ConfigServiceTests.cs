using Moongate.Abstractions.Data.Internal;
using Moongate.Tests.Hosting.Configuration.Support;
using ConfigService = Moongate.Abstractions.Configuration.ConfigService;

namespace Moongate.Tests.Hosting.Configuration;

public class ConfigServiceTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nh-config-{Guid.NewGuid():N}");
    private string Path_ => Path.Combine(_dir, "moongate.toml");

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Load_Default_LeavesNoTempFile()
    {
        ConfigService.Load(Path_, [ServerSection()]);

        Assert.False(File.Exists(Path_ + ".tmp"));
    }

    [Fact]
    public void Load_ExistingFile_BindsValues()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(Path_, "[server]\nport = 7000\nname = \"shard\"\nheartbeat = \"00:01:00\"\n");

        var results = ConfigService.Load(Path_, [ServerSection()]);

        var settings = Assert.IsType<TestServerSettings>(results[0].Instance);
        Assert.Equal(7000, settings.Port);
        Assert.Equal("shard", settings.Name);
        Assert.Equal(TimeSpan.FromMinutes(1), settings.Heartbeat);
    }

    [Fact]
    public void Load_InvalidValue_Throws()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(Path_, "[limits]\nmax_players = 0\n");

        var ex = Assert.ThrowsAny<Exception>(() => ConfigService.Load(Path_, [ValidatableSection()]));
        Assert.Contains("MaxPlayers", ex.Message);
    }

    [Fact]
    public void Load_MalformedToml_Throws()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(Path_, "[server]\nport = = =\n");

        Assert.ThrowsAny<Exception>(() => ConfigService.Load(Path_, [ServerSection()]));
    }

    [Fact]
    public void Load_MissingFile_CreatesDefaultFileAndReturnsDefaults()
    {
        var results = ConfigService.Load(Path_, [ServerSection()]);

        Assert.True(File.Exists(Path_));
        var settings = Assert.IsType<TestServerSettings>(Assert.Single(results).Instance);
        Assert.Equal(2593, settings.Port);
        Assert.Contains("[server]", File.ReadAllText(Path_));
    }

    [Fact]
    public void Load_MissingSectionInExistingFile_DefaultsAndWritesItBack()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(Path_, "[server]\nport = 7000\n");

        var results = ConfigService.Load(Path_, [ServerSection(), ValidatableSection()]);

        Assert.Equal(2, results.Count);
        Assert.Contains("[limits]", File.ReadAllText(Path_));
    }

    [Fact]
    public void Load_UnknownSection_IsIgnored()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(Path_, "[server]\nport = 7000\n\n[ghost]\nfoo = 1\n");

        var results = ConfigService.Load(Path_, [ServerSection()]);

        Assert.Single(results);
        Assert.Equal(7000, ((TestServerSettings)results[0].Instance).Port);
    }

    private static ConfigSectionRegistration ServerSection()
        => new("server", typeof(TestServerSettings), () => new TestServerSettings());

    private static ConfigSectionRegistration ValidatableSection()
        => new("limits", typeof(ValidatableSettings), () => new ValidatableSettings());
}

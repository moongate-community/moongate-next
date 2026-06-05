using Moongate.Core.Types;
using Moongate.Plugins.Data;

namespace Moongate.Tests.Plugins;

public sealed class PluginContextTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        $"nh-plugin-context-{Guid.NewGuid():N}"
    );

    private string PluginDirectory => Path.Combine(_root, "plugins", "moongate.test");

    private sealed class WeatherPluginConfig
    {
        public int WeatherIntervalSeconds { get; set; } = 2;
        public string Region { get; set; } = "Britannia";
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void LoadConfig_ExistingFile_BindsValues()
    {
        Directory.CreateDirectory(PluginDirectory);
        File.WriteAllText(
            Path.Combine(PluginDirectory, "plugin.toml"),
            "weather_interval_seconds = 7\nregion = \"Trammel\"\n"
        );
        var context = CreateContext();

        var config = context.LoadConfig(() => new WeatherPluginConfig());

        Assert.Equal(7, config.WeatherIntervalSeconds);
        Assert.Equal("Trammel", config.Region);
    }

    [Fact]
    public void LoadConfig_MalformedFile_Throws()
    {
        Directory.CreateDirectory(PluginDirectory);
        File.WriteAllText(Path.Combine(PluginDirectory, "plugin.toml"), "weather_interval_seconds = = =\n");
        var context = CreateContext();

        var ex = Assert.Throws<InvalidOperationException>(() => context.LoadConfig(() => new WeatherPluginConfig()));
        Assert.Contains("plugin.toml", ex.Message);
    }

    [Fact]
    public void LoadConfig_MissingFile_WritesDefaultsAndReturnsDefaults()
    {
        var context = CreateContext();

        var config = context.LoadConfig(() => new WeatherPluginConfig());

        Assert.Equal(2, config.WeatherIntervalSeconds);
        Assert.True(File.Exists(context.PluginConfigPath));
        Assert.Contains("weather_interval_seconds = 2", File.ReadAllText(context.PluginConfigPath));
    }

    private PluginContext CreateContext()
        => new(PluginDirectory, new(_root, Enum.GetNames<DirectoryType>()));
}

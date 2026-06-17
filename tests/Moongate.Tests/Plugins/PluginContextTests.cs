using Moongate.Abstractions.Services.Commands;
using Moongate.Abstractions.Types.Commands;
using Moongate.Core.Data.Directories;
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
            Path.Combine(PluginDirectory, "plugin.yaml"),
            "weather_interval_seconds: 7\nregion: Trammel\n"
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
        File.WriteAllText(Path.Combine(PluginDirectory, "plugin.yaml"), "weather_interval_seconds: [\n");
        var context = CreateContext();

        var ex = Assert.Throws<InvalidOperationException>(() => context.LoadConfig(() => new WeatherPluginConfig()));
        Assert.Contains("plugin.yaml", ex.Message);
    }

    [Fact]
    public void LoadConfig_MissingFile_WritesDefaultsAndReturnsDefaults()
    {
        var context = CreateContext();

        var config = context.LoadConfig(() => new WeatherPluginConfig());

        Assert.Equal(2, config.WeatherIntervalSeconds);
        Assert.True(File.Exists(context.PluginConfigPath));
        Assert.Contains("weather_interval_seconds: 2", File.ReadAllText(context.PluginConfigPath));
    }

    [Fact]
    public void RegisterCommand_WithCommandRegistry_RegistersPluginCommand()
    {
        var registry = new CommandRegistry();
        var context = CreateContext(registry);

        context.RegisterCommand(
            "weather",
            static _ => Task.CompletedTask,
            "Controls weather.",
            CommandSourceType.All
        );

        Assert.True(registry.TryGetCommand("weather", out var command));
        Assert.Equal(CommandSourceType.All, command.Source);
    }

    [Fact]
    public void RegisterCommand_WithoutCommandRegistry_Throws()
    {
        var context = CreateContext();

        Assert.Throws<InvalidOperationException>(() => context.RegisterCommand("weather", static _ => Task.CompletedTask));
    }

    private PluginContext CreateContext(CommandRegistry? registry = null)
    {
        return new PluginContext(PluginDirectory, new DirectoriesConfig(_root, Enum.GetNames<DirectoryType>()), registry);
    }

    private sealed class WeatherPluginConfig
    {
        public int WeatherIntervalSeconds { get; set; } = 2;
        public string Region { get; set; } = "Britannia";
    }
}

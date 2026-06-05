using DryIoc;
using Moongate.Abstractions.Data.Logging;
using Moongate.Core.Types;
using Moongate.Server.Extensions.Configuration;
using Moongate.Server.Extensions.Logging;

namespace Moongate.Tests.Hosting.Logging;

public sealed class LoggerConfigTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nr-logger-config-{Guid.NewGuid():N}");
    private string ConfigPath => Path.Combine(_dir, "moongate.toml");

    [Fact]
    public void AddMoongateLogging_RegistersLoggerConfigSection()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(
            ConfigPath,
            "[logger]\nlevel = \"Debug\"\nlog_packets = true\nwrite_to_file = true\nfile_name = \"server.log\"\n"
        );

        var container = new Container();
        container.AddMoongateLogging();
        container.AddMoongateConfig(ConfigPath);

        var config = container.Resolve<LoggerConfig>();
        Assert.Equal(LogLevelType.Debug, config.Level);
        Assert.True(config.LogPackets);
        Assert.True(config.WriteToFile);
        Assert.Equal("server.log", config.FileName);
    }

    [Fact]
    public void Defaults_MatchServerStartupLogging()
    {
        var config = new LoggerConfig();

        Assert.Equal(LogLevelType.Information, config.Level);
        Assert.False(config.LogPackets);
        Assert.False(config.WriteToFile);
        Assert.Equal("moongate.log", config.FileName);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }
}

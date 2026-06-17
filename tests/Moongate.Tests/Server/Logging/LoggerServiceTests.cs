using Moongate.Abstractions.Data.Logging;
using Moongate.Core.Types;
using Moongate.Server.Services.Logging;
using Serilog;
using Serilog.Events;

namespace Moongate.Tests.Server.Logging;

public sealed class LoggerServiceTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nr-logger-{Guid.NewGuid():N}");

    public void Dispose()
    {
        Log.CloseAndFlush();

        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void CreateLogger_HonorsMinimumLevel()
    {
        using var logger = LoggerService.CreateLogger(
            new LoggerConfig
            {
                Level = LogLevelType.Warning,
                WriteToFile = true,
                FileName = "minimum.log"
            },
            _dir
        );

        var path = Path.Combine(_dir, "minimum.log");
        logger.Write(LogEventLevel.Information, "ignored");
        logger.Write(LogEventLevel.Warning, "written");
        logger.Dispose();

        var content = File.ReadAllText(path);
        Assert.DoesNotContain("ignored", content);
        Assert.Contains("written", content);
    }

    [Fact]
    public void CreateLogger_WhenFileEnabled_WritesToConfiguredFile()
    {
        using var logger = LoggerService.CreateLogger(
            new LoggerConfig
            {
                Level = LogLevelType.Information,
                WriteToFile = true,
                FileName = "server.log"
            },
            _dir
        );

        logger.Information("file sink works");
        logger.Dispose();

        Assert.Contains("file sink works", File.ReadAllText(Path.Combine(_dir, "server.log")));
    }
}

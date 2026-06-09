using Moongate.Core.Types;
using Moongate.Server.Services.LiveConsole;
using Moongate.Server.Services.Logging;
using Moongate.Server.Types.LiveConsole;

namespace Moongate.Tests.Server.LiveConsole;

public class LiveConsoleLoggerWiringTests
{
    [Fact]
    public void CreateLogger_WithBroadcaster_StreamsLogEntriesToIt()
    {
        var broadcaster = new LiveConsoleBroadcaster();
        var logsDir = Path.Combine(Path.GetTempPath(), $"lc-logtest-{Guid.NewGuid():N}");

        using var logger = LoggerService.CreateLogger(
            new()
            {
                Level = LogLevelType.Information,
                WriteToFile = false
            },
            logsDir,
            broadcaster
        );
        logger.Information("hello console");

        var entry = Assert.Single(broadcaster.GetBacklog());
        Assert.Equal(LiveConsoleEntryKind.Log, entry.Kind);
        Assert.Equal("hello console", entry.Message);
    }
}

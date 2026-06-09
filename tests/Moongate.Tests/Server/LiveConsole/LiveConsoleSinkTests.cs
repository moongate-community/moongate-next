using Moongate.Server.Services.Logging;
using Moongate.Server.Services.LiveConsole;
using Moongate.Server.Types.LiveConsole;
using Serilog.Events;
using Serilog.Parsing;

namespace Moongate.Tests.Server.LiveConsole;

public class LiveConsoleSinkTests
{
    private static LogEvent MakeLogEvent(LogEventLevel level, string message, string? sourceContext = null)
    {
        var properties = new List<LogEventProperty>();

        if (sourceContext is not null)
        {
            properties.Add(new LogEventProperty("SourceContext", new ScalarValue(sourceContext)));
        }

        return new LogEvent(
            DateTimeOffset.UnixEpoch,
            level,
            exception: null,
            new MessageTemplateParser().Parse(message),
            properties
        );
    }

    [Fact]
    public void Emit_LogEvent_PublishesLogEntry()
    {
        var broadcaster = new LiveConsoleBroadcaster();
        var sink = new LiveConsoleSink(broadcaster);

        sink.Emit(MakeLogEvent(LogEventLevel.Warning, "disk almost full"));

        var entry = Assert.Single(broadcaster.GetBacklog());
        Assert.Equal(LiveConsoleEntryKind.Log, entry.Kind);
        Assert.Equal("Warning", entry.Level);
        Assert.Equal("disk almost full", entry.Message);
        Assert.Equal(0, entry.Timestamp);
    }

    [Fact]
    public void Emit_SignalRSourceContext_IsSkipped()
    {
        var broadcaster = new LiveConsoleBroadcaster();
        var sink = new LiveConsoleSink(broadcaster);

        sink.Emit(MakeLogEvent(LogEventLevel.Information, "hub noise", "Microsoft.AspNetCore.SignalR.HubConnectionHandler"));

        Assert.Empty(broadcaster.GetBacklog());
    }
}

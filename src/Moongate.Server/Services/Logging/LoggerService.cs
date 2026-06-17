using Moongate.Abstractions.Data.Logging;
using Moongate.Core.Extensions.Logger;
using Moongate.Server.Interfaces.LiveConsole;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Moongate.Server.Services.Logging;

/// <summary>
///     Builds the server logger from YAML configuration.
/// </summary>
public static class LoggerService
{
    /// <summary>
    ///     Creates a Serilog logger using the configured level and sinks.
    /// </summary>
    public static Logger CreateLogger(LoggerConfig config, string logsDirectory, ILiveConsoleBroadcaster? liveConsole = null)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentException.ThrowIfNullOrWhiteSpace(logsDirectory);

        var builder = new LoggerConfiguration()
            .MinimumLevel
            .Is(config.Level.ToSerilogLogLevel())
            .MinimumLevel
            .Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .WriteTo
            .Console();

        if (liveConsole is not null)
        {
            builder.WriteTo.Sink(new LiveConsoleSink(liveConsole), LogEventLevel.Information);
        }

        if (config.WriteToFile)
        {
            Directory.CreateDirectory(logsDirectory);
            builder.WriteTo.File(Path.Combine(logsDirectory, config.FileName));
        }

        return builder.CreateLogger();
    }
}

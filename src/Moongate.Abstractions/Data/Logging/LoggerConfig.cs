using Moongate.Core.Types;

namespace Moongate.Abstractions.Data.Logging;

/// <summary>
///     Configuration for server logging.
/// </summary>
public sealed class LoggerConfig
{
    /// <summary>
    ///     Minimum event level written by the configured logger.
    /// </summary>
    public LogLevelType Level { get; set; } = LogLevelType.Information;

    /// <summary>
    ///     When <c>true</c>, logs parsed network packets.
    /// </summary>
    public bool LogPackets { get; set; }

    /// <summary>
    ///     When <c>true</c>, also writes log events to a file under the logs directory.
    /// </summary>
    public bool WriteToFile { get; set; }

    /// <summary>
    ///     File name used when <see cref="WriteToFile" /> is enabled.
    /// </summary>
    public string FileName { get; set; } = "moongate.log";
}

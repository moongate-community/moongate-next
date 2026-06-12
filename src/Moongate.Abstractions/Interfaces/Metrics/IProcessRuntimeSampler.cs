using Moongate.Abstractions.Data.Metrics;

namespace Moongate.Abstractions.Interfaces.Metrics;

/// <summary>
/// Captures raw process runtime readings (CPU time, memory, GC counters) for the runtime metric provider.
/// </summary>
public interface IProcessRuntimeSampler
{
    /// <summary>Logical processor count used to normalize CPU percentage. Always &gt;= 1.</summary>
    int ProcessorCount { get; }

    /// <summary>Captures a point-in-time runtime reading. Must be cheap and thread-safe.</summary>
    ProcessRuntimeReading Read();
}

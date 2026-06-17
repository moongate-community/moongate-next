namespace Moongate.Abstractions.Data.Metrics;

/// <summary>
///     One raw runtime reading captured by <see cref="Interfaces.Metrics.IProcessRuntimeSampler" />.
///     CPU percentage is derived from the delta between two readings, so the timestamp is part of the reading.
/// </summary>
public sealed record ProcessRuntimeReading
{
    /// <summary>Total CPU time consumed by the process up to <see cref="Timestamp" />.</summary>
    public TimeSpan TotalProcessorTime { get; init; }

    /// <summary>Process working set (physical memory) in bytes.</summary>
    public long WorkingSetBytes { get; init; }

    /// <summary>Managed heap size in bytes (<see cref="GC.GetTotalMemory(bool)" />).</summary>
    public long ManagedHeapBytes { get; init; }

    /// <summary>GC heap size in bytes (<see cref="GCMemoryInfo.HeapSizeBytes" />).</summary>
    public long GcHeapSizeBytes { get; init; }

    /// <summary>Total bytes allocated since process start.</summary>
    public long AllocatedBytesTotal { get; init; }

    /// <summary>Cumulative gen0 GC collections.</summary>
    public int Gen0Collections { get; init; }

    /// <summary>Cumulative gen1 GC collections.</summary>
    public int Gen1Collections { get; init; }

    /// <summary>Cumulative gen2 GC collections.</summary>
    public int Gen2Collections { get; init; }

    /// <summary>When the reading was captured.</summary>
    public DateTimeOffset Timestamp { get; init; }
}

using System.Diagnostics;
using Moongate.Abstractions.Data.Metrics;
using Moongate.Abstractions.Interfaces.Metrics;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.Metrics;

/// <summary>
/// Default <see cref="IProcessRuntimeSampler" /> reading from <see cref="Process" />, <see cref="GC" /> and
/// <see cref="Environment" />.
/// A read failure yields an empty (zeroed) reading so the provider keeps emitting valid samples.
/// </summary>
public sealed class ProcessRuntimeSampler : IProcessRuntimeSampler
{
    private readonly ILogger _logger = Log.ForContext<ProcessRuntimeSampler>();

    public int ProcessorCount => Math.Max(1, Environment.ProcessorCount);

    public ProcessRuntimeReading Read()
    {
        var timestamp = DateTimeOffset.UtcNow;

        try
        {
            using var process = Process.GetCurrentProcess();
            var gcInfo = GC.GetGCMemoryInfo();

            return new()
            {
                TotalProcessorTime = process.TotalProcessorTime,
                WorkingSetBytes = process.WorkingSet64,
                ManagedHeapBytes = GC.GetTotalMemory(false),
                GcHeapSizeBytes = gcInfo.HeapSizeBytes,
                AllocatedBytesTotal = GC.GetTotalAllocatedBytes(),
                Gen0Collections = GC.CollectionCount(0),
                Gen1Collections = GC.CollectionCount(1),
                Gen2Collections = GC.CollectionCount(2),
                Timestamp = timestamp
            };
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "ProcessRuntimeSampler failed to read; returning empty reading");

            return new() { Timestamp = timestamp };
        }
    }
}

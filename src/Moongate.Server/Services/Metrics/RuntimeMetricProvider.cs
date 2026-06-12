using System.Threading;
using Moongate.Abstractions.Data.Metrics;
using Moongate.Abstractions.Interfaces.Metrics;
using Moongate.Abstractions.Types.Metrics;

namespace Moongate.Server.Services.Metrics;

/// <summary>
/// Exposes process-level runtime metrics (CPU%, memory, GC) as an <see cref="IMetricProvider" />.
/// CPU% is derived from the delta between consecutive readings, so it is 0 until a second Collect.
/// </summary>
public sealed class RuntimeMetricProvider : IMetricProvider
{
    private readonly IProcessRuntimeSampler _sampler;
    private readonly Lock _sync = new();

    private ProcessRuntimeReading? _previous;

    public string Prefix => "runtime";

    public RuntimeMetricProvider(IProcessRuntimeSampler sampler)
    {
        _sampler = sampler;
    }

    public IReadOnlyList<MetricSample> Collect()
    {
        var current = _sampler.Read();
        var cores = _sampler.ProcessorCount;
        double cpuPercent;

        lock (_sync)
        {
            cpuPercent = ComputeCpuPercent(_previous, current, cores);
            _previous = current;
        }

        return
        [
            new("cpu_percent", cpuPercent, Help: "Process CPU usage percent (0-100, normalized over cores)"),
            new("memory_working_set_bytes", current.WorkingSetBytes, Help: "Process working set in bytes"),
            new("memory_managed_heap_bytes", current.ManagedHeapBytes, Help: "Managed heap in bytes"),
            new("gc_gen0_collections", current.Gen0Collections, MetricType.Counter, Help: "Gen0 GC collections"),
            new("gc_gen1_collections", current.Gen1Collections, MetricType.Counter, Help: "Gen1 GC collections"),
            new("gc_gen2_collections", current.Gen2Collections, MetricType.Counter, Help: "Gen2 GC collections"),
            new("gc_heap_size_bytes", current.GcHeapSizeBytes, Help: "GC heap size in bytes"),
            new("gc_allocated_bytes", current.AllocatedBytesTotal, MetricType.Counter, Help: "Total bytes allocated since start")
        ];
    }

    private static double ComputeCpuPercent(ProcessRuntimeReading? previous, ProcessRuntimeReading current, int processorCount)
    {
        if (previous is null)
        {
            return 0;
        }

        var wallMs = (current.Timestamp - previous.Timestamp).TotalMilliseconds;

        if (wallMs <= 0)
        {
            return 0;
        }

        var cores = processorCount < 1 ? 1 : processorCount;
        var cpuMs = (current.TotalProcessorTime - previous.TotalProcessorTime).TotalMilliseconds;
        var percent = cpuMs / (wallMs * cores) * 100;

        return Math.Clamp(percent, 0, 100);
    }
}

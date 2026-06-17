using Moongate.Abstractions.Data.Metrics;
using Moongate.Abstractions.Interfaces.Metrics;
using Moongate.Abstractions.Types.Metrics;
using Moongate.Server.Services.Metrics;

namespace Moongate.Tests.Hosting.Metrics;

public class RuntimeMetricProviderTests
{
    [Fact]
    public void Collect_FirstCall_ReportsZeroCpuAndEightSamples()
    {
        var sampler = new FakeSampler { ProcessorCount = 2, Reading = Reading(1000, 0) };
        var provider = new RuntimeMetricProvider(sampler);

        var samples = provider.Collect();

        Assert.Equal("runtime", provider.Prefix);
        Assert.Equal(8, samples.Count);
        Assert.Equal(0, ValueOf(samples, "cpu_percent"));
    }

    [Fact]
    public void Collect_HugeCpuDelta_ClampsTo100()
    {
        var sampler = new FakeSampler { ProcessorCount = 1 };
        var provider = new RuntimeMetricProvider(sampler);

        sampler.Reading = Reading(0, 0);
        provider.Collect();
        sampler.Reading = Reading(100_000, 1000);
        var samples = provider.Collect();

        Assert.Equal(100, ValueOf(samples, "cpu_percent"));
    }

    [Fact]
    public void Collect_MemoryAndGcSamples_HaveExpectedValuesAndTypes()
    {
        var sampler = new FakeSampler
        {
            ProcessorCount = 1,
            Reading = new ProcessRuntimeReading
            {
                TotalProcessorTime = TimeSpan.Zero,
                WorkingSetBytes = 1_000,
                ManagedHeapBytes = 2_000,
                GcHeapSizeBytes = 3_000,
                AllocatedBytesTotal = 4_000,
                Gen0Collections = 5,
                Gen1Collections = 6,
                Gen2Collections = 7,
                Timestamp = DateTimeOffset.UnixEpoch
            }
        };
        var provider = new RuntimeMetricProvider(sampler);

        var samples = provider.Collect();

        Assert.Equal(1_000, ValueOf(samples, "memory_working_set_bytes"));
        Assert.Equal(2_000, ValueOf(samples, "memory_managed_heap_bytes"));
        Assert.Equal(3_000, ValueOf(samples, "gc_heap_size_bytes"));
        Assert.Equal(4_000, ValueOf(samples, "gc_allocated_bytes"));
        Assert.Equal(5, ValueOf(samples, "gc_gen0_collections"));
        Assert.Equal(6, ValueOf(samples, "gc_gen1_collections"));
        Assert.Equal(7, ValueOf(samples, "gc_gen2_collections"));
        Assert.Equal(MetricType.Counter, TypeOf(samples, "gc_gen0_collections"));
        Assert.Equal(MetricType.Counter, TypeOf(samples, "gc_allocated_bytes"));
        Assert.Equal(MetricType.Gauge, TypeOf(samples, "memory_working_set_bytes"));
    }

    [Fact]
    public void Collect_NegativeCpuDelta_ReportsZeroCpu()
    {
        var sampler = new FakeSampler { ProcessorCount = 1 };
        var provider = new RuntimeMetricProvider(sampler);

        sampler.Reading = Reading(9000, 0);
        provider.Collect();
        sampler.Reading = Reading(1000, 1000);
        var samples = provider.Collect();

        Assert.Equal(0, ValueOf(samples, "cpu_percent"));
    }

    [Fact]
    public void Collect_SecondCall_ComputesCpuFromDelta()
    {
        var sampler = new FakeSampler { ProcessorCount = 2 };
        var provider = new RuntimeMetricProvider(sampler);

        sampler.Reading = Reading(10_000, 0);
        provider.Collect();
        sampler.Reading = Reading(11_000, 2000);
        var samples = provider.Collect();

        // delta cpu 1000ms over wall 2000ms * 2 cores => 25%
        Assert.Equal(25, ValueOf(samples, "cpu_percent"), 3);
    }

    [Fact]
    public void Collect_ZeroWallDelta_ReportsZeroCpu()
    {
        var sampler = new FakeSampler { ProcessorCount = 1 };
        var provider = new RuntimeMetricProvider(sampler);

        sampler.Reading = Reading(1000, 5000);
        provider.Collect();
        sampler.Reading = Reading(9000, 5000);
        var samples = provider.Collect();

        Assert.Equal(0, ValueOf(samples, "cpu_percent"));
    }

    private static ProcessRuntimeReading Reading(double cpuMs, double t)
    {
        return new ProcessRuntimeReading
        {
            TotalProcessorTime = TimeSpan.FromMilliseconds(cpuMs),
            Timestamp = DateTimeOffset.UnixEpoch.AddMilliseconds(t)
        };
    }

    private static MetricType TypeOf(IReadOnlyList<MetricSample> samples, string name)
    {
        return samples.Single(s => s.Name == name).Type;
    }

    private static double ValueOf(IReadOnlyList<MetricSample> samples, string name)
    {
        return samples.Single(s => s.Name == name).Value;
    }

    private sealed class FakeSampler : IProcessRuntimeSampler
    {
        public ProcessRuntimeReading Reading { get; set; } = new() { Timestamp = DateTimeOffset.UnixEpoch };
        public int ProcessorCount { get; set; } = 1;

        public ProcessRuntimeReading Read()
        {
            return Reading;
        }
    }
}

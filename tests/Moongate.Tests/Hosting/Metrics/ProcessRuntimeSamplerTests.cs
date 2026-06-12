using Moongate.Server.Services.Metrics;

namespace Moongate.Tests.Hosting.Metrics;

public class ProcessRuntimeSamplerTests
{
    [Fact]
    public void Read_ReturnsNonNegativeValues()
    {
        var sampler = new ProcessRuntimeSampler();

        var reading = sampler.Read();

        Assert.True(sampler.ProcessorCount >= 1);
        Assert.True(reading.WorkingSetBytes >= 0);
        Assert.True(reading.ManagedHeapBytes >= 0);
        Assert.True(reading.GcHeapSizeBytes >= 0);
        Assert.True(reading.AllocatedBytesTotal >= 0);
        Assert.True(reading.Gen0Collections >= 0);
        Assert.True(reading.TotalProcessorTime >= TimeSpan.Zero);
        Assert.NotEqual(default, reading.Timestamp);
    }
}

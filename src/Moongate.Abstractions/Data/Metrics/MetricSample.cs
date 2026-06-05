using Moongate.Abstractions.Types.Metrics;

namespace Moongate.Abstractions.Data.Metrics;

/// <summary>
/// One collected metric datapoint returned by an <see cref="Interfaces.Metrics.IMetricProvider" />.
/// </summary>
public sealed record MetricSample
{
    /// <summary>Sample name without provider prefix. The collector prepends the prefix.</summary>
    public string Name { get; init; }

    /// <summary>Numeric value at collection time.</summary>
    public double Value { get; init; }

    /// <summary>Counter or Gauge. Default Gauge.</summary>
    public MetricType Type { get; init; }

    /// <summary>Optional label set (<c>{key="value",...}</c> in OpenMetrics output).</summary>
    public IReadOnlyDictionary<string, string>? Tags { get; init; }

    /// <summary>Optional one-line description. Becomes <c># HELP</c> in OpenMetrics output.</summary>
    public string? Help { get; init; }

    /// <summary>Optional point-in-time override. Defaults to collection time.</summary>
    public DateTimeOffset? Timestamp { get; init; }

    public MetricSample(
        string Name,
        double Value,
        MetricType Type = MetricType.Gauge,
        IReadOnlyDictionary<string, string>? Tags = null,
        string? Help = null,
        DateTimeOffset? Timestamp = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Name);

        this.Name = Name;
        this.Value = Value;
        this.Type = Type;
        this.Tags = Tags;
        this.Help = Help;
        this.Timestamp = Timestamp;
    }
}

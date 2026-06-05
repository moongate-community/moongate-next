using System.Globalization;
using System.Text;
using Moongate.Abstractions.Data.Metrics;
using Moongate.Abstractions.Types.Metrics;

namespace Moongate.Server.Services.Metrics;

/// <summary>
/// Renders a <see cref="MetricsSnapshot" /> into OpenMetrics 1.0 text/plain.
/// No NuGet dependency; format is small and stable.
/// </summary>
public static class OpenMetricsFormatter
{
    /// <summary>
    /// Returns the OpenMetrics text representation of <paramref name="snapshot" />, including the trailing <c># EOF</c> line.
    /// Counter sample names that do not already end in <c>_total</c> get the suffix appended.
    /// </summary>
    public static string Format(MetricsSnapshot snapshot)
    {
        var sb = new StringBuilder(256);
        var groups = snapshot.Samples
                             .GroupBy(s => GetEmittedName(s), StringComparer.Ordinal);

        foreach (var group in groups)
        {
            var first = group.First();
            var emittedName = GetEmittedName(first);
            var help = first.Help;

            if (!string.IsNullOrEmpty(help))
            {
                sb.Append("# HELP ").Append(emittedName).Append(' ').Append(help).Append('\n');
            }

            sb.Append("# TYPE ").Append(emittedName).Append(' ').Append(TypeName(first.Type)).Append('\n');

            foreach (var sample in group)
            {
                sb.Append(emittedName);
                AppendTags(sb, sample.Tags);
                sb.Append(' ');
                sb.Append(sample.Value.ToString("R", CultureInfo.InvariantCulture));
                sb.Append('\n');
            }
        }

        sb.Append("# EOF\n");

        return sb.ToString();
    }

    private static void AppendEscaped(StringBuilder sb, string value)
    {
        foreach (var c in value)
        {
            switch (c)
            {
                case '\\':
                    sb.Append(@"\\");

                    break;
                case '"':
                    sb.Append(@"\""");

                    break;
                case '\n':
                    sb.Append(@"\n");

                    break;
                default:
                    sb.Append(c);

                    break;
            }
        }
    }

    private static void AppendTags(StringBuilder sb, IReadOnlyDictionary<string, string>? tags)
    {
        if (tags is null || tags.Count == 0)
        {
            return;
        }

        sb.Append('{');
        var first = true;

        foreach (var kvp in tags)
        {
            if (!first)
            {
                sb.Append(',');
            }

            sb.Append(kvp.Key).Append('=').Append('"');
            AppendEscaped(sb, kvp.Value);
            sb.Append('"');
            first = false;
        }

        sb.Append('}');
    }

    private static string GetEmittedName(MetricSample sample)
    {
        if (sample.Type == MetricType.Counter && !sample.Name.EndsWith("_total", StringComparison.Ordinal))
        {
            return sample.Name + "_total";
        }

        return sample.Name;
    }

    private static string TypeName(MetricType type)
        => type switch
        {
            MetricType.Counter => "counter",
            MetricType.Gauge   => "gauge",
            _                  => "unknown"
        };
}

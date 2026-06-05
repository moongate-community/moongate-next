using Moongate.Abstractions.Data.Metrics;
using Moongate.Abstractions.Types.Metrics;
using Moongate.Server.Services.Metrics;

namespace Moongate.Tests.Hosting.Metrics;

public class OpenMetricsFormatterTests
{
    [Fact]
    public void Format_CounterAlreadyEndingInTotal_LeavesNameAsIs()
    {
        var snapshot = new MetricsSnapshot(
            DateTimeOffset.UtcNow,
            [new("requests_total", 123, MetricType.Counter)]
        );

        var text = OpenMetricsFormatter.Format(snapshot);

        Assert.Contains("# TYPE requests_total counter\n", text);
        Assert.DoesNotContain("requests_total_total", text);
    }

    [Fact]
    public void Format_CounterMissingTotalSuffix_AppendsIt()
    {
        var snapshot = new MetricsSnapshot(
            DateTimeOffset.UtcNow,
            [new("connections", 7, MetricType.Counter)]
        );

        var text = OpenMetricsFormatter.Format(snapshot);

        Assert.Contains("# TYPE connections_total counter\n", text);
        Assert.Contains("connections_total 7\n", text);
    }

    [Fact]
    public void Format_EmptySnapshot_EmitsOnlyEof()
    {
        var snapshot = new MetricsSnapshot(DateTimeOffset.UtcNow, []);
        var text = OpenMetricsFormatter.Format(snapshot);

        Assert.Equal("# EOF\n", text);
    }

    [Fact]
    public void Format_MultipleSamplesSameName_EmitOneHelpAndTypeBlockOnly()
    {
        var snapshot = new MetricsSnapshot(
            DateTimeOffset.UtcNow,
            [
                new("conn", 1, MetricType.Gauge, new Dictionary<string, string> { ["zone"] = "a" }, "Active connections"),
                new("conn", 2, MetricType.Gauge, new Dictionary<string, string> { ["zone"] = "b" })
            ]
        );

        var text = OpenMetricsFormatter.Format(snapshot);

        Assert.Equal(1, CountOccurrences(text, "# TYPE conn gauge"));
        Assert.Equal(1, CountOccurrences(text, "# HELP conn"));
        Assert.Contains(@"conn{zone=""a""} 1", text);
        Assert.Contains(@"conn{zone=""b""} 2", text);
    }

    [Fact]
    public void Format_SingleGauge_EmitsHelpTypeAndValue()
    {
        var snapshot = new MetricsSnapshot(
            DateTimeOffset.UtcNow,
            [new("tick_queue_depth", 42, Help: "Tick events pending")]
        );

        var text = OpenMetricsFormatter.Format(snapshot);

        Assert.Contains("# HELP tick_queue_depth Tick events pending\n", text);
        Assert.Contains("# TYPE tick_queue_depth gauge\n", text);
        Assert.Contains("tick_queue_depth 42\n", text);
        Assert.EndsWith("# EOF\n", text);
    }

    [Fact]
    public void Format_TagValueWithNewline_IsEscaped()
    {
        var snapshot = new MetricsSnapshot(
            DateTimeOffset.UtcNow,
            [new("x", 1, MetricType.Gauge, new Dictionary<string, string> { ["k"] = "line1\nline2" })]
        );

        var text = OpenMetricsFormatter.Format(snapshot);

        Assert.Contains(@"x{k=""line1\nline2""} 1", text);
    }

    [Fact]
    public void Format_TagValueWithQuoteAndBackslash_IsEscaped()
    {
        var snapshot = new MetricsSnapshot(
            DateTimeOffset.UtcNow,
            [
                new(
                    "x",
                    1,
                    MetricType.Gauge,
                    new Dictionary<string, string> { ["k"] = @"a""b\c" }
                )
            ]
        );

        var text = OpenMetricsFormatter.Format(snapshot);

        Assert.Contains(@"x{k=""a\""b\\c""} 1", text);
    }

    [Fact]
    public void Format_WithTags_EmitsLabelSet()
    {
        var snapshot = new MetricsSnapshot(
            DateTimeOffset.UtcNow,
            [
                new(
                    "handler_errors_total",
                    5,
                    MetricType.Counter,
                    new Dictionary<string, string> { ["handler"] = "MovementHandler", ["path"] = "tick" }
                )
            ]
        );

        var text = OpenMetricsFormatter.Format(snapshot);

        Assert.Matches(
            @"handler_errors_total\{(handler=""MovementHandler"",path=""tick""|path=""tick"",handler=""MovementHandler"")\} 5\n",
            text
        );
    }

    private static int CountOccurrences(string haystack, string needle)
    {
        var count = 0;
        var idx = 0;

        while ((idx = haystack.IndexOf(needle, idx, StringComparison.Ordinal)) >= 0)
        {
            count++;
            idx += needle.Length;
        }

        return count;
    }
}

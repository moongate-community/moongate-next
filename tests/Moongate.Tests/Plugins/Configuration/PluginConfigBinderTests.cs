using System.Text.Json;
using Moongate.Plugins.Configuration;

namespace Moongate.Tests.Plugins.Configuration;

public class PluginConfigBinderTests
{
    [Fact]
    public void Apply_CoercesNumericJsonElementToInt()
    {
        var existing = new Sample { Inner = new() { Port = 1 } };

        var result = PluginConfigBinder.Apply(existing, new Dictionary<string, object?> { ["inner.port"] = Json("2525") });

        Assert.Equal(2525, result.Inner.Port);
    }

    [Fact]
    public void Apply_OverlaysProvidedLeaf_AndPreservesUnspecified()
    {
        var existing = new Sample
        {
            Enabled = false,
            Inner = new() { Host = "old", Port = 25 },
            Secrets = new() { Prefix = "keep" }
        };

        var values = new Dictionary<string, object?>
        {
            ["inner.host"] = Json("\"mail\""),
            ["inner.port"] = Json("587")
        };

        var result = PluginConfigBinder.Apply(existing, values);

        Assert.Equal("mail", result.Inner.Host);
        Assert.Equal(587, result.Inner.Port);
        Assert.Equal("keep", result.Secrets.Prefix); // not in values -> preserved
        Assert.False(result.Enabled);
    }

    [Fact]
    public void Apply_PassesThroughClrPrimitive()
    {
        var existing = new Sample();

        var result = PluginConfigBinder.Apply(existing, new Dictionary<string, object?> { ["inner.port"] = 99 });

        Assert.Equal(99, result.Inner.Port);
    }

    [Fact]
    public void Apply_UnwrapsJsonElementBool()
    {
        var existing = new Sample();

        var result = PluginConfigBinder.Apply(existing, new Dictionary<string, object?> { ["enabled"] = Json("true") });

        Assert.True(result.Enabled);
    }

    [Fact]
    public void Apply_UnwrapsJsonElementString()
    {
        var existing = new Sample();

        var result = PluginConfigBinder.Apply(existing, new Dictionary<string, object?> { ["inner.host"] = Json("\"h\"") });

        Assert.Equal("h", result.Inner.Host);
    }

    private static JsonElement Json(string json)
        => JsonSerializer.Deserialize<JsonElement>(json);
}

public sealed class Sample
{
    public bool Enabled { get; set; }

    public SampleInner Inner { get; set; } = new();

    public SampleSecrets Secrets { get; set; } = new();
}

public sealed class SampleInner
{
    public string Host { get; set; } = "";

    public int Port { get; set; }
}

public sealed class SampleSecrets
{
    public string Prefix { get; set; } = "";
}

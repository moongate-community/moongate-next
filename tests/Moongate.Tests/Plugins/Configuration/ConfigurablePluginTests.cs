using System.Text.Json;
using Moongate.Plugins.Configuration;
using Moongate.Plugins.Data;

namespace Moongate.Tests.Plugins.Configuration;

public class ConfigurablePluginTests
{
    [Fact]
    public async Task SaveConfigAsync_AppliesOverlay_AndPassesMergedToSaveTyped()
    {
        var plugin = new FakePlugin
        {
            Existing = new Sample
            {
                Inner = new SampleInner { Host = "old", Port = 25 },
                Secrets = new SampleSecrets { Prefix = "keep" }
            }
        };

        var request = new PluginConfigSaveRequest(new Dictionary<string, object?>
        {
            ["inner.host"] = JsonSerializer.Deserialize<JsonElement>("\"mail\"")
        });

        var result = await plugin.SaveConfigAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(plugin.Saved);
        Assert.Equal("mail", plugin.Saved!.Inner.Host);
        Assert.Equal("keep", plugin.Saved.Secrets.Prefix); // preserved
    }

    [Fact]
    public async Task SaveConfigAsync_NullValues_ReturnsFailure_AndDoesNotSave()
    {
        var plugin = new FakePlugin();

        var result = await plugin.SaveConfigAsync(new PluginConfigSaveRequest(null!));

        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
        Assert.Null(plugin.Saved);
    }

    [Fact]
    public async Task SaveConfigAsync_BinderFailure_ReturnsFailure_AndDoesNotSave()
    {
        var plugin = new FakePlugin();

        var request = new PluginConfigSaveRequest(new Dictionary<string, object?>
        {
            ["inner.port"] = JsonSerializer.Deserialize<JsonElement>("\"not-a-number\"")
        });

        var result = await plugin.SaveConfigAsync(request);

        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
        Assert.Null(plugin.Saved);
    }

    private sealed class FakePlugin : ConfigurablePlugin<Sample>
    {
        public Sample Existing { get; set; } = new();

        public Sample? Saved { get; private set; }

        public override ValueTask<PluginConfigForm> GetConfigFormAsync(CancellationToken ct = default)
            => ValueTask.FromResult(new PluginConfigForm([]));

        protected override ValueTask<Sample> LoadConfigAsync(CancellationToken ct)
            => ValueTask.FromResult(Existing);

        protected override ValueTask<PluginConfigSaveResult> SaveTypedConfigAsync(Sample config, CancellationToken ct)
        {
            Saved = config;

            return ValueTask.FromResult(new PluginConfigSaveResult(true, false, [], null));
        }
    }
}

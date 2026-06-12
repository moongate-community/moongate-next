using System.Text.Json;
using Moongate.Plugins.Configuration;
using Moongate.Plugins.Data;

namespace Moongate.Tests.Plugins.Configuration;

public class ConfigurablePluginTests
{
    private sealed class FakeFormPlugin : ConfigurablePlugin<FormSample>
    {
        public FormSample Existing { get; set; } = new();

        protected override ValueTask<FormSample> LoadConfigAsync(CancellationToken ct)
            => ValueTask.FromResult(Existing);

        protected override ValueTask<PluginConfigSaveResult> SaveTypedConfigAsync(FormSample config, CancellationToken ct)
            => ValueTask.FromResult(new PluginConfigSaveResult(true, false, [], null));
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

    [Fact]
    public async Task GetConfigFormAsync_DefaultsToAttributeScan()
    {
        var plugin = new FakeFormPlugin { Existing = new() { Smtp = { Port = 2525 } } };

        var form = await plugin.GetConfigFormAsync();

        Assert.Equal(["general", "sender", "smtp"], form.Sections.Select(section => section.Id).ToArray());
        var port = form.Sections.SelectMany(section => section.Fields).Single(field => field.Path == "smtp.port");
        Assert.Equal(2525, port.Value);
    }

    [Fact]
    public async Task SaveConfigAsync_AppliesOverlay_AndPassesMergedToSaveTyped()
    {
        var plugin = new FakePlugin
        {
            Existing = new()
            {
                Inner = new() { Host = "old", Port = 25 },
                Secrets = new() { Prefix = "keep" }
            }
        };

        var request = new PluginConfigSaveRequest(
            new()
            {
                ["inner.host"] = JsonSerializer.Deserialize<JsonElement>("\"mail\"")
            }
        );

        var result = await plugin.SaveConfigAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(plugin.Saved);
        Assert.Equal("mail", plugin.Saved!.Inner.Host);
        Assert.Equal("keep", plugin.Saved.Secrets.Prefix); // preserved
    }

    [Fact]
    public async Task SaveConfigAsync_BinderFailure_ReturnsFailure_AndDoesNotSave()
    {
        var plugin = new FakePlugin();

        var request = new PluginConfigSaveRequest(
            new()
            {
                ["inner.port"] = JsonSerializer.Deserialize<JsonElement>("\"not-a-number\"")
            }
        );

        var result = await plugin.SaveConfigAsync(request);

        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
        Assert.Null(plugin.Saved);
    }

    [Fact]
    public async Task SaveConfigAsync_NullValues_ReturnsFailure_AndDoesNotSave()
    {
        var plugin = new FakePlugin();

        var result = await plugin.SaveConfigAsync(new(null!));

        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
        Assert.Null(plugin.Saved);
    }
}

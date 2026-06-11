using DryIoc;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Plugins.Data;
using Moongate.Plugin.Email;
using Moongate.Plugin.Email.Data;
using Moongate.Plugin.Email.Interfaces;

namespace Moongate.Tests.Plugins.Email;

public sealed class EmailPluginTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"moongate-email-plugin-{Guid.NewGuid():N}");

    private sealed class CapturingTester : IEmailConfigurationTester
    {
        public EmailPluginConfig? Config { get; private set; }

        public ValueTask<PluginTestResult> TestAsync(
            EmailPluginConfig config,
            CancellationToken cancellationToken = default
        )
        {
            Config = config;

            return ValueTask.FromResult(new PluginTestResult(true, "OK", []));
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetConfigFormAsync_ReturnsCurrentYamlValues()
    {
        var pluginDirectory = CreatePluginDirectory(
            """
            enabled: true
            from:
              name: Moon Mail
              address: noreply@example.com
            smtp:
              host: smtp.example.com
              port: 2525
              username: noreply@example.com
              password_secret: smtp_password
              use_ssl: false
              start_tls: true
              timeout_seconds: 45
            activation:
              template_id: account_activation
              url_template: https://example.com/activate?activation_id={activation_id}
            templates:
              directory: templates
              reload_on_change: true
            """
        );
        var plugin = ConfigurePlugin(pluginDirectory);

        var form = await plugin.GetConfigFormAsync();

        Assert.Equal("smtp.example.com", FindField(form, "smtp.host").Value);
        Assert.Equal(2525, FindField(form, "smtp.port").Value);
        Assert.True((bool)FindField(form, "enabled").Value!);
        Assert.True(FindField(form, "smtp.password_secret").SecretReference);
    }

    [Fact]
    public async Task SaveConfigAsync_ValidValues_WritesYaml()
    {
        var pluginDirectory = CreatePluginDirectory();
        var plugin = ConfigurePlugin(pluginDirectory);
        var request = new PluginConfigSaveRequest(
            new Dictionary<string, object?>
            {
                ["enabled"] = true,
                ["from.address"] = "noreply@example.com",
                ["smtp.host"] = "smtp.example.com",
                ["smtp.username"] = "noreply@example.com",
                ["smtp.password_secret"] = "smtp_password",
                ["activation.url_template"] = "https://example.com/activate?activation_id={activation_id}"
            }
        );

        var result = await plugin.SaveConfigAsync(request);

        Assert.True(result.Success);
        Assert.True(result.RequiresRestart);
        var yaml = File.ReadAllText(Path.Combine(pluginDirectory, PluginContext.PluginConfigFileName));
        Assert.Contains("enabled: true", yaml);
        Assert.Contains("address: noreply@example.com", yaml);
        Assert.Contains("host: smtp.example.com", yaml);
    }

    [Fact]
    public async Task SaveConfigAsync_UnknownField_ReturnsValidationError()
    {
        var plugin = ConfigurePlugin(CreatePluginDirectory());
        var request = new PluginConfigSaveRequest(
            new Dictionary<string, object?>
            {
                ["smtp.unknown"] = "value"
            }
        );

        var result = await plugin.SaveConfigAsync(request);

        Assert.False(result.Success);
        Assert.Contains(result.Errors, error => error.Contains("Unsupported config field", StringComparison.Ordinal));
    }

    [Fact]
    public async Task TestAsync_ValidConfig_DelegatesToTester()
    {
        var tester = new CapturingTester();
        var pluginDirectory = CreatePluginDirectory(
            """
            enabled: true
            from:
              address: noreply@example.com
            smtp:
              host: smtp.example.com
              username: noreply@example.com
              password_secret: smtp_password
            activation:
              url_template: https://example.com/activate?activation_id={activation_id}
            """
        );
        var plugin = ConfigurePlugin(pluginDirectory, tester);

        var result = await plugin.TestAsync();

        Assert.True(result.Success);
        Assert.Equal("OK", result.Message);
        Assert.NotNull(tester.Config);
        Assert.Equal("smtp.example.com", tester.Config!.Smtp.Host);
    }

    private static PluginConfigField FindField(PluginConfigForm form, string path)
        => form.Sections.SelectMany(section => section.Fields).Single(field => field.Path == path);

    private EmailPlugin ConfigurePlugin(string pluginDirectory, CapturingTester? tester = null)
    {
        var plugin = tester is null
                         ? new EmailPlugin()
                         : new EmailPlugin(_ => tester);
        var container = new Container();
        var directories = new DirectoriesConfig(_root, Enum.GetNames<DirectoryType>());
        var context = new PluginContext(pluginDirectory, directories);

        plugin.Configure(container, context);

        return plugin;
    }

    private string CreatePluginDirectory(string? yaml = null)
    {
        var pluginDirectory = Path.Combine(_root, "email");
        Directory.CreateDirectory(pluginDirectory);

        if (yaml is not null)
        {
            File.WriteAllText(Path.Combine(pluginDirectory, PluginContext.PluginConfigFileName), yaml);
        }

        return pluginDirectory;
    }
}

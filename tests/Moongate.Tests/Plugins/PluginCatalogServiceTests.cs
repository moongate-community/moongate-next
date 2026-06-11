using Moongate.Plugins.Data;
using Moongate.Plugins.Interfaces.Plugins;
using Moongate.Plugins.Services;

namespace Moongate.Tests.Plugins;

public sealed class PluginCatalogServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"moongate-plugin-catalog-{Guid.NewGuid():N}");

    private sealed class FakePlugin : IMoongatePlugin
    {
        public PluginMetadata Metadata { get; } = new()
        {
            Id = "moongate.fixture.catalog",
            Name = "Catalog Fixture",
            Version = new(1, 2, 3),
            Author = "Moongate Tests",
            Description = "Fixture plugin for catalog tests",
            Dependencies = ["moongate.core"]
        };

        public void Configure(DryIoc.IContainer container, PluginContext context) { }
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
    public async Task GetConfigAsync_MissingConfig_ReturnsNonExistingView()
    {
        var plugin = CreateLoadedPlugin("catalog");
        var service = new PluginCatalogService([plugin]);

        var config = await service.GetConfigAsync("moongate.fixture.catalog");

        Assert.NotNull(config);
        Assert.False(config!.Exists);
        Assert.Equal("catalog/plugin.yaml", NormalizePath(config.ConfigPath));
        Assert.Equal("", config.SanitizedYaml);
        Assert.Empty(config.RedactedKeys);
    }

    [Fact]
    public async Task GetConfigAsync_SanitizesSensitiveYamlKeys()
    {
        var plugin = CreateLoadedPlugin("catalog");
        File.WriteAllText(
            Path.Combine(plugin.PluginDirectory, "plugin.yaml"),
            """
            smtp:
              username: noreply@example.com
              password: plain-text
              password_secret: smtp_password
            api:
              nested_api_key: abc
              credentials:
                token: token-value
            """
        );
        var service = new PluginCatalogService([plugin]);

        var config = await service.GetConfigAsync("moongate.fixture.catalog");

        Assert.NotNull(config);
        Assert.True(config!.Exists);
        Assert.Contains("password: '***REDACTED***'", config.SanitizedYaml);
        Assert.Contains("password_secret: smtp_password", config.SanitizedYaml);
        Assert.Contains("nested_api_key: '***REDACTED***'", config.SanitizedYaml);
        Assert.Contains("credentials: '***REDACTED***'", config.SanitizedYaml);
        Assert.DoesNotContain("plain-text", config.SanitizedYaml);
        Assert.DoesNotContain("token-value", config.SanitizedYaml);
        Assert.Equal(["smtp.password", "api.nested_api_key", "api.credentials"], config.RedactedKeys);
    }

    [Fact]
    public void GetLoadedPlugins_ReturnsMetadata()
    {
        var plugin = CreateLoadedPlugin("catalog");
        File.WriteAllText(Path.Combine(plugin.PluginDirectory, "plugin.yaml"), "enabled: true\n");
        var service = new PluginCatalogService([plugin]);

        var entry = Assert.Single(service.GetLoadedPlugins());

        Assert.Equal("moongate.fixture.catalog", entry.Id);
        Assert.Equal("Catalog Fixture", entry.Name);
        Assert.Equal("1.2.3", entry.Version);
        Assert.Equal("Moongate Tests", entry.Author);
        Assert.Equal("Fixture plugin for catalog tests", entry.Description);
        Assert.Equal(["moongate.core"], entry.Dependencies);
        Assert.Equal("catalog", entry.DirectoryName);
        Assert.True(entry.HasConfig);
        Assert.Equal(typeof(FakePlugin).Assembly.GetName().Name, entry.AssemblyName);
    }

    [Fact]
    public async Task GetConfigAsync_UnknownPlugin_ReturnsNull()
    {
        var service = new PluginCatalogService([]);

        var config = await service.GetConfigAsync("missing");

        Assert.Null(config);
    }

    private LoadedPlugin CreateLoadedPlugin(string directoryName)
    {
        var pluginDirectory = Path.Combine(_root, directoryName);
        Directory.CreateDirectory(pluginDirectory);

        return new(pluginDirectory, new FakePlugin(), typeof(FakePlugin).Assembly);
    }

    private static string NormalizePath(string path)
        => path.Replace(Path.DirectorySeparatorChar, '/');
}

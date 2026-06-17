using Microsoft.AspNetCore.Http.HttpResults;
using Moongate.Plugins.Data;
using Moongate.Plugins.Interfaces.Plugins;
using Moongate.Server.Extensions.Endpoints;

namespace Moongate.Tests.Server.Endpoints;

public sealed class AdminPluginEndpointExtensionsTests
{
    [Fact]
    public void HandleGet_KnownPlugin_ReturnsPlugin()
    {
        var service = new FakePluginCatalogService();
        var plugin = CreatePlugin();
        service.Add(plugin);

        var result = AdminPluginEndpointExtensions.HandleGet(service, "MOONGATE.FIXTURE.CATALOG");

        var ok = Assert.IsType<Ok<PluginCatalogEntry>>(result);
        Assert.Same(plugin, ok.Value);
    }

    [Fact]
    public void HandleGet_UnknownPlugin_ReturnsNotFound()
    {
        var result = AdminPluginEndpointExtensions.HandleGet(new FakePluginCatalogService(), "missing");

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task HandleGetConfigAsync_KnownPlugin_ReturnsConfig()
    {
        var service = new FakePluginCatalogService();
        var plugin = CreatePlugin();
        var config = new PluginConfigView(plugin.Id, true, "catalog/plugin.yaml", "enabled: true\n", []);
        service.Add(plugin, config);

        var result = await AdminPluginEndpointExtensions.HandleGetConfigAsync(
            service,
            plugin.Id,
            CancellationToken.None
        );

        var ok = Assert.IsType<Ok<PluginConfigView>>(result);
        Assert.Same(config, ok.Value);
    }

    [Fact]
    public async Task HandleGetConfigAsync_UnknownPlugin_ReturnsNotFound()
    {
        var result = await AdminPluginEndpointExtensions.HandleGetConfigAsync(
            new FakePluginCatalogService(),
            "missing",
            CancellationToken.None
        );

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task HandleGetConfigFormAsync_KnownConfigurablePlugin_ReturnsForm()
    {
        var service = new FakePluginCatalogService();
        var plugin = CreatePlugin();
        var form = new PluginConfigForm([new PluginConfigSection("general", "General", [])]);
        service.Add(plugin, form: form);

        var result = await AdminPluginEndpointExtensions.HandleGetConfigFormAsync(
            service,
            plugin.Id,
            CancellationToken.None
        );

        var ok = Assert.IsType<Ok<PluginConfigForm>>(result);
        Assert.Same(form, ok.Value);
    }

    [Fact]
    public async Task HandleGetConfigFormAsync_UnsupportedPlugin_ReturnsNotFound()
    {
        var result = await AdminPluginEndpointExtensions.HandleGetConfigFormAsync(
            new FakePluginCatalogService(),
            "missing",
            CancellationToken.None
        );

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public void HandleList_ReturnsLoadedPlugins()
    {
        var service = new FakePluginCatalogService();
        service.Add(CreatePlugin());

        var result = AdminPluginEndpointExtensions.HandleList(service);

        var ok = Assert.IsType<Ok<IReadOnlyList<PluginCatalogEntry>>>(result);
        Assert.NotNull(ok.Value);
        Assert.Single(ok.Value);
    }

    [Fact]
    public async Task HandleSaveConfigAsync_KnownConfigurablePlugin_ReturnsResult()
    {
        var service = new FakePluginCatalogService();
        var plugin = CreatePlugin();
        var saveResult = new PluginConfigSaveResult(true, true, [], null);
        service.Add(plugin, saveResult: saveResult);

        var result = await AdminPluginEndpointExtensions.HandleSaveConfigAsync(
            service,
            plugin.Id,
            new PluginConfigSaveRequest(new Dictionary<string, object?>()),
            CancellationToken.None
        );

        var ok = Assert.IsType<Ok<PluginConfigSaveResult>>(result);
        Assert.Same(saveResult, ok.Value);
    }

    [Fact]
    public async Task HandleSaveConfigAsync_UnsupportedPlugin_ReturnsNotFound()
    {
        var result = await AdminPluginEndpointExtensions.HandleSaveConfigAsync(
            new FakePluginCatalogService(),
            "missing",
            new PluginConfigSaveRequest(new Dictionary<string, object?>()),
            CancellationToken.None
        );

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task HandleTestAsync_KnownTestablePlugin_ReturnsResult()
    {
        var service = new FakePluginCatalogService();
        var plugin = CreatePlugin();
        var testResult = new PluginTestResult(true, "OK", []);
        service.Add(plugin, testResult: testResult);

        var result = await AdminPluginEndpointExtensions.HandleTestAsync(
            service,
            plugin.Id,
            CancellationToken.None
        );

        var ok = Assert.IsType<Ok<PluginTestResult>>(result);
        Assert.Same(testResult, ok.Value);
    }

    [Fact]
    public async Task HandleTestAsync_UnsupportedPlugin_ReturnsNotFound()
    {
        var result = await AdminPluginEndpointExtensions.HandleTestAsync(
            new FakePluginCatalogService(),
            "missing",
            CancellationToken.None
        );

        Assert.IsType<NotFound>(result);
    }

    private static PluginCatalogEntry CreatePlugin()
    {
        return new PluginCatalogEntry(
            "moongate.fixture.catalog",
            "Catalog Fixture",
            "1.2.3",
            "Moongate Tests",
            "Fixture plugin",
            [],
            "Moongate.PluginFixtures.Basic",
            "catalog",
            true
        );
    }

    private sealed class FakePluginCatalogService : IPluginCatalogService
    {
        private readonly Dictionary<string, PluginConfigView> _configs = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PluginConfigForm> _forms = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PluginCatalogEntry> _plugins = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PluginConfigSaveResult> _saveResults = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PluginTestResult> _testResults = new(StringComparer.OrdinalIgnoreCase);

        public ValueTask<PluginConfigView?> GetConfigAsync(
            string pluginId,
            CancellationToken cancellationToken = default
        )
        {
            return ValueTask.FromResult(_configs.GetValueOrDefault(pluginId));
        }

        public ValueTask<PluginConfigForm?> GetConfigFormAsync(
            string pluginId,
            CancellationToken cancellationToken = default
        )
        {
            return ValueTask.FromResult(_forms.GetValueOrDefault(pluginId));
        }

        public IReadOnlyList<PluginCatalogEntry> GetLoadedPlugins()
        {
            return _plugins.Values.ToArray();
        }

        public ValueTask<PluginConfigSaveResult?> SaveConfigAsync(
            string pluginId,
            PluginConfigSaveRequest request,
            CancellationToken cancellationToken = default
        )
        {
            return ValueTask.FromResult(_saveResults.GetValueOrDefault(pluginId));
        }

        public ValueTask<PluginTestResult?> TestAsync(
            string pluginId,
            CancellationToken cancellationToken = default
        )
        {
            return ValueTask.FromResult(_testResults.GetValueOrDefault(pluginId));
        }

        public void Add(
            PluginCatalogEntry plugin,
            PluginConfigView? config = null,
            PluginConfigForm? form = null,
            PluginConfigSaveResult? saveResult = null,
            PluginTestResult? testResult = null
        )
        {
            _plugins[plugin.Id] = plugin;

            if (config is not null)
            {
                _configs[plugin.Id] = config;
            }

            if (form is not null)
            {
                _forms[plugin.Id] = form;
            }

            if (saveResult is not null)
            {
                _saveResults[plugin.Id] = saveResult;
            }

            if (testResult is not null)
            {
                _testResults[plugin.Id] = testResult;
            }
        }
    }
}

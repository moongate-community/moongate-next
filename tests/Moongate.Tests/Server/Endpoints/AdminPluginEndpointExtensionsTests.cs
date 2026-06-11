using Microsoft.AspNetCore.Http.HttpResults;
using Moongate.Plugins.Data;
using Moongate.Plugins.Interfaces.Plugins;
using Moongate.Server.Extensions.Endpoints;

namespace Moongate.Tests.Server.Endpoints;

public sealed class AdminPluginEndpointExtensionsTests
{
    private sealed class FakePluginCatalogService : IPluginCatalogService
    {
        private readonly Dictionary<string, PluginCatalogEntry> _plugins = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PluginConfigView> _configs = new(StringComparer.OrdinalIgnoreCase);

        public void Add(PluginCatalogEntry plugin, PluginConfigView? config = null)
        {
            _plugins[plugin.Id] = plugin;

            if (config is not null)
            {
                _configs[plugin.Id] = config;
            }
        }

        public ValueTask<PluginConfigView?> GetConfigAsync(
            string pluginId,
            CancellationToken cancellationToken = default
        )
            => ValueTask.FromResult(_configs.GetValueOrDefault(pluginId));

        public IReadOnlyList<PluginCatalogEntry> GetLoadedPlugins()
            => _plugins.Values.ToArray();
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
    public void HandleList_ReturnsLoadedPlugins()
    {
        var service = new FakePluginCatalogService();
        service.Add(CreatePlugin());

        var result = AdminPluginEndpointExtensions.HandleList(service);

        var ok = Assert.IsType<Ok<IReadOnlyList<PluginCatalogEntry>>>(result);
        Assert.NotNull(ok.Value);
        Assert.Single(ok.Value);
    }

    private static PluginCatalogEntry CreatePlugin()
        => new(
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

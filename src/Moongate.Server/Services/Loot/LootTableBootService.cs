using Moongate.Abstractions.Interfaces.Services;
using Moongate.Server.Services.Templates;
using Moongate.UO.Data.Interfaces.Services;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.Loot;

/// <summary>
///     Loads and validates loot tables at boot, then publishes a registry snapshot
///     to the loot service. Any invalid table throws and prevents the server from
///     starting (fail fast); a missing directory only logs a warning.
/// </summary>
public sealed class LootTableBootService : IMoongateService
{
    private readonly IItemService _items;
    private readonly LootTableYamlLoader _loader;
    private readonly ILogger _logger = Log.ForContext<LootTableBootService>();
    private readonly ILootService _lootService;
    private readonly LootTableRegistryStore _registryStore;
    private readonly IItemTemplateService _templates;

    public LootTableBootService(
        LootTableYamlLoader loader,
        ILootService lootService,
        IItemTemplateService templates,
        LootTableRegistryStore registryStore,
        IItemService items
    )
    {
        ArgumentNullException.ThrowIfNull(loader);
        ArgumentNullException.ThrowIfNull(lootService);
        ArgumentNullException.ThrowIfNull(templates);
        ArgumentNullException.ThrowIfNull(registryStore);
        ArgumentNullException.ThrowIfNull(items);

        _loader = loader;
        _lootService = lootService;
        _templates = templates;
        _registryStore = registryStore;
        _items = items;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        var tables = _loader.LoadAll();

        LootTableValidator.Validate(tables, _templates);

        var registry = new LootTableRegistry(tables, _templates.GetAll());
        ItemTemplateContentsValidator.Validate(_templates.GetAll(), registry, _items);

        _registryStore.SetRegistry(registry);

        if (_lootService is LootService lootService)
        {
            lootService.SetRegistry(registry);

            _logger.Information("Loot table registry ready with {Count} tables", tables.Count);

            return Task.CompletedTask;
        }

        _logger.Warning(
            "Loot service implementation {Type} does not accept a registry; {Count} validated tables were not published",
            _lootService.GetType().Name,
            tables.Count
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        return Task.CompletedTask;
    }
}

using Moongate.Abstractions.Interfaces.Services;
using Moongate.UO.Data.Interfaces.Services;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.Loot;

/// <summary>
/// Loads and validates loot tables at boot, then publishes a registry snapshot
/// to the loot service. Any invalid table throws and prevents the server from
/// starting (fail fast); a missing directory only logs a warning.
/// </summary>
public sealed class LootTableBootService : IMoongateService
{
    private readonly ILogger _logger = Log.ForContext<LootTableBootService>();
    private readonly LootTableYamlLoader _loader;
    private readonly ILootService _lootService;
    private readonly IItemTemplateService _templates;

    public LootTableBootService(
        LootTableYamlLoader loader,
        ILootService lootService,
        IItemTemplateService templates
    )
    {
        ArgumentNullException.ThrowIfNull(loader);
        ArgumentNullException.ThrowIfNull(lootService);
        ArgumentNullException.ThrowIfNull(templates);

        _loader = loader;
        _lootService = lootService;
        _templates = templates;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        var tables = _loader.LoadAll();

        LootTableValidator.Validate(tables, _templates);

        if (_lootService is LootService lootService)
        {
            lootService.SetRegistry(new(tables, _templates.GetAll()));

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

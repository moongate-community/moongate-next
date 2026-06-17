using Moongate.Abstractions.Interfaces.Services;
using Moongate.UO.Data.Interfaces.Services;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.Mobiles;

/// <summary>
///     Loads and validates all mobile templates at boot. Any invalid template throws
///     here and prevents the server from starting (fail fast).
/// </summary>
public sealed class MobileTemplateBootService : IMoongateService
{
    private readonly IItemTemplateService _items;
    private readonly MobileTemplateYamlLoader _loader;
    private readonly ILogger _logger = Log.ForContext<MobileTemplateBootService>();
    private readonly ILootService _loot;
    private readonly IMobileTemplateService _templates;

    public MobileTemplateBootService(
        MobileTemplateYamlLoader loader,
        IMobileTemplateService templates,
        IItemTemplateService items,
        ILootService loot
    )
    {
        ArgumentNullException.ThrowIfNull(loader);
        ArgumentNullException.ThrowIfNull(templates);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(loot);

        _loader = loader;
        _templates = templates;
        _items = items;
        _loot = loot;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        var loaded = _loader.LoadAll();

        MobileTemplateValidator.Validate(loaded, _items, _loot);

        _templates.Clear();
        _templates.UpsertRange(loaded);

        _logger.Information("Mobile template registry ready with {Count} templates", _templates.Count);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        return Task.CompletedTask;
    }
}

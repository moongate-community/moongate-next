using Moongate.Abstractions.Interfaces.Services;
using Moongate.UO.Data.Interfaces.Services;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.Templates;

/// <summary>
/// Loads and validates all item templates at boot. Any invalid template file
/// throws here and prevents the server from starting (fail fast).
/// </summary>
public sealed class ItemTemplateBootService : IMoongateService
{
    private readonly ILogger _logger = Log.ForContext<ItemTemplateBootService>();
    private readonly ItemTemplateYamlLoader _loader;
    private readonly IItemTemplateService _templates;

    public ItemTemplateBootService(ItemTemplateYamlLoader loader, IItemTemplateService templates)
    {
        ArgumentNullException.ThrowIfNull(loader);
        ArgumentNullException.ThrowIfNull(templates);

        _loader = loader;
        _templates = templates;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        var loaded = _loader.LoadAll();

        _templates.Clear();
        _templates.UpsertRange(loaded);

        _logger.Information("Item template registry ready with {Count} templates", _templates.Count);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        return Task.CompletedTask;
    }
}

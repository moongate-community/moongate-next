using Moongate.Abstractions.Interfaces.Services;
using Moongate.Server.Interfaces.Services.World;
using Moongate.UO.Data.Interfaces.Services;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.Loadouts;

/// <summary>
/// Loads and validates the starter loadout at boot. An invalid loadout file
/// throws and prevents the server from starting (fail fast); a missing file
/// only logs a warning and leaves no loadout configured.
/// </summary>
public sealed class StarterLoadoutBootService : IMoongateService
{
    private readonly ILogger _logger = Log.ForContext<StarterLoadoutBootService>();
    private readonly StarterLoadoutYamlLoader _loader;
    private readonly IStarterLoadoutService _loadouts;
    private readonly IItemTemplateService _templates;
    private readonly IProfessionDataService _professions;

    public StarterLoadoutBootService(
        StarterLoadoutYamlLoader loader,
        IStarterLoadoutService loadouts,
        IItemTemplateService templates,
        IProfessionDataService professions
    )
    {
        ArgumentNullException.ThrowIfNull(loader);
        ArgumentNullException.ThrowIfNull(loadouts);
        ArgumentNullException.ThrowIfNull(templates);
        ArgumentNullException.ThrowIfNull(professions);

        _loader = loader;
        _loadouts = loadouts;
        _templates = templates;
        _professions = professions;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        var definition = _loader.Load();

        if (definition is not null)
        {
            StarterLoadoutValidator.Validate(definition, _loader.StarterLoadoutFilePath, _templates, _professions);
        }

        _loadouts.SetDefinition(definition);

        _logger.Information(
            "Starter loadout {State}",
            definition is null ? "not configured" : "loaded and validated"
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        return Task.CompletedTask;
    }
}

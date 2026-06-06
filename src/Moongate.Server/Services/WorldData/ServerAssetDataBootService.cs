using Moongate.Abstractions.Interfaces.Services;
using Moongate.Server.Interfaces.Services.World;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.WorldData;

/// <summary>
/// Loads server asset world data before network services start.
/// </summary>
public sealed class ServerAssetDataBootService : IMoongateService
{
    private readonly ILogger _logger = Log.ForContext<ServerAssetDataBootService>();
    private readonly ServerAssetDataLoader _loader;
    private readonly IDoorDataService _doorDataService;
    private readonly ISpawnsDataService _spawnsDataService;
    private readonly ITeleportersDataService _teleportersDataService;
    private readonly IRegionDataService _regionDataService;
    private readonly IWeatherDataService _weatherDataService;
    private readonly IContainerDataService _containerDataService;
    private readonly ILocationCatalogService _locationCatalogService;
    private readonly INameDataService _nameDataService;
    private readonly IProfessionDataService _professionDataService;
    private readonly ISignDataService _signDataService;
    private readonly IDecorationDataService _decorationDataService;
    private readonly IMountDataService _mountDataService;

    public ServerAssetDataBootService(
        ServerAssetDataLoader loader,
        IDoorDataService doorDataService,
        ISpawnsDataService spawnsDataService,
        ITeleportersDataService teleportersDataService,
        IRegionDataService regionDataService,
        IWeatherDataService weatherDataService,
        IContainerDataService containerDataService,
        ILocationCatalogService locationCatalogService,
        INameDataService nameDataService,
        IProfessionDataService professionDataService,
        ISignDataService signDataService,
        IDecorationDataService decorationDataService,
        IMountDataService mountDataService
    )
    {
        _loader = loader;
        _doorDataService = doorDataService;
        _spawnsDataService = spawnsDataService;
        _teleportersDataService = teleportersDataService;
        _regionDataService = regionDataService;
        _weatherDataService = weatherDataService;
        _containerDataService = containerDataService;
        _locationCatalogService = locationCatalogService;
        _nameDataService = nameDataService;
        _professionDataService = professionDataService;
        _signDataService = signDataService;
        _decorationDataService = decorationDataService;
        _mountDataService = mountDataService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        _loader.LoadDoors(_doorDataService);
        _loader.LoadSpawns(_spawnsDataService);
        _loader.LoadCatalogs(
            _teleportersDataService,
            _regionDataService,
            _weatherDataService,
            _containerDataService,
            _locationCatalogService,
            _nameDataService,
            _professionDataService,
            _signDataService,
            _decorationDataService,
            _mountDataService
        );

        _logger.Information(
            "World data ready: {Doors} doors, {Spawns} spawns, {Teleporters} teleporters, {Regions} regions, " +
            "{Weather} weather, {Containers} containers, {Locations} locations, {Names} name groups, " +
            "{Professions} professions, {Signs} signs, {Decorations} decorations, {Mounts} mounts",
            _doorDataService.GetAllEntries().Count,
            _spawnsDataService.GetAllEntries().Count,
            _teleportersDataService.GetAllEntries().Count,
            _regionDataService.GetAllEntries().Count,
            _weatherDataService.GetAllEntries().Count,
            _containerDataService.GetAllContainers().Count,
            _locationCatalogService.GetAllLocations().Count,
            _nameDataService.GetAllGroups().Count,
            _professionDataService.GetAllProfessions().Count,
            _signDataService.GetAllEntries().Count,
            _decorationDataService.GetAllEntries().Count,
            _mountDataService.GetAllEntries().Count
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        return Task.CompletedTask;
    }
}

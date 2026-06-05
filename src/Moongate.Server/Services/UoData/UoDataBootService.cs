using Moongate.Abstractions.Interfaces.Services;
using Moongate.UO.Data.Interfaces.Art;
using Moongate.UO.Data.Interfaces.Bodies;
using Moongate.UO.Data.Interfaces.Expansions;
using Moongate.UO.Data.Interfaces.Hues;
using Moongate.UO.Data.Interfaces.Localization;
using Moongate.UO.Data.Interfaces.Maps;
using Moongate.UO.Data.Interfaces.Multi;
using Moongate.UO.Data.Interfaces.Races;
using Moongate.UO.Data.Interfaces.Skills;
using Moongate.UO.Data.Interfaces.Textures;
using Moongate.UO.Data.Interfaces.Tiles;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.UoData;

/// <summary>
/// Eagerly loads every UO data store at boot (constructor injection forces each singleton to load),
/// then logs a one-line readiness summary. Registered before the network service so data is ready —
/// and missing required client files fail fast — before connections are accepted.
/// </summary>
public sealed class UoDataBootService : IMoongateService
{
    private static readonly ILogger _logger = Log.ForContext<UoDataBootService>();

    private readonly ITileDataStore _tileData;
    private readonly IMapService _maps;
    private readonly ILocalizationService _localization;
    private readonly IMultiDataStore _multis;
    private readonly IArtService _art;
    private readonly ISkillDataStore _skills;
    private readonly IRaceStore _races;
    private readonly IBodyDataStore _bodies;
    private readonly IHueStore _hues;
    private readonly IRadarColorStore _radarColors;
    private readonly ITextureStore _textures;
    private readonly IExpansionStore _expansions;

    public UoDataBootService(
        ITileDataStore tileData,
        IMapService maps,
        ILocalizationService localization,
        IMultiDataStore multis,
        IArtService art,
        ISkillDataStore skills,
        IRaceStore races,
        IBodyDataStore bodies,
        IHueStore hues,
        IRadarColorStore radarColors,
        ITextureStore textures,
        IExpansionStore expansions
    )
    {
        _tileData = tileData;
        _maps = maps;
        _localization = localization;
        _multis = multis;
        _art = art;
        _skills = skills;
        _races = races;
        _bodies = bodies;
        _hues = hues;
        _radarColors = radarColors;
        _textures = textures;
        _expansions = expansions;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Information(
            "UO data ready: {Land} land tiles, {Item} item tiles, {Maps} maps, {Multis} multis, " +
            "{Cliloc} localized strings, {Skills} skills, {Races} races, {Bodies} bodies, art {Art}, " +
            "{Hues} hues, {RadarColors} radar colours, textures {Textures}, {Expansions} expansions",
            _tileData.LandTable.Count,
            _tileData.ItemTable.Count,
            _maps.Maps.Count,
            _multis.Count,
            _localization.Count,
            _skills.Count,
            _races.Races.Count,
            _bodies.Count,
            _art.IsValidArt(0) ? "available" : "absent",
            _hues.Count,
            _radarColors.Count,
            _textures.IsValidTexture(0) ? "available" : "absent",
            _expansions.Count
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

using System.Globalization;
using Moongate.Core.Geometry;
using Moongate.Core.Yaml;
using Moongate.Server.Data.World;
using Moongate.Server.Interfaces.Services.World;
using Moongate.Server.Types.World;
using Moongate.UO.Data.Data.ServerAssets;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Server.Services.WorldData;

/// <summary>
/// Loads server world data from bundled YAML asset models.
/// </summary>
public class ServerAssetDataLoader
{
    private const int MaxDoorPieces = 8;
    private const int FeluccaMapId = 0;
    private const int TrammelMapId = 1;
    private const int IlshenarMapId = 2;
    private const int MalasMapId = 3;
    private const int TokunoMapId = 4;
    private const int TermurMapId = 5;
    private const int InternalMapId = 0x7F;

    private readonly ILogger _logger = Log.ForContext<ServerAssetDataLoader>();
    private readonly string _dataDirectory;

    public ServerAssetDataLoader(string dataDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataDirectory);

        _dataDirectory = Path.GetFullPath(dataDirectory);
    }

    public void LoadCatalogs(
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
        ArgumentNullException.ThrowIfNull(teleportersDataService);
        ArgumentNullException.ThrowIfNull(regionDataService);
        ArgumentNullException.ThrowIfNull(weatherDataService);
        ArgumentNullException.ThrowIfNull(containerDataService);
        ArgumentNullException.ThrowIfNull(locationCatalogService);
        ArgumentNullException.ThrowIfNull(nameDataService);
        ArgumentNullException.ThrowIfNull(professionDataService);
        ArgumentNullException.ThrowIfNull(signDataService);
        ArgumentNullException.ThrowIfNull(decorationDataService);
        ArgumentNullException.ThrowIfNull(mountDataService);

        LoadTeleporters(teleportersDataService);
        LoadRegions(regionDataService);
        LoadWeather(weatherDataService);
        LoadContainers(containerDataService);
        LoadLocations(locationCatalogService);
        LoadNames(nameDataService);
        LoadProfessions(professionDataService);
        LoadSigns(signDataService);
        LoadDecorations(decorationDataService);
        LoadMounts(mountDataService);
    }

    public void LoadDoors(IDoorDataService doorDataService)
    {
        ArgumentNullException.ThrowIfNull(doorDataService);

        var doorsPath = Path.Combine(_dataDirectory, "components", "doors.yaml");

        if (!File.Exists(doorsPath))
        {
            _logger.Warning("Door asset file {Path} was not found; clearing door data", doorsPath);
            doorDataService.SetEntries([]);

            return;
        }

        var table = YamlUtils.DeserializeFromFile<ServerAssetDoorTable>(doorsPath);
        var entries = table.Door.Select(MapDoorDefinition).ToArray();

        doorDataService.SetEntries(entries);
    }

    public void LoadSpawns(ISpawnsDataService spawnsDataService)
    {
        ArgumentNullException.ThrowIfNull(spawnsDataService);

        var spawnsDirectory = Path.Combine(_dataDirectory, "spawns");

        if (!Directory.Exists(spawnsDirectory))
        {
            _logger.Warning("Spawn asset directory {Path} was not found; clearing spawn data", spawnsDirectory);
            spawnsDataService.SetEntries([]);

            return;
        }

        var entries = new List<SpawnDefinitionEntry>();
        var spawnFiles = Directory
                         .EnumerateFiles(spawnsDirectory, "*.yaml", SearchOption.AllDirectories)
                         .Select(
                             path => (
                                         FullPath: path,
                                         SourcePath: ToSourcePath(Path.GetRelativePath(spawnsDirectory, path))
                                     )
                         )
                         .OrderBy(file => file.SourcePath, StringComparer.OrdinalIgnoreCase)
                         .ThenBy(file => file.SourcePath, StringComparer.Ordinal)
                         .ToArray();

        foreach (var spawnFile in spawnFiles)
        {
            var sourceGroup = GetSourceGroup(spawnFile.SourcePath);
            var sourceFile = GetSourceFile(spawnFile.SourcePath);
            var table = YamlUtils.DeserializeFromFile<ServerAssetSpawnTable>(spawnFile.FullPath);

            foreach (var spawn in table.Spawn)
            {
                if (TryMapSpawnDefinition(spawn, sourceGroup, sourceFile, spawnFile.SourcePath, out var entry))
                {
                    entries.Add(entry);
                }
            }
        }

        spawnsDataService.SetEntries(entries);
    }

    public void LoadTeleporters(ITeleportersDataService teleportersDataService)
    {
        var teleportersDirectory = Path.Combine(_dataDirectory, "teleporters");

        if (!Directory.Exists(teleportersDirectory))
        {
            _logger.Warning(
                "Teleporter asset directory {Path} was not found; clearing teleporter data",
                teleportersDirectory
            );
            teleportersDataService.SetEntries([]);

            return;
        }

        var entries = new List<TeleporterEntry>();

        foreach (var teleporterFile in EnumerateYamlFiles(teleportersDirectory, SearchOption.AllDirectories))
        {
            var table = YamlUtils.DeserializeFromFile<ServerAssetTeleporterTable>(teleporterFile.FullPath);

            foreach (var definition in table.Teleporter)
            {
                if (TryMapTeleporterDefinition(definition, teleporterFile.SourcePath, out var entry))
                {
                    entries.Add(entry);
                }
            }
        }

        teleportersDataService.SetEntries(entries);
    }

    public void LoadRegions(IRegionDataService regionDataService)
    {
        var regionsDirectory = Path.Combine(_dataDirectory, "regions");

        if (!Directory.Exists(regionsDirectory))
        {
            _logger.Warning("Region asset directory {Path} was not found; clearing region data", regionsDirectory);
            regionDataService.SetEntries([]);

            return;
        }

        var entries = new List<RegionEntry>();

        foreach (var regionFile in EnumerateYamlFiles(regionsDirectory, SearchOption.TopDirectoryOnly))
        {
            var table = YamlUtils.DeserializeFromFile<ServerAssetRegionTable>(regionFile.FullPath);

            foreach (var definition in table.Region)
            {
                if (!TryResolveMap(definition.Map, out var mapId, out var canonicalMap))
                {
                    _logger.Warning(
                        "Skipping region {RegionName} from {SourcePath}: unsupported map {Map}",
                        definition.Name,
                        regionFile.SourcePath,
                        definition.Map
                    );

                    continue;
                }

                entries.Add(
                    new(
                        definition.Type,
                        mapId,
                        canonicalMap,
                        definition.Name,
                        definition.Priority,
                        definition.Area
                                  .Select(static area => new RegionAreaEntry(area.X1, area.Y1, area.X2, area.Y2))
                                  .ToArray(),
                        definition.Music,
                        ToPoint3D(definition.Entrance),
                        ToPoint3D(definition.GoLocation)
                    )
                );
            }
        }

        regionDataService.SetEntries(entries);
    }

    public void LoadWeather(IWeatherDataService weatherDataService)
    {
        var weatherDirectory = Path.Combine(_dataDirectory, "weather");

        if (!Directory.Exists(weatherDirectory))
        {
            _logger.Warning("Weather asset directory {Path} was not found; clearing weather data", weatherDirectory);
            weatherDataService.SetEntries([]);

            return;
        }

        var entries = new List<WeatherEntry>();

        foreach (var weatherFile in EnumerateYamlFiles(weatherDirectory, SearchOption.TopDirectoryOnly))
        {
            var table = YamlUtils.DeserializeFromFile<ServerAssetWeatherTable>(weatherFile.FullPath);

            entries.AddRange(table.WeatherType.Select(MapWeatherDefinition));
        }

        weatherDataService.SetEntries(entries);
    }

    public void LoadContainers(IContainerDataService containerDataService)
    {
        var containersPath = Path.Combine(_dataDirectory, "containers", "default_containers.yaml");
        var layoutsPath = Path.Combine(_dataDirectory, "containers", "containers.yaml");

        if (!File.Exists(containersPath))
        {
            _logger.Warning("Container asset file {Path} was not found; clearing container data", containersPath);
            containerDataService.SetContainers([]);
        }
        else
        {
            var table = YamlUtils.DeserializeFromFile<ServerAssetContainerTable>(containersPath);
            var entries = table.Container
                               .Select(
                                   static definition => new ContainerEntry(
                                       definition.Id,
                                       definition.ItemId,
                                       definition.Width,
                                       definition.Height,
                                       definition.Name
                                   )
                               )
                               .ToArray();

            containerDataService.SetContainers(entries);
        }

        if (!File.Exists(layoutsPath))
        {
            _logger.Warning("Container layout asset file {Path} was not found; clearing container layout data", layoutsPath);
            containerDataService.SetLayouts([]);
        }
        else
        {
            var table = YamlUtils.DeserializeFromFile<ServerAssetContainerLayoutTable>(layoutsPath);
            var entries = table.ContainerLayout
                               .Select(
                                   static definition => new ContainerLayoutEntry(
                                       definition.GumpId,
                                       definition.Bounds,
                                       definition.DropSound,
                                       definition.ItemIds
                                   )
                               )
                               .ToArray();

            containerDataService.SetLayouts(entries);
        }
    }

    public void LoadLocations(ILocationCatalogService locationCatalogService)
    {
        var locationsDirectory = Path.Combine(_dataDirectory, "locations");

        if (!Directory.Exists(locationsDirectory))
        {
            _logger.Warning(
                "Location asset directory {Path} was not found; clearing location catalog data",
                locationsDirectory
            );
            locationCatalogService.SetLocations([]);

            return;
        }

        var entries = new List<WorldLocationEntry>();

        foreach (var locationFile in EnumerateYamlFiles(locationsDirectory, SearchOption.TopDirectoryOnly))
        {
            var mapLocations = YamlUtils.DeserializeFromFile<ServerAssetMapLocations>(locationFile.FullPath);
            var fileMapName = Path.GetFileNameWithoutExtension(locationFile.FullPath);

            if (!TryResolveMap(fileMapName, out var mapId, out var canonicalMap) &&
                !TryResolveMap(mapLocations.Name, out mapId, out canonicalMap))
            {
                _logger.Warning(
                    "Skipping location file {SourcePath}: unsupported map file/name {MapFileName}/{MapName}",
                    locationFile.SourcePath,
                    fileMapName,
                    mapLocations.Name
                );

                continue;
            }

            var mapName = string.IsNullOrWhiteSpace(mapLocations.Name) ? canonicalMap : mapLocations.Name.Trim();

            AddLocationPoints(mapId, mapName, "", mapLocations.Locations, locationFile.SourcePath, entries);

            foreach (var category in mapLocations.Categories)
            {
                FlattenLocationCategory(mapId, mapName, category, "", locationFile.SourcePath, entries);
            }
        }

        locationCatalogService.SetLocations(entries);
    }

    public void LoadNames(INameDataService nameDataService)
    {
        var namesDirectory = Path.Combine(_dataDirectory, "names");

        if (!Directory.Exists(namesDirectory))
        {
            _logger.Warning("Name asset directory {Path} was not found; clearing name data", namesDirectory);
            nameDataService.SetGroups([]);

            return;
        }

        var groups = new List<NameGroupEntry>();

        foreach (var nameFile in EnumerateYamlFiles(namesDirectory, SearchOption.TopDirectoryOnly))
        {
            var table = YamlUtils.DeserializeFromFile<ServerAssetNameGroupTable>(nameFile.FullPath);
            groups.AddRange(table.NameGroup.Select(static group => new NameGroupEntry(group.Type, group.Names)));
        }

        nameDataService.SetGroups(groups);
    }

    public void LoadProfessions(IProfessionDataService professionDataService)
    {
        var professionsPath = Path.Combine(_dataDirectory, "Professions", "professions.yaml");

        if (!File.Exists(professionsPath))
        {
            _logger.Warning("Profession asset file {Path} was not found; clearing profession data", professionsPath);
            professionDataService.SetProfessions([]);

            return;
        }

        var table = YamlUtils.DeserializeFromFile<ServerAssetProfessionTable>(professionsPath);

        professionDataService.SetProfessions(table.Profession.Select(MapProfession).ToArray());
    }

    public void LoadSigns(ISignDataService signDataService)
    {
        var signsDirectory = Path.Combine(_dataDirectory, "signs");

        if (!Directory.Exists(signsDirectory))
        {
            _logger.Warning("Sign asset directory {Path} was not found; clearing sign data", signsDirectory);
            signDataService.SetEntries([]);

            return;
        }

        var entries = new List<SignEntry>();

        foreach (var signFile in EnumerateYamlFiles(signsDirectory, SearchOption.TopDirectoryOnly))
        {
            var table = YamlUtils.DeserializeFromFile<ServerAssetSignTable>(signFile.FullPath);

            foreach (var definition in table.Sign)
            {
                if (!TryResolveSignMapIds(definition.Map, out var mapIds))
                {
                    _logger.Warning(
                        "Skipping sign from {SourcePath}: unsupported source map code {MapCode}",
                        signFile.SourcePath,
                        definition.Map
                    );

                    continue;
                }

                if (!TryParsePoint3D(definition.Location, out var location))
                {
                    _logger.Warning(
                        "Skipping sign from {SourcePath}: location must have at least three coordinates",
                        signFile.SourcePath
                    );

                    continue;
                }

                foreach (var mapId in mapIds)
                {
                    entries.Add(new(mapId, definition.Map, definition.ItemId, location, definition.Text));
                }
            }
        }

        signDataService.SetEntries(entries);
    }

    public void LoadDecorations(IDecorationDataService decorationDataService)
    {
        var decorationDirectory = Path.Combine(_dataDirectory, "decoration");

        if (!Directory.Exists(decorationDirectory))
        {
            _logger.Warning(
                "Decoration asset directory {Path} was not found; clearing decoration data",
                decorationDirectory
            );
            decorationDataService.SetEntries([]);

            return;
        }

        var entries = new List<DecorationEntry>();

        foreach (var decorationFile in EnumerateYamlFiles(decorationDirectory, SearchOption.AllDirectories))
        {
            var sourceGroup = GetSourceGroup(decorationFile.SourcePath);
            var sourceFile = GetSourceFile(decorationFile.SourcePath);
            var mapGroup = GetFirstSourceSegment(decorationFile.SourcePath);

            if (!TryResolveDecorationMapIds(mapGroup, out var mapIds))
            {
                _logger.Warning(
                    "Skipping decoration file {SourcePath}: unsupported decoration group {Group}",
                    decorationFile.SourcePath,
                    mapGroup
                );

                continue;
            }

            var table = YamlUtils.DeserializeFromFile<ServerAssetDecorationTable>(decorationFile.FullPath);

            foreach (var definition in table.Decoration)
            {
                var itemId = ToItemId(definition.ItemId);
                var parameters = ToDecorationParameters(definition.Arguments);

                foreach (var placement in definition.Placements)
                {
                    if (!TryParsePoint3D(placement.Location, out var location))
                    {
                        _logger.Warning(
                            "Skipping decoration placement from {SourcePath}: location must have at least three coordinates",
                            decorationFile.SourcePath
                        );

                        continue;
                    }

                    var target = TryParsePoint3D(placement.Target, out var placementTarget)
                                     ? placementTarget
                                     : (Point3D?)null;

                    foreach (var mapId in mapIds)
                    {
                        entries.Add(
                            new(
                                mapId,
                                sourceGroup,
                                sourceFile,
                                definition.Type,
                                definition.Description,
                                itemId,
                                parameters,
                                location,
                                target,
                                placement.Note
                            )
                        );
                    }
                }
            }
        }

        decorationDataService.SetEntries(entries);
    }

    public void LoadMounts(IMountDataService mountDataService)
    {
        var conversionsPath = Path.Combine(_dataDirectory, "support", "uoconvert.yaml");

        if (!File.Exists(conversionsPath))
        {
            _logger.Warning("Conversion asset file {Path} was not found; clearing mount data", conversionsPath);
            mountDataService.SetEntries([]);

            return;
        }

        var table = YamlUtils.DeserializeFromFile<ServerAssetConversionTable>(conversionsPath);
        var mountsSection = table.ConversionSection.FirstOrDefault(
            static section => section.Name.Equals("Mounts", StringComparison.OrdinalIgnoreCase)
        );
        var tilesEntry = mountsSection?.Entries.FirstOrDefault(
            static entry => entry.Name.Equals("Tiles", StringComparison.OrdinalIgnoreCase)
        );

        if (tilesEntry is null)
        {
            _logger.Warning(
                "Conversion asset file {Path} did not contain a Mounts/Tiles entry; clearing mount data",
                conversionsPath
            );
            mountDataService.SetEntries([]);

            return;
        }

        var itemIds = new HashSet<int>();

        foreach (var value in tilesEntry.Values)
        {
            if (TryParseInt(value, out var itemId))
            {
                itemIds.Add(itemId);

                continue;
            }

            _logger.Warning("Skipping invalid mount tile id {Value} from {Path}", value, conversionsPath);
        }

        mountDataService.SetEntries(itemIds);
    }

    private static DoorComponentEntry MapDoorDefinition(ServerAssetDoorDefinition definition)
    {
        var pieces = definition.Pieces;

        return new(
            definition.Category,
            GetDoorPiece(pieces, 0),
            GetDoorPiece(pieces, 1),
            GetDoorPiece(pieces, 2),
            GetDoorPiece(pieces, 3),
            GetDoorPiece(pieces, 4),
            GetDoorPiece(pieces, 5),
            GetDoorPiece(pieces, 6),
            GetDoorPiece(pieces, 7),
            definition.FeatureMask,
            definition.Comment
        );
    }

    private static ProfessionEntry MapProfession(ServerAssetProfession profession)
    {
        return new(
            profession.Name,
            profession.TrueName,
            profession.NameId,
            profession.DescId,
            profession.Desc,
            profession.TopLevel,
            profession.Gump,
            profession.Type,
            profession.Skills
                      .Select(static skill => new ProfessionSkillEntry(skill.Name, skill.Value))
                      .ToArray(),
            profession.Stats
                      .Select(static stat => new ProfessionStatEntry(stat.Type, stat.Value))
                      .ToArray()
        );
    }

    private static WeatherEntry MapWeatherDefinition(ServerAssetWeatherDefinition definition)
    {
        return new(
            definition.Id,
            definition.Name,
            definition.Rainchance,
            ToWeatherRange(definition.Rainintensity),
            definition.Raintempdrop,
            definition.Snowchance,
            ToWeatherRange(definition.Snowintensity),
            definition.Snowthreshold,
            definition.Stormchance,
            ToWeatherRange(definition.Stormintensity),
            definition.Stormtempdrop,
            definition.Maxtemp,
            definition.Mintemp,
            definition.Coldchance,
            definition.Coldintensity,
            definition.Heatchance,
            definition.Heatintensity,
            definition.Lightmin,
            definition.Lightmax
        );
    }

    private static WeatherRange ToWeatherRange(ServerAssetRange range)
    {
        return new(range.Min, range.Max);
    }

    private bool TryMapTeleporterDefinition(
        ServerAssetTeleporterDefinition definition,
        string relativeFilePath,
        out TeleporterEntry entry
    )
    {
        entry = default;

        if (!TryResolveMap(definition.Src.Map, out var sourceMapId, out var sourceMapName) ||
            !TryResolveMap(definition.Dst.Map, out var destinationMapId, out var destinationMapName))
        {
            _logger.Warning(
                "Skipping teleporter from {SourcePath}: unsupported src/dst map {SourceMap} -> {DestinationMap}",
                relativeFilePath,
                definition.Src.Map,
                definition.Dst.Map
            );

            return false;
        }

        if (!TryParsePoint3D(definition.Src.Loc, out var sourceLocation) ||
            !TryParsePoint3D(definition.Dst.Loc, out var destinationLocation))
        {
            _logger.Warning(
                "Skipping teleporter from {SourcePath}: src/dst location must have at least three coordinates",
                relativeFilePath
            );

            return false;
        }

        entry = new(
            sourceMapId,
            sourceMapName,
            sourceLocation,
            destinationMapId,
            destinationMapName,
            destinationLocation,
            definition.Back
        );

        return true;
    }

    private bool TryMapSpawnDefinition(
        ServerAssetSpawnDefinition definition,
        string sourceGroup,
        string sourceFile,
        string relativeFilePath,
        out SpawnDefinitionEntry entry
    )
    {
        entry = default;

        var mapName = ResolveMapName(definition, sourceGroup);

        if (!TryResolveMap(mapName, out var mapId, out var canonicalMap))
        {
            _logger.Warning(
                "Skipping spawn {SpawnGuid} from {SourcePath}: unsupported map {Map}",
                definition.Guid,
                relativeFilePath,
                mapName
            );

            return false;
        }

        if (definition.Location.Count < 3)
        {
            _logger.Warning(
                "Skipping spawn {SpawnGuid} from {SourcePath}: location must have at least three coordinates",
                definition.Guid,
                relativeFilePath
            );

            return false;
        }

        if (!Guid.TryParse(definition.Guid, out var guid))
        {
            _logger.Warning(
                "Skipping spawn {SpawnGuid} from {SourcePath}: invalid GUID",
                definition.Guid,
                relativeFilePath
            );

            return false;
        }

        entry = new(
            mapId,
            canonicalMap,
            sourceGroup,
            sourceFile,
            guid,
            ResolveKind(definition.Type),
            definition.Name,
            new Point3D(definition.Location[0], definition.Location[1], definition.Location[2]),
            definition.Count,
            definition.MinDelay,
            definition.MaxDelay,
            definition.Team,
            definition.HomeRange,
            definition.WalkingRange,
            definition.Entries
                      .Select(
                          static spawnEntry => new SpawnEntryDefinition(
                              spawnEntry.Name,
                              spawnEntry.MaxCount,
                              spawnEntry.Probability
                          )
                      )
                      .ToArray()
        );

        return true;
    }

    private static int GetDoorPiece(IReadOnlyList<int> pieces, int index)
    {
        return index < MaxDoorPieces && index < pieces.Count ? pieces[index] : 0;
    }

    private static string ResolveMapName(ServerAssetSpawnDefinition definition, string sourceGroup)
    {
        if (!string.IsNullOrWhiteSpace(definition.Map))
        {
            return definition.Map.Trim();
        }

        return InferMapName(sourceGroup);
    }

    private static string InferMapName(string sourceGroup)
    {
        if (string.IsNullOrWhiteSpace(sourceGroup))
        {
            return "";
        }

        var segments = sourceGroup.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0)
        {
            return "";
        }

        foreach (var segment in segments)
        {
            if (TryResolveMap(segment, out _, out var canonicalMap))
            {
                return canonicalMap;
            }
        }

        return "";
    }

    private static bool TryResolveMap(string mapName, out int mapId, out string canonicalMap)
    {
        if (string.IsNullOrWhiteSpace(mapName))
        {
            mapId = 0;
            canonicalMap = "";

            return false;
        }

        var trimmedMapName = mapName.Trim();

        if (trimmedMapName.Equals("felucca", StringComparison.OrdinalIgnoreCase))
        {
            mapId = FeluccaMapId;
            canonicalMap = "felucca";

            return true;
        }

        if (trimmedMapName.Equals("trammel", StringComparison.OrdinalIgnoreCase))
        {
            mapId = TrammelMapId;
            canonicalMap = "trammel";

            return true;
        }

        if (trimmedMapName.Equals("ilshenar", StringComparison.OrdinalIgnoreCase))
        {
            mapId = IlshenarMapId;
            canonicalMap = "ilshenar";

            return true;
        }

        if (trimmedMapName.Equals("malas", StringComparison.OrdinalIgnoreCase))
        {
            mapId = MalasMapId;
            canonicalMap = "malas";

            return true;
        }

        if (trimmedMapName.Equals("tokuno", StringComparison.OrdinalIgnoreCase))
        {
            mapId = TokunoMapId;
            canonicalMap = "tokuno";

            return true;
        }

        if (trimmedMapName.Equals("termur", StringComparison.OrdinalIgnoreCase))
        {
            mapId = TermurMapId;
            canonicalMap = "termur";

            return true;
        }

        if (trimmedMapName.Equals("internal", StringComparison.OrdinalIgnoreCase))
        {
            mapId = InternalMapId;
            canonicalMap = "internal";

            return true;
        }

        mapId = 0;
        canonicalMap = "";

        return false;
    }

    private static SpawnDefinitionKind ResolveKind(string type)
    {
        return type.Equals("ProximitySpawner", StringComparison.OrdinalIgnoreCase)
                   ? SpawnDefinitionKind.ProximitySpawner
                   : SpawnDefinitionKind.Spawner;
    }

    private void FlattenLocationCategory(
        int mapId,
        string mapName,
        ServerAssetLocationCategory category,
        string parentPath,
        string sourcePath,
        List<WorldLocationEntry> output
    )
    {
        var categoryName = category.Name.Trim();
        var categoryPath = string.IsNullOrWhiteSpace(parentPath) ? categoryName :
                           string.IsNullOrWhiteSpace(categoryName) ? parentPath : $"{parentPath} / {categoryName}";

        AddLocationPoints(mapId, mapName, categoryPath, category.Locations, sourcePath, output);

        foreach (var childCategory in category.Categories)
        {
            FlattenLocationCategory(mapId, mapName, childCategory, categoryPath, sourcePath, output);
        }
    }

    private void AddLocationPoints(
        int mapId,
        string mapName,
        string categoryPath,
        IReadOnlyList<ServerAssetLocationPoint> locations,
        string sourcePath,
        List<WorldLocationEntry> output
    )
    {
        foreach (var location in locations)
        {
            if (!TryParsePoint3D(location.Location, out var point))
            {
                _logger.Warning(
                    "Skipping location {LocationName} from {SourcePath}: location must have at least three coordinates",
                    location.Name,
                    sourcePath
                );

                continue;
            }

            output.Add(new(mapId, mapName, categoryPath, location.Name, point));
        }
    }

    private static IReadOnlyList<(string FullPath, string SourcePath)> EnumerateYamlFiles(
        string directory,
        SearchOption searchOption
    )
    {
        return Directory
               .EnumerateFiles(directory, "*.yaml", searchOption)
               .Select(
                   path => (
                               FullPath: path,
                               SourcePath: ToSourcePath(Path.GetRelativePath(directory, path))
                           )
               )
               .OrderBy(file => file.SourcePath, StringComparer.OrdinalIgnoreCase)
               .ThenBy(file => file.SourcePath, StringComparer.Ordinal)
               .ToArray();
    }

    private static Point3D? ToPoint3D(ServerAssetWorldPoint? point)
    {
        return point is null ? null : new Point3D(point.X, point.Y, point.Z);
    }

    private static bool TryParsePoint3D(IReadOnlyList<int> coordinates, out Point3D location)
    {
        if (coordinates.Count < 3)
        {
            location = Point3D.Zero;

            return false;
        }

        location = new(coordinates[0], coordinates[1], coordinates[2]);

        return true;
    }

    private static bool TryResolveSignMapIds(int sourceMapCode, out IReadOnlyList<int> mapIds)
    {
        mapIds = sourceMapCode switch
        {
            0 => [FeluccaMapId, TrammelMapId],
            1 => [FeluccaMapId],
            2 => [TrammelMapId],
            3 => [IlshenarMapId],
            4 => [MalasMapId],
            5 => [TokunoMapId],
            6 => [TermurMapId],
            _ => []
        };

        return mapIds.Count > 0;
    }

    private static bool TryResolveDecorationMapIds(string groupName, out IReadOnlyList<int> mapIds)
    {
        mapIds = groupName switch
        {
            var value when value.Equals("Britannia", StringComparison.OrdinalIgnoreCase) =>
                [FeluccaMapId, TrammelMapId],
            var value when value.Equals("Felucca", StringComparison.OrdinalIgnoreCase) =>
                [FeluccaMapId],
            var value when value.Equals("Trammel", StringComparison.OrdinalIgnoreCase) =>
                [TrammelMapId],
            var value when value.Equals("Ilshenar", StringComparison.OrdinalIgnoreCase) =>
                [IlshenarMapId],
            var value when value.Equals("Malas", StringComparison.OrdinalIgnoreCase) =>
                [MalasMapId],
            var value when value.Equals("Tokuno", StringComparison.OrdinalIgnoreCase) =>
                [TokunoMapId],
            var value when value.Equals("Termur", StringComparison.OrdinalIgnoreCase) =>
                [TermurMapId],
            var value when value.Equals("RuinedMaginciaFel", StringComparison.OrdinalIgnoreCase) =>
                [FeluccaMapId],
            var value when value.Equals("RuinedMaginciaTram", StringComparison.OrdinalIgnoreCase) =>
                [TrammelMapId],
            _ => []
        };

        return mapIds.Count > 0;
    }

    private static IReadOnlyDictionary<string, string> ToDecorationParameters(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["arguments"] = arguments
        };
    }

    private static int ToItemId(int? value)
    {
        return value is null or <= 0 ? 0 : value.Value;
    }

    private static bool TryParseInt(string value, out int parsed)
    {
        parsed = 0;
        var trimmed = value.Trim();

        if (trimmed.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return int.TryParse(trimmed.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out parsed);
        }

        return int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed);
    }

    private static string GetSourceGroup(string relativeFilePath)
    {
        var separatorIndex = relativeFilePath.LastIndexOf('/');

        if (separatorIndex <= 0)
        {
            return "";
        }

        return relativeFilePath[..separatorIndex];
    }

    private static string GetSourceFile(string relativeFilePath)
    {
        var segments = relativeFilePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0)
        {
            return "";
        }

        return segments[^1];
    }

    private static string GetFirstSourceSegment(string relativeFilePath)
    {
        var segments = relativeFilePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0)
        {
            return "";
        }

        return segments[0];
    }

    private static string ToSourcePath(string path)
    {
        return path
               .Replace(Path.DirectorySeparatorChar, '/')
               .Replace(Path.AltDirectorySeparatorChar, '/');
    }
}

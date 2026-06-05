using Moongate.UO.Data.Data.Maps;
using Moongate.UO.Data.Interfaces.Files;
using Moongate.UO.Data.Interfaces.Maps;
using Moongate.UO.Data.Types.Maps;

namespace Moongate.UO.Data.Maps;

/// <summary>
/// Registry of the standard UO map facets, each backed by a lazily-loaded <see cref="Map" />.
/// </summary>
public sealed class MapService : IMapService
{
    private static readonly MapDefinition[] _definitions =
    [
        new MapDefinition(0, 0, 0, 7168, 4096, "Felucca", MapRulesType.FeluccaRules),
        new MapDefinition(1, 1, 1, 7168, 4096, "Trammel", MapRulesType.TrammelRules),
        new MapDefinition(2, 2, 2, 2304, 1600, "Ilshenar", MapRulesType.TrammelRules),
        new MapDefinition(3, 3, 3, 2560, 2048, "Malas", MapRulesType.TrammelRules),
        new MapDefinition(4, 4, 4, 1448, 1448, "Tokuno", MapRulesType.TrammelRules),
        new MapDefinition(5, 5, 5, 1280, 4096, "TerMur", MapRulesType.TrammelRules)
    ];

    private readonly Dictionary<int, Map> _maps;
    private readonly Map[] _all;

    public MapService(IUoFileResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);

        _all = new Map[_definitions.Length];
        _maps = new Dictionary<int, Map>(_definitions.Length);

        for (var i = 0; i < _definitions.Length; i++)
        {
            var map = new Map(_definitions[i], resolver);
            _all[i] = map;
            _maps[map.MapId] = map;
        }
    }

    public IReadOnlyList<Map> Maps => _all;

    public Map? GetMap(int mapId)
    {
        return _maps.GetValueOrDefault(mapId);
    }
}

using Moongate.Core.Yaml;
using Moongate.UO.Data.Data.Internal;
using Moongate.UO.Data.Data.Races;
using Moongate.UO.Data.Interfaces.Races;
using Serilog;

namespace Moongate.UO.Data.Races;

/// <summary>
/// Loads the UO race definitions from <c>races.yaml</c> in the data directory. A missing or
/// malformed file yields an empty store (non-fatal).
/// </summary>
public sealed class RaceStore : IRaceStore
{
    private static readonly ILogger _logger = Log.ForContext<RaceStore>();

    private readonly List<RaceDefinition> _races;
    private readonly Dictionary<int, RaceDefinition> _byId;

    public RaceStore(string dataDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataDirectory);

        _races = [];
        _byId = new();

        var path = Path.Combine(dataDirectory, "races.yaml");

        if (!File.Exists(path))
        {
            _logger.Warning("races.yaml not found in {Directory}; race table is empty.", dataDirectory);

            return;
        }

        try
        {
            var model = YamlUtils.DeserializeFromFile<RaceTableModel>(path);
            _races = model.Race;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to parse races.yaml; race table is empty.");

            return;
        }

        foreach (var race in _races)
        {
            _byId[race.Id] = race;
        }

        _logger.Information("Loaded {Count} races from {Path}", _races.Count, path);
    }

    public IReadOnlyList<RaceDefinition> Races => _races;

    public RaceDefinition? GetById(int raceId)
        => _byId.GetValueOrDefault(raceId);
}

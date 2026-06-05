using Moongate.Abstractions.Configuration;
using Moongate.UO.Data.Data.Internal;
using Moongate.UO.Data.Interfaces.Bodies;
using Moongate.UO.Data.Types.Bodies;
using Serilog;
using Tomlyn;

namespace Moongate.UO.Data.Bodies;

/// <summary>
/// Loads the body-id classification table from <c>bodies.toml</c> in the data directory. A missing
/// or malformed file yields an empty table (non-fatal).
/// </summary>
public sealed class BodyDataStore : IBodyDataStore
{
    private const int TableSize = 0x1000;

    private static readonly ILogger _logger = Log.ForContext<BodyDataStore>();

    private readonly UoBodyType[] _types;

    public BodyDataStore(string dataDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataDirectory);

        _types = new UoBodyType[TableSize];

        var path = Path.Combine(dataDirectory, "bodies.toml");

        if (!File.Exists(path))
        {
            _logger.Warning("bodies.toml not found in {Directory}; body table is empty.", dataDirectory);

            return;
        }

        BodyTableModel model;

        try
        {
            model = TomlSerializer.Deserialize<BodyTableModel>(File.ReadAllText(path), ConfigTomlOptions.Instance);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to parse bodies.toml; body table is empty.");

            return;
        }

        Apply(model.Bodies.Monster, UoBodyType.Monster);
        Apply(model.Bodies.Sea, UoBodyType.Sea);
        Apply(model.Bodies.Animal, UoBodyType.Animal);
        Apply(model.Bodies.Human, UoBodyType.Human);
        Apply(model.Bodies.Equipment, UoBodyType.Equipment);

        _logger.Information("Loaded {Count} body classifications from {Path}", Count, path);
    }

    public int Count
    {
        get
        {
            var count = 0;

            for (var i = 0; i < _types.Length; i++)
            {
                if (_types[i] != UoBodyType.Empty)
                {
                    count++;
                }
            }

            return count;
        }
    }

    public UoBodyType GetBodyType(int bodyId)
        => bodyId >= 0 && bodyId < _types.Length ? _types[bodyId] : UoBodyType.Empty;

    private void Apply(List<int> ids, UoBodyType type)
    {
        foreach (var id in ids)
        {
            if (id >= 0 && id < _types.Length)
            {
                _types[id] = type;
            }
        }
    }
}

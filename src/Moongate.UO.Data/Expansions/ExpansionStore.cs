using Moongate.Core.Yaml;
using Moongate.UO.Data.Data.Expansions;
using Moongate.UO.Data.Data.Internal;
using Moongate.UO.Data.Interfaces.Expansions;
using Moongate.UO.Data.Types.Expansions;
using Serilog;

namespace Moongate.UO.Data.Expansions;

/// <summary>
/// Loads the UO expansion table from <c>expansions.yaml</c> in the data directory. A missing or
/// malformed file yields an empty store (non-fatal).
/// </summary>
public sealed class ExpansionStore : IExpansionStore
{
    private static readonly ILogger _logger = Log.ForContext<ExpansionStore>();

    private readonly List<ExpansionInfo> _table;
    private readonly Dictionary<int, ExpansionInfo> _byId;

    public ExpansionStore(string dataDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataDirectory);

        _table = [];
        _byId = new();

        var path = Path.Combine(dataDirectory, "expansions.yaml");

        if (!File.Exists(path))
        {
            _logger.Warning("expansions.yaml not found in {Directory}; expansion table is empty.", dataDirectory);

            return;
        }

        try
        {
            var model = YamlUtils.DeserializeFromFile<ExpansionTableModel>(path);
            _table = model.Expansion;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to parse expansions.yaml; expansion table is empty.");

            return;
        }

        foreach (var info in _table)
        {
            _byId[info.Id] = info;
        }

        _logger.Information("Loaded {Count} expansions from {Path}", _table.Count, path);
    }

    public IReadOnlyList<ExpansionInfo> Table => _table;

    public int Count => _table.Count;

    public ExpansionInfo? Core
    {
        get
        {
            ExpansionInfo? core = null;

            foreach (var info in _table)
            {
                if (core is null || info.Id > core.Id)
                {
                    core = info;
                }
            }

            return core;
        }
    }

    public ExpansionInfo? GetInfo(int id)
        => _byId.GetValueOrDefault(id);

    public ExpansionInfo? GetInfo(UoExpansionType expansion)
        => GetInfo((int)expansion);
}

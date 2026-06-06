using Moongate.Server.Data.World;
using Moongate.Server.Interfaces.Services.World;

namespace Moongate.Server.Services.World;

/// <summary>
/// In-memory store for name groups loaded at startup.
/// </summary>
public class NameDataService : INameDataService
{
    private readonly object _sync = new();
    private List<NameGroupEntry> _groups = [];

    public IReadOnlyList<NameGroupEntry> GetAllGroups()
    {
        lock (_sync)
        {
            return [.. _groups];
        }
    }

    public void SetGroups(IReadOnlyList<NameGroupEntry> groups)
    {
        ArgumentNullException.ThrowIfNull(groups);

        var snapshot = groups.ToList();

        lock (_sync)
        {
            _groups = snapshot;
        }
    }
}

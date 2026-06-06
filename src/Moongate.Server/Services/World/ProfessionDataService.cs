using Moongate.Server.Data.World;
using Moongate.Server.Interfaces.Services.World;

namespace Moongate.Server.Services.World;

/// <summary>
/// In-memory store for profession definitions loaded at startup.
/// </summary>
public class ProfessionDataService : IProfessionDataService
{
    private readonly object _sync = new();
    private List<ProfessionEntry> _professions = [];

    public IReadOnlyList<ProfessionEntry> GetAllProfessions()
    {
        lock (_sync)
        {
            return [.. _professions];
        }
    }

    public void SetProfessions(IReadOnlyList<ProfessionEntry> professions)
    {
        ArgumentNullException.ThrowIfNull(professions);

        var snapshot = professions.ToList();

        lock (_sync)
        {
            _professions = snapshot;
        }
    }
}

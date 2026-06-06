using Moongate.Server.Data.World;
using Moongate.Server.Interfaces.Services.World;
using Moongate.Server.Services.World.Internal;
using Moongate.Server.Services.WorldData;

namespace Moongate.Server.Services.World;

/// <summary>
/// Lazy in-memory store for profession definitions.
/// </summary>
public class ProfessionDataService : LazyDataService, IProfessionDataService
{
    private readonly ServerAssetDataLoader? _loader;
    private readonly Lock _sync = new();
    private List<ProfessionEntry> _professions = [];

    public ProfessionDataService() { }

    public ProfessionDataService(ServerAssetDataLoader loader)
    {
        ArgumentNullException.ThrowIfNull(loader);

        _loader = loader;
    }

    public IReadOnlyList<ProfessionEntry> GetAllProfessions()
    {
        EnsureLoaded();

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

        MarkLoaded();
    }

    protected override void LoadCore()
        => _loader?.LoadProfessions(this);
}

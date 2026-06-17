using Moongate.Server.Data.World;
using Moongate.Server.Interfaces.Services.World;
using Moongate.Server.Services.World.Internal;
using Moongate.Server.Services.WorldData;

namespace Moongate.Server.Services.World;

/// <summary>
///     Lazy in-memory location catalog populated by server asset data.
/// </summary>
public class LocationCatalogService : LazyDataService, ILocationCatalogService
{
    private readonly ServerAssetDataLoader? _loader;
    private readonly Lock _sync = new();
    private List<WorldLocationEntry> _locations = [];

    public LocationCatalogService()
    {
    }

    public LocationCatalogService(ServerAssetDataLoader loader)
    {
        ArgumentNullException.ThrowIfNull(loader);

        _loader = loader;
    }

    public IReadOnlyList<WorldLocationEntry> GetAllLocations()
    {
        EnsureLoaded();

        lock (_sync)
        {
            return [.. _locations];
        }
    }

    public void SetLocations(IReadOnlyList<WorldLocationEntry> locations)
    {
        ArgumentNullException.ThrowIfNull(locations);

        var snapshot = locations.ToList();

        lock (_sync)
        {
            _locations = snapshot;
        }

        MarkLoaded();
    }

    protected override void LoadCore()
    {
        _loader?.LoadLocations(this);
    }
}

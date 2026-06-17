using Moongate.Core.Ids;
using Moongate.Server.Interfaces.Services.World;
using Moongate.UO.Data.Entities.Mobiles;

namespace Moongate.Server.Services.World;

/// <summary>
/// Thread-safe in-memory registry of live in-world mobiles.
/// </summary>
public sealed class WorldMobileRegistry : IWorldMobileRegistry
{
    private readonly Lock _sync = new();
    private readonly Dictionary<Serial, MobileEntity> _mobiles = [];

    public IReadOnlyCollection<MobileEntity> All
    {
        get
        {
            lock (_sync)
            {
                return _mobiles.Values.ToArray();
            }
        }
    }

    public void Add(MobileEntity mobile)
    {
        ArgumentNullException.ThrowIfNull(mobile);

        lock (_sync)
        {
            _mobiles[mobile.Id] = mobile;
        }
    }

    public bool TryGet(Serial id, out MobileEntity mobile)
    {
        lock (_sync)
        {
            return _mobiles.TryGetValue(id, out mobile!);
        }
    }

    public bool Remove(Serial id)
    {
        lock (_sync)
        {
            return _mobiles.Remove(id);
        }
    }
}

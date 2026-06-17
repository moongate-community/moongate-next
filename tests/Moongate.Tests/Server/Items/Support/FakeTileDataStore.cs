using Moongate.UO.Data.Data.Tiles;
using Moongate.UO.Data.Interfaces.Tiles;
using Moongate.UO.Data.Types.Tiles;

namespace Moongate.Tests.Server.Items.Support;

public sealed class FakeTileDataStore : ITileDataStore
{
    private readonly HashSet<int> _containers = [];
    private readonly HashSet<int> _doors = [];

    public IReadOnlyList<LandData> LandTable => [];
    public IReadOnlyList<ItemData> ItemTable => [];

    public ItemData GetItem(int id)
    {
        var flags = UoTileFlag.None;

        if (_containers.Contains(id))
        {
            flags |= UoTileFlag.Container;
        }

        if (_doors.Contains(id))
        {
            flags |= UoTileFlag.Door;
        }

        return new ItemData { Flags = flags };
    }

    public LandData GetLand(int id)
    {
        return default;
    }

    public void Container(int itemId)
    {
        _containers.Add(itemId);
    }

    public void MakeDoor(int itemId)
    {
        _doors.Add(itemId);
    }
}

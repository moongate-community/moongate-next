using Moongate.UO.Data.Data.Tiles;
using Moongate.UO.Data.Interfaces.Tiles;
using Moongate.UO.Data.Types.Tiles;

namespace Moongate.Tests.Support;

public sealed class TestTileDataStore : ITileDataStore
{
    private readonly Dictionary<int, ItemData> _items = [];

    public TestTileDataStore() { }

    public TestTileDataStore(params (int Id, string Name)[] items)
    {
        foreach (var (id, name) in items)
        {
            _items[id] = CreateItem(name);
        }
    }

    public IReadOnlyList<LandData> LandTable => [];

    public IReadOnlyList<ItemData> ItemTable => [];

    public ItemData GetItem(int id)
        => _items.TryGetValue(id, out var item) ? item : CreateItem("");

    public LandData GetLand(int id)
        => default;

    private static ItemData CreateItem(string name)
        => new(name, UoTileFlag.None, 0, 0, 0, 0, 0, 0);
}

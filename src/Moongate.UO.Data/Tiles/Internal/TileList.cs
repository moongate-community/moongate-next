using System.Buffers;
using Moongate.UO.Data.Data.Tiles;

namespace Moongate.UO.Data.Tiles.Internal;

/// <summary>
/// Growable accumulator of <see cref="StaticTile" /> used while bucketing a statics block.
/// Backed by a pooled array; <see cref="ToArray" /> returns the pooled buffer to the pool.
/// </summary>
public sealed class TileList
{
    private static readonly StaticTile[] _emptyTiles = [];

    private StaticTile[]? _tiles;

    public int Count { get; private set; }

    public void Add(StaticTile tile)
    {
        TryResize(1);
        _tiles![Count++] = tile;
    }

    public StaticTile[] ToArray()
    {
        if (Count == 0)
        {
            return _emptyTiles;
        }

        var tiles = new StaticTile[Count];
        _tiles.AsSpan(0, Count).CopyTo(tiles);

        ArrayPool<StaticTile>.Shared.Return(_tiles!);
        _tiles = null;
        Count = 0;

        return tiles;
    }

    private void TryResize(int length)
    {
        _tiles ??= ArrayPool<StaticTile>.Shared.Rent(length);

        var newLength = Count + length;

        if (newLength > _tiles.Length)
        {
            var old = _tiles;
            _tiles = ArrayPool<StaticTile>.Shared.Rent(newLength);
            old.AsSpan(0, Count).CopyTo(_tiles);
            ArrayPool<StaticTile>.Shared.Return(old);
        }
    }
}

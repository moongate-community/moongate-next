using Moongate.Core.Geometry;
using Moongate.UO.Data.Data.Tiles;
using Moongate.UO.Data.Tiles.Internal;
using Moongate.UO.Data.Types.Tiles;

namespace Moongate.UO.Data.Data.Multi;

/// <summary>
///     A parsed multi (house, boat, structure): its component entries, bucketed static tiles, and
///     bounding geometry. Read-only — built once from client data.
/// </summary>
public sealed class MultiComponentList
{
    public static readonly MultiComponentList Empty = new();

    private MultiComponentList()
    {
        Tiles = [];
        List = [];
    }

    public MultiComponentList(BinaryReader reader, int length, bool postHsFormat)
    {
        var count = length / (postHsFormat ? 16 : 12);
        var allTiles = List = new MultiTileEntry[count];

        var minX = 0;
        var minY = 0;
        var maxX = 0;
        var maxY = 0;

        for (var i = 0; i < count; ++i)
        {
            allTiles[i].ItemId = reader.ReadUInt16();
            allTiles[i].OffsetX = reader.ReadInt16();
            allTiles[i].OffsetY = reader.ReadInt16();
            allTiles[i].OffsetZ = reader.ReadInt16();
            allTiles[i].Flags = postHsFormat ? (UoTileFlag)reader.ReadUInt64() : (UoTileFlag)reader.ReadUInt32();

            var e = allTiles[i];

            if (i == 0 || e.Flags != 0)
            {
                minX = Math.Min(minX, e.OffsetX);
                minY = Math.Min(minY, e.OffsetY);
                maxX = Math.Max(maxX, e.OffsetX);
                maxY = Math.Max(maxY, e.OffsetY);
            }
        }

        BuildTiles(allTiles, minX, minY, maxX, maxY);
    }

    public MultiComponentList(List<MultiTileEntry> list)
    {
        var allTiles = List = list.ToArray();

        var minX = 0;
        var minY = 0;
        var maxX = 0;
        var maxY = 0;

        for (var i = 0; i < allTiles.Length; ++i)
        {
            var e = allTiles[i];

            if (i == 0 || e.Flags != 0)
            {
                minX = Math.Min(minX, e.OffsetX);
                minY = Math.Min(minY, e.OffsetY);
                maxX = Math.Max(maxX, e.OffsetX);
                maxY = Math.Max(maxY, e.OffsetY);
            }
        }

        BuildTiles(allTiles, minX, minY, maxX, maxY);
    }

    public Point2D Min { get; private set; }

    public Point2D Max { get; private set; }

    public Point2D Center { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public StaticTile[][][] Tiles { get; private set; }

    public MultiTileEntry[] List { get; private set; }

    private void BuildTiles(MultiTileEntry[] allTiles, int minX, int minY, int maxX, int maxY)
    {
        Min = new Point2D(minX, minY);
        Max = new Point2D(maxX, maxY);
        Center = new Point2D(-minX, -minY);
        Width = maxX - minX + 1;
        Height = maxY - minY + 1;

        var tiles = new TileList[Width][];
        Tiles = new StaticTile[Width][][];

        for (var i = 0; i < allTiles.Length; ++i)
        {
            if (i == 0 || allTiles[i].Flags != 0)
            {
                var xOffset = allTiles[i].OffsetX + Center.X;
                var yOffset = allTiles[i].OffsetY + Center.Y;

                tiles[xOffset] ??= new TileList[Height];
                Tiles[xOffset] ??= new StaticTile[Height][];

                tiles[xOffset][yOffset] ??= new TileList();
                tiles[xOffset][yOffset].Add(new StaticTile(allTiles[i].ItemId, (sbyte)allTiles[i].OffsetZ));
            }
        }

        for (var x = 0; x < Width; ++x)
        {
            Tiles[x] ??= new StaticTile[Height][];

            for (var y = 0; y < Height; ++y)
            {
                var tileList = tiles[x]?[y];
                Tiles[x][y] = tileList?.ToArray() ?? [];
            }
        }
    }
}

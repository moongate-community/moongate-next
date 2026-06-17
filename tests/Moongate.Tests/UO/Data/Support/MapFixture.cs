namespace Moongate.Tests.UO.Data.Support;

/// <summary>
///     Writes synthetic <c>map{n}.mul</c>, <c>staidx{n}.mul</c> and <c>statics{n}.mul</c> files for a
///     small facet (width/height multiples of 8), with caller-specified land cells and static tiles.
/// </summary>
public static class MapFixture
{
    public static void Write(
        string directory,
        int fileIndex,
        int width,
        int height,
        IReadOnlyList<LandCell> landCells,
        IReadOnlyList<StaticTileSpec> statics
    )
    {
        var blockWidth = width >> 3;
        var blockHeight = height >> 3;

        WriteLand(directory, fileIndex, blockWidth, blockHeight, landCells);
        WriteStatics(directory, fileIndex, blockWidth, blockHeight, statics);
    }

    private static void WriteLand(
        string directory,
        int fileIndex,
        int blockWidth,
        int blockHeight,
        IReadOnlyList<LandCell> landCells
    )
    {
        var blockCount = blockWidth * blockHeight;
        var path = Path.Combine(directory, $"map{fileIndex}.mul");

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        using var bw = new BinaryWriter(fs);

        var cells = new Dictionary<(int block, int cell), (short id, sbyte z)>();

        foreach (var c in landCells)
        {
            var bx = c.X >> 3;
            var by = c.Y >> 3;
            var block = bx * blockHeight + by;
            var cell = ((c.Y & 7) << 3) + (c.X & 7);
            cells[(block, cell)] = (c.Id, c.Z);
        }

        for (var block = 0; block < blockCount; block++)
        {
            bw.Write(0); // 4-byte header

            for (var cell = 0; cell < 64; cell++)
            {
                var value = cells.GetValueOrDefault((block, cell));
                bw.Write(value.id);
                bw.Write(value.z);
            }
        }
    }

    private static void WriteStatics(
        string directory,
        int fileIndex,
        int blockWidth,
        int blockHeight,
        IReadOnlyList<StaticTileSpec> statics
    )
    {
        var blockCount = blockWidth * blockHeight;
        var idxPath = Path.Combine(directory, $"staidx{fileIndex}.mul");
        var dataPath = Path.Combine(directory, $"statics{fileIndex}.mul");

        var byBlock = new Dictionary<int, List<StaticTileSpec>>();

        foreach (var s in statics)
        {
            var block = s.X * blockHeight + s.Y; // X/Y here are block coords

            if (!byBlock.TryGetValue(block, out var list))
            {
                list = new List<StaticTileSpec>();
                byBlock[block] = list;
            }

            list.Add(s);
        }

        using var data = new FileStream(dataPath, FileMode.Create, FileAccess.Write, FileShare.None);
        using var dataWriter = new BinaryWriter(data);
        using var idx = new FileStream(idxPath, FileMode.Create, FileAccess.Write, FileShare.None);
        using var idxWriter = new BinaryWriter(idx);

        var offset = 0;

        for (var block = 0; block < blockCount; block++)
        {
            if (byBlock.TryGetValue(block, out var list) && list.Count > 0)
            {
                foreach (var s in list)
                {
                    dataWriter.Write(s.Id);
                    dataWriter.Write(s.BlockX);
                    dataWriter.Write(s.BlockY);
                    dataWriter.Write(s.Z);
                    dataWriter.Write(s.Hue);
                }

                var length = list.Count * 7;
                idxWriter.Write(offset); // lookup
                idxWriter.Write(length); // length
                idxWriter.Write(0);      // extra
                offset += length;
            }
            else
            {
                idxWriter.Write(-1); // lookup (empty)
                idxWriter.Write(0);  // length
                idxWriter.Write(0);  // extra
            }
        }
    }

    public sealed record LandCell(int X, int Y, short Id, sbyte Z);

    public sealed record StaticTileSpec(int X, int Y, ushort Id, byte BlockX, byte BlockY, sbyte Z, short Hue);
}

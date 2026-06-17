namespace Moongate.Tests.UO.Data.Support;

/// <summary>
///     Writes synthetic classic <c>multi.idx</c> + <c>multi.mul</c> (postHS 16-byte entries) for the
///     given multis keyed by id; absent ids get an empty index record.
/// </summary>
public static class MultiFixture
{
    public static void WriteMul(string directory, IReadOnlyDictionary<int, IReadOnlyList<Tile>> multis)
    {
        var maxId = multis.Keys.Max();
        var idxPath = Path.Combine(directory, "multi.idx");
        var mulPath = Path.Combine(directory, "multi.mul");

        using var mul = new FileStream(mulPath, FileMode.Create, FileAccess.Write, FileShare.None);
        using var mulWriter = new BinaryWriter(mul);
        using var idx = new FileStream(idxPath, FileMode.Create, FileAccess.Write, FileShare.None);
        using var idxWriter = new BinaryWriter(idx);

        var offset = 0;

        for (var id = 0; id <= maxId; id++)
        {
            if (multis.TryGetValue(id, out var tiles) && tiles.Count > 0)
            {
                foreach (var t in tiles)
                {
                    mulWriter.Write(t.ItemId);
                    mulWriter.Write(t.X);
                    mulWriter.Write(t.Y);
                    mulWriter.Write(t.Z);
                    mulWriter.Write(t.Flags);
                }

                var length = tiles.Count * 16;
                idxWriter.Write(offset);
                idxWriter.Write(length);
                idxWriter.Write(0);
                offset += length;
            }
            else
            {
                idxWriter.Write(-1);
                idxWriter.Write(0);
                idxWriter.Write(0);
            }
        }
    }

    public sealed record Tile(ushort ItemId, short X, short Y, short Z, ulong Flags);
}

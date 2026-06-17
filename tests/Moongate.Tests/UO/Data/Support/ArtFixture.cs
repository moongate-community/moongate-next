namespace Moongate.Tests.UO.Data.Support;

/// <summary>
///     Writes a minimal classic <c>artidx.mul</c> + <c>art.mul</c> containing a single decodable static
///     (a 1×1 opaque-white pixel) at the index slot for <paramref name="itemId" /> (= itemId + 0x4000).
/// </summary>
public static class ArtFixture
{
    // 1x1 opaque-white static: header(4) + width(1) + height(1) + 1 row lookup(0)
    // + run (xOffset 0, xRun 1) + pixel 0x7FFF (-> 0xFFFF after XOR 0x8000 = white) + (0,0) terminator.
    private static readonly byte[] _whitePixelStatic =
    [
        0x00, 0x00, 0x00, 0x00, // unused header
        0x01, 0x00,             // width = 1
        0x01, 0x00,             // height = 1
        0x00, 0x00,             // row lookup[0] = 0
        0x00, 0x00,             // xOffset = 0
        0x01, 0x00,             // xRun = 1
        0xFF, 0x7F,             // pixel 0x7FFF
        0x00, 0x00,             // xOffset = 0 (terminator)
        0x00, 0x00              // xRun = 0 (terminator)
    ];

    public static void WriteWhitePixel(string directory, int itemId)
    {
        var slot = itemId + 0x4000;
        var idxPath = Path.Combine(directory, "artidx.mul");
        var mulPath = Path.Combine(directory, "art.mul");

        File.WriteAllBytes(mulPath, _whitePixelStatic);

        using var idx = new FileStream(idxPath, FileMode.Create, FileAccess.Write, FileShare.None);
        using var bw = new BinaryWriter(idx);

        for (var i = 0; i <= slot; i++)
        {
            if (i == slot)
            {
                bw.Write(0);                        // lookup (offset in art.mul)
                bw.Write(_whitePixelStatic.Length); // length
                bw.Write(0);                        // extra
            }
            else
            {
                bw.Write(-1); // empty entry
                bw.Write(0);
                bw.Write(0);
            }
        }
    }
}

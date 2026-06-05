namespace Moongate.Tests.UO.Data.Support;

/// <summary>
/// Writes a synthetic <c>texidx.mul</c> + <c>texmaps.mul</c> with one 64×64 texture (single RGB555
/// colour) at the given index.
/// </summary>
public static class TextureFixture
{
    public static void Write64(string directory, int index, ushort color)
    {
        const int dim = 64;
        var payload = dim * dim * 2; // bytes
        var idxPath = Path.Combine(directory, "texidx.mul");
        var mulPath = Path.Combine(directory, "texmaps.mul");

        using (var mul = new FileStream(mulPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            using (var bw = new BinaryWriter(mul))
            {
                for (var i = 0; i < dim * dim; i++)
                {
                    bw.Write(color);
                }
            }
        }

        using (var idx = new FileStream(idxPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            using (var bw = new BinaryWriter(idx))
            {
                for (var i = 0; i <= index; i++)
                {
                    if (i == index)
                    {
                        bw.Write(0);       // lookup
                        bw.Write(payload); // length (8192 -> 64x64)
                        bw.Write(0);       // extra
                    }
                    else
                    {
                        bw.Write(-1);
                        bw.Write(0);
                        bw.Write(0);
                    }
                }
            }
        }
    }
}

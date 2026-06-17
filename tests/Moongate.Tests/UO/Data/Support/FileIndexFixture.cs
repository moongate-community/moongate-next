namespace Moongate.Tests.UO.Data.Support;

/// <summary>
///     Writes a synthetic <c>.idx</c> + <c>.mul</c> pair: each entry is 12 bytes (lookup, length, extra)
///     in the idx, pointing at a payload appended to the mul.
/// </summary>
public static class FileIndexFixture
{
    public static (string IdxPath, string MulPath) Write(string directory, string baseName, IReadOnlyList<Payload> payloads)
    {
        var idxPath = Path.Combine(directory, baseName + ".idx");
        var mulPath = Path.Combine(directory, baseName + ".mul");

        using (var mul = new FileStream(mulPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            using (var idx = new FileStream(idxPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var idxWriter = new BinaryWriter(idx))
                {
                    var offset = 0;

                    foreach (var payload in payloads)
                    {
                        mul.Write(payload.Data, 0, payload.Data.Length);

                        idxWriter.Write(offset);              // lookup
                        idxWriter.Write(payload.Data.Length); // length
                        idxWriter.Write(payload.Extra);       // extra

                        offset += payload.Data.Length;
                    }
                }
            }
        }

        return (idxPath, mulPath);
    }

    public sealed record Payload(int Extra, byte[] Data);
}

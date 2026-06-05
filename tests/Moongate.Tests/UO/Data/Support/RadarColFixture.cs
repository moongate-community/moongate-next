namespace Moongate.Tests.UO.Data.Support;

/// <summary>
/// Writes a synthetic <c>radarcol.mul</c> (0x8000 ushort RGB555 entries) with caller-specified colours.
/// </summary>
public static class RadarColFixture
{
    public static void Write(string directory, IReadOnlyDictionary<int, ushort> colorsByIndex)
    {
        const int total = 0x8000;
        var path = Path.Combine(directory, "radarcol.mul");

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        using var bw = new BinaryWriter(fs);

        for (var i = 0; i < total; i++)
        {
            bw.Write(colorsByIndex.GetValueOrDefault(i, (ushort)0));
        }
    }
}

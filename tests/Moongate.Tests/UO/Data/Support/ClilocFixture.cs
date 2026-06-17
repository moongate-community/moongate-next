using System.Text;

namespace Moongate.Tests.UO.Data.Support;

/// <summary>
///     Writes a minimal uncompressed <c>cliloc.enu</c> (6-byte header then number/flag/length/utf8
///     records) into a directory.
/// </summary>
public static class ClilocFixture
{
    public static string Write(string directory, IReadOnlyList<Entry> entries)
    {
        var path = Path.Combine(directory, "cliloc.enu");

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        using var bw = new BinaryWriter(fs);

        bw.Write(0);        // int32 header (skipped by reader)
        bw.Write((short)0); // int16 header (skipped by reader)

        foreach (var entry in entries)
        {
            var bytes = Encoding.UTF8.GetBytes(entry.Text);
            bw.Write(entry.Number);
            bw.Write(entry.Flag);
            bw.Write((ushort)bytes.Length);
            bw.Write(bytes);
        }

        return path;
    }

    public sealed record Entry(int Number, byte Flag, string Text);
}

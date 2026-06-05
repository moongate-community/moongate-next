using System.Text;

namespace Moongate.Tests.UO.Data.Support;

/// <summary>
/// Writes a synthetic <c>hues.mul</c> with a single group of 8 entries; entry 0 gets the given
/// first colour and name, the rest are zeroed.
/// </summary>
public static class HuesFixture
{
    public static void WriteSingleGroup(string directory, ushort firstColor, ushort tableStart, ushort tableEnd, string name)
    {
        var path = Path.Combine(directory, "hues.mul");

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        using var bw = new BinaryWriter(fs);

        bw.Write(0u); // group header

        for (var entry = 0; entry < 8; entry++)
        {
            for (var c = 0; c < 32; c++)
            {
                bw.Write(entry == 0 && c == 0 ? firstColor : (ushort)0);
            }

            bw.Write(entry == 0 ? tableStart : (ushort)0);
            bw.Write(entry == 0 ? tableEnd : (ushort)0);

            Span<byte> nameBytes = stackalloc byte[20];

            if (entry == 0)
            {
                var bytes = Encoding.ASCII.GetBytes(name);
                bytes.AsSpan(0, Math.Min(bytes.Length, 20)).CopyTo(nameBytes);
            }

            bw.Write(nameBytes);
        }
    }
}

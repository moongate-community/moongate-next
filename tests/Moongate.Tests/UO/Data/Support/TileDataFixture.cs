using System.Text;

namespace Moongate.Tests.UO.Data.Support;

/// <summary>
/// Writes a minimal but structurally valid <c>tiledata.mul</c> (32-bit flags, 0x4000 land + 0x4000
/// item entries) to a temp directory, with a few caller-specified entries populated.
/// </summary>
public static class TileDataFixture
{
    private const int LandLength = 0x4000;
    private const int ItemLength = 0x4000;

    public sealed record LandEntry(int Index, uint Flags, string Name);

    public sealed record ItemEntry(
        int Index,
        uint Flags,
        string Name,
        int Weight,
        int Quality,
        int Animation,
        int Quantity,
        int Value,
        int Height
    );

    public static string Write(string directory, IEnumerable<LandEntry> land, IEnumerable<ItemEntry> items)
    {
        var landByIndex = land.ToDictionary(e => e.Index);
        var itemByIndex = items.ToDictionary(e => e.Index);

        var path = Path.Combine(directory, "tiledata.mul");
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        using var bw = new BinaryWriter(fs);

        for (var i = 0; i < LandLength; i++)
        {
            if ((i & 0x1F) == 0)
            {
                bw.Write(0); // block header
            }

            var entry = landByIndex.GetValueOrDefault(i);
            bw.Write(entry?.Flags ?? 0u); // 4 bytes flags
            bw.Write((short)0);           // textureId
            WriteName(bw, entry?.Name ?? "");
        }

        for (var i = 0; i < ItemLength; i++)
        {
            if ((i & 0x1F) == 0)
            {
                bw.Write(0); // block header
            }

            var entry = itemByIndex.GetValueOrDefault(i);
            bw.Write(entry?.Flags ?? 0u);              // 4 bytes flags
            bw.Write((byte)(entry?.Weight ?? 0));      // weight
            bw.Write((byte)(entry?.Quality ?? 0));     // quality
            bw.Write((ushort)(entry?.Animation ?? 0)); // animation
            bw.Write((byte)0);                         // unknown
            bw.Write((byte)(entry?.Quantity ?? 0));    // quantity
            bw.Write(0);                               // unknown (4 bytes)
            bw.Write((byte)0);                         // unknown
            bw.Write((byte)(entry?.Value ?? 0));       // value
            bw.Write((byte)(entry?.Height ?? 0));      // height
            WriteName(bw, entry?.Name ?? "");
        }

        return path;
    }

    private static void WriteName(BinaryWriter bw, string name)
    {
        Span<byte> buffer = stackalloc byte[20];
        var bytes = Encoding.ASCII.GetBytes(name);
        var count = Math.Min(bytes.Length, 20);
        bytes.AsSpan(0, count).CopyTo(buffer);
        bw.Write(buffer);
    }
}

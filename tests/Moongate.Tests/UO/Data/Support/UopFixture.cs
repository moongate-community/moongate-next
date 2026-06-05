using Moongate.UO.Data.Files.Internal;

namespace Moongate.Tests.UO.Data.Support;

/// <summary>
/// Writes a minimal valid UOP container with a single data block, so the index reader can be
/// exercised without a real client file.
/// </summary>
public static class UopFixture
{
    public static (string Path, long DataOffset, int DataLength) WriteSingleEntry(
        string directory,
        string baseName,
        byte[] payload,
        int version = 5)
    {
        var path = Path.Combine(directory, baseName + ".uop");

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        using var bw = new BinaryWriter(fs);

        bw.Write((uint)0x50594D);   // "MYP\0"
        bw.Write(version);          // version
        bw.Write((uint)0xFD23EC43); // signature

        var nextBlockPos = fs.Position;
        bw.Write((long)0);          // placeholder for first block offset

        var dataOffset = fs.Position;
        bw.Write(payload);

        var blockOffset = fs.Position;
        bw.Write(1);                // fileCount
        bw.Write((long)0);          // nextBlock = 0 (stop)

        var hash = UopIndexReader.HashLittle2($"build/{baseName.ToLowerInvariant()}/00000000.dat");

        bw.Write(dataOffset);       // offset
        bw.Write(0);                // headerLength
        bw.Write(payload.Length);   // compressedLength
        bw.Write(payload.Length);   // decompressedLength
        bw.Write(hash);             // fileNameHash
        bw.Write((uint)0);          // Adler32
        bw.Write((short)0);         // compressed flag

        bw.Flush();
        fs.Seek(nextBlockPos, SeekOrigin.Begin);
        bw.Write(blockOffset);

        return (path, dataOffset, payload.Length);
    }
}

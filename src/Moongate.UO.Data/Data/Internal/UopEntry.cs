namespace Moongate.UO.Data.Data.Internal;

/// <summary>
///     One data-block entry from a UOP file index: its byte offset, decompressed size, compression
///     state, compressed size and an extra ordering value populated by the map reader.
/// </summary>
public struct UopEntry
{
    public readonly long Offset;
    public readonly int Size;
    public bool Compressed;
    public int CompressedSize;
    public int Extra;

    public UopEntry(long offset, int length)
    {
        Offset = offset;
        Size = length;
        Compressed = false;
        CompressedSize = 0;
        Extra = 0;
    }
}

namespace Moongate.UO.Data.Files;

/// <summary>
/// A verdata index entry: file, index, byte offset, length and an extra field.
/// </summary>
public struct Entry5D
{
    public int file;
    public int index;
    public int lookup;
    public int length;
    public int extra;

    public override string ToString()
        => $"File: {file}, Index: {index}, Lookup: {lookup}, Length: {length}, Extra: {extra}";
}

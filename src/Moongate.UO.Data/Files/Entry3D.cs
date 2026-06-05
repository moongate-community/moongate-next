using System.Runtime.InteropServices;

namespace Moongate.UO.Data.Files;

/// <summary>
/// A single 12-byte entry of a UO <c>.idx</c> index: byte offset, length and an extra field.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Entry3D
{
    public int lookup;
    public int length;
    public int extra;
}

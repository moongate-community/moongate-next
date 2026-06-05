namespace Moongate.UO.Data.Data.Internal;

/// <summary>
/// A single <c>verdata.mul</c> patch entry: which file/index it overrides and where the replacement
/// data lives within the verdata stream.
/// </summary>
public struct VerdataPatch
{
    public int file;
    public int index;
    public int lookup;
    public int length;
    public int extra;
}

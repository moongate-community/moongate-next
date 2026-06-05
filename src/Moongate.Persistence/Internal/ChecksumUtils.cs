namespace Moongate.Persistence.Internal;

/// <summary>
/// FNV-1a 32-bit checksum used to validate journal records.
/// </summary>
internal static class ChecksumUtils
{
    private const uint FnvOffsetBasis = 2166136261;
    private const uint FnvPrime = 16777619;

    public static uint Compute(ReadOnlySpan<byte> data)
    {
        var hash = FnvOffsetBasis;

        for (var i = 0; i < data.Length; i++)
        {
            hash ^= data[i];
            hash *= FnvPrime;
        }

        return hash;
    }
}

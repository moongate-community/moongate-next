using System.Numerics;

namespace Moongate.UO.Data.Utils;

/// <summary>
/// Sector grid constants for spatial indexing (sector size in tiles + derived bit shift).
/// </summary>
public static class MapSectorConsts
{
    /// <summary>Size of each sector in tiles. Must be a positive power of two.</summary>
    public const int SectorSize = 16;

    /// <summary>Bit shift equivalent to dividing a tile coordinate by <see cref="SectorSize" />.</summary>
    public static readonly int SectorShift = ComputeSectorShift();

    private static int ComputeSectorShift()
    {
        if (SectorSize <= 0 || !BitOperations.IsPow2((uint)SectorSize))
        {
            throw new InvalidOperationException(
                $"Invalid {nameof(SectorSize)}={SectorSize}. Sector size must be a positive power of two."
            );
        }

        return BitOperations.TrailingZeroCount((uint)SectorSize);
    }
}

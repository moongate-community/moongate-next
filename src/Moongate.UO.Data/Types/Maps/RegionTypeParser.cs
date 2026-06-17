namespace Moongate.UO.Data.Types.Maps;

/// <summary>
///     Maps server-asset region type strings (e.g. "DungeonRegion") to <see cref="RegionType" />.
/// </summary>
public static class RegionTypeParser
{
    /// <summary>Maps a server-asset region type string to a <see cref="RegionType" /> (case-insensitive; null/unknown → Unknown).</summary>
    public static RegionType FromAssetType(string? assetType)
    {
        return assetType?.Trim().ToLowerInvariant() switch
        {
            "baseregion" => RegionType.Base,
            "greenacresregion" => RegionType.GreenAcres,
            "guardedregion" => RegionType.Guarded,
            "jailregion" => RegionType.Jail,
            "dungeonregion" => RegionType.Dungeon,
            "nohousingregion" => RegionType.NoHousing,
            "townregion" => RegionType.Town,
            _ => RegionType.Unknown
        };
    }
}

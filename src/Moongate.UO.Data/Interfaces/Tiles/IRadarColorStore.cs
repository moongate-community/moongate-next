namespace Moongate.UO.Data.Interfaces.Tiles;

/// <summary>Provides radar/minimap colours for land and static tiles, from <c>radarcol.mul</c>.</summary>
public interface IRadarColorStore
{
    /// <summary>Number of colour entries in the table.</summary>
    int Count { get; }

    /// <summary>Radar colour for a land tile id.</summary>
    /// <param name="tileId">Land tile id.</param>
    (byte R, byte G, byte B) GetLandColor(int tileId);

    /// <summary>Radar colour for a static (item) tile id.</summary>
    /// <param name="tileId">Static tile id.</param>
    (byte R, byte G, byte B) GetStaticColor(int tileId);
}

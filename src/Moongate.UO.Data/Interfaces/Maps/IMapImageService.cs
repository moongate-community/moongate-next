using SixLabors.ImageSharp;

namespace Moongate.UO.Data.Interfaces.Maps;

/// <summary>
/// Generates radar-colour images for UO map facets.
/// </summary>
public interface IMapImageService
{
    /// <summary>
    /// Renders the map with the given id.
    /// </summary>
    /// <param name="mapId">Map id to render.</param>
    /// <returns>The rendered image, or <c>null</c> when the map or backing files are unavailable.</returns>
    Image? GetMapImage(int mapId);
}

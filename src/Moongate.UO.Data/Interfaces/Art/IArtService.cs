using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.UO.Data.Interfaces.Art;

/// <summary>
///     Provides access to item-art bitmaps decoded from the Ultima Online art files.
/// </summary>
public interface IArtService
{
    /// <summary>
    ///     Gets the highest legal item graphic id supported by the loaded art index.
    /// </summary>
    int MaxItemId { get; }

    /// <summary>
    ///     Decodes the item-art bitmap for <paramref name="itemId" />, or <c>null</c> when absent.
    /// </summary>
    /// <param name="itemId">Item graphic id.</param>
    /// <param name="clone">When <c>true</c> (default) returns a detached copy of the cached image.</param>
    Image<Rgba32>? GetArt(int itemId, bool clone = true);

    /// <summary>Returns <c>true</c> when item art exists for <paramref name="itemId" />.</summary>
    /// <param name="itemId">Item graphic id.</param>
    bool IsValidArt(int itemId);
}

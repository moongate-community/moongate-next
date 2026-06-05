using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.UO.Data.Interfaces.Textures;

/// <summary>Provides land-texture bitmaps decoded from <c>texmaps.mul</c>.</summary>
public interface ITextureStore
{
    /// <summary>Decodes the texture at <paramref name="index" />, or <c>null</c> when absent.</summary>
    /// <param name="index">Texture index.</param>
    /// <param name="clone">When <c>true</c> (default) returns a detached copy of the cached image.</param>
    Image<Rgba32>? GetTexture(int index, bool clone = true);

    /// <summary>Returns <c>true</c> when a texture exists at <paramref name="index" />.</summary>
    /// <param name="index">Texture index.</param>
    bool IsValidTexture(int index);
}

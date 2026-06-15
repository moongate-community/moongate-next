using Moongate.UO.Data.Data.Animations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.UO.Data.Interfaces.Animations;

/// <summary>Renders a composited UO paperdoll (gump art) for a source-agnostic request.</summary>
public interface IPaperdollRenderer
{
    /// <summary>Returns the composited paperdoll image, or <c>null</c> when the base body gump is unavailable.</summary>
    Image<Rgba32>? Render(PaperdollRenderRequest request);
}

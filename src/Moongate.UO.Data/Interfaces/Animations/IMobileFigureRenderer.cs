using Moongate.UO.Data.Animations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.UO.Data.Interfaces.Animations;

/// <summary>Renders a composited mobile figure (body + hair + facial hair) for a render request.</summary>
public interface IMobileFigureRenderer
{
    /// <summary>Returns the composited image, or <c>null</c> when the body has no animation.</summary>
    Image<Rgba32>? Render(MobileRenderRequest request);
}

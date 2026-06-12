using Moongate.UO.Data.Animations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.UO.Data.Interfaces.Animations;

/// <summary>
/// Provides a single decoded body-animation frame (default: idle, front-facing, frame 0) for a body id,
/// decoded from <c>anim.mul</c>. Returns <c>null</c> when the body has no usable animation.
/// </summary>
public interface IAnimationService
{
    /// <summary>
    /// Decodes a body frame, or <c>null</c> when absent. Defaults render the idle pose facing the viewer.
    /// </summary>
    /// <param name="body">Body graphic id (will be remapped via Body.def).</param>
    /// <param name="action">Animation action; 0 is idle/stand.</param>
    /// <param name="direction">Stored direction 0..4; 1 is the default front-ish facing.</param>
    /// <param name="frame">Frame within the action/direction; 0 is the first.</param>
    /// <param name="hue">
    /// Optional caller hue (e.g. a mobile skin hue, 1-based packet id); when non-zero it
    /// overrides the Body.def hue. 0 = use the Body.def hue (or none).
    /// </param>
    Image<Rgba32>? GetBodyFrame(int body, int action = 0, int direction = 1, int frame = 0, int hue = 0);

    /// <summary>
    /// Decodes a single graphic at one specific direction (no fallback), hued, returning the frame and its
    /// centre for compositing, or <c>null</c> when absent.
    /// </summary>
    DecodedFrame? GetDecodedFrame(int graphic, int action, int direction, int frame, int hue);
}

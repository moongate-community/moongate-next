using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.UO.Data.Interfaces.Gumps;

/// <summary>Provides decoded UO gump images from <c>gumpidx.mul</c>/<c>gumpart.mul</c>.</summary>
public interface IGumpStore
{
    /// <summary>Returns the decoded gump for <paramref name="gumpId" />, or <c>null</c> when absent/empty.</summary>
    Image<Rgba32>? GetGump(int gumpId);
}

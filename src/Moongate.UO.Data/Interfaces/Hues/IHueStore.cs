using Moongate.UO.Data.Data.Hues;

namespace Moongate.UO.Data.Interfaces.Hues;

/// <summary>Provides the UO hue palette table from <c>hues.mul</c>.</summary>
public interface IHueStore
{
    /// <summary>All loaded hues (index 0-based; packet hue id is index + 1).</summary>
    IReadOnlyList<Hue> Hues { get; }

    /// <summary>Number of loaded hues.</summary>
    int Count { get; }

    /// <summary>Returns the hue at <paramref name="index" /> (0-based), or <c>null</c> when out of range.</summary>
    /// <param name="index">Zero-based hue index.</param>
    Hue? GetHue(int index);
}

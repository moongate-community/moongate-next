using Moongate.UO.Data.Data.Internal;
using Moongate.UO.Data.Files;

namespace Moongate.UO.Data.Interfaces.Files;

/// <summary>
/// Supplies <c>verdata.mul</c> patch entries and the patched data stream to <see cref="FileIndex" />.
/// Implementations report no patches when verdata patching is disabled or unavailable.
/// </summary>
public interface IVerdataPatchSource
{
    /// <summary>Patch entries to overlay onto file indexes; empty when no verdata is loaded.</summary>
    IReadOnlyList<VerdataPatch> Patches { get; }

    /// <summary>Positions the verdata stream at <paramref name="lookup" /> and returns it.</summary>
    /// <param name="lookup">Byte offset within the verdata stream.</param>
    Stream Seek(int lookup);
}

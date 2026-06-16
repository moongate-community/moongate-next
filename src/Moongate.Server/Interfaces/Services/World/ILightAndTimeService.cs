using Moongate.Abstractions.Interfaces.Services;
using Moongate.Core.Geometry;

namespace Moongate.Server.Interfaces.Services.World;

/// <summary>
/// Drives the day/night light cycle and the accelerated world clock.
/// </summary>
public interface ILightAndTimeService : IMoongateService
{
    /// <summary>Computes the global light level (0 = brightest) at a map location, with region and manual overrides.</summary>
    int ComputeGlobalLightLevel(int mapId, Point3D location, DateTime? utcNow = null);

    /// <summary>Returns the current accelerated world time-of-day (used for the SetTime packet).</summary>
    /// <remarks>Only the time-of-day components (hour/minute/second) are meaningful; the date part is not a UO calendar date.</remarks>
    DateTime GetWorldTime(DateTime? utcNow = null);

    /// <summary>Forces a global light level (0-255), or clears the override with null.</summary>
    /// <remarks><paramref name="applyImmediately" /> = true triggers a synchronous broadcast pass; do not call it from the network I/O thread or an async context.</remarks>
    void SetGlobalLightOverride(int? lightLevel, bool applyImmediately = true);
}

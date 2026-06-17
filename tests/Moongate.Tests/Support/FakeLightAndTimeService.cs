using Moongate.Core.Geometry;
using Moongate.Server.Interfaces.Services.World;

namespace Moongate.Tests.Support;

/// <summary>
///     ILightAndTimeService stub returning a fixed light level (12) and world time (18:00 UTC).
/// </summary>
public sealed class FakeLightAndTimeService : ILightAndTimeService
{
    public int ComputeGlobalLightLevel(int mapId, Point3D location, DateTime? utcNow = null)
    {
        return 12;
    }

    public DateTime GetWorldTime(DateTime? utcNow = null)
    {
        return new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc);
    }

    public void SetGlobalLightOverride(int? lightLevel, bool applyImmediately = true)
    {
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

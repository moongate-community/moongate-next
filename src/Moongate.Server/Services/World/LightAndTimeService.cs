using Moongate.Abstractions.Interfaces.Jobs;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Abstractions.Types.Player;
using Moongate.Core.Geometry;
using Moongate.Network.UO.Packets.Outgoing.World;
using Moongate.Network.UO.Types.Environment;
using Moongate.Server.Data.Config;
using Moongate.Server.Data.World.Internal;
using Moongate.Server.Interfaces.Network;
using Moongate.Server.Interfaces.Services.World;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Types.Maps;

namespace Moongate.Server.Services.World;

/// <summary>
/// Computes and broadcasts day/night light and supplies the accelerated world clock.
/// </summary>
public sealed class LightAndTimeService : ILightAndTimeService
{
    private const int DungeonLevel = 26;
    private const int JailLevel = 9;
    private const int PersonalLightLevel = 0;
    private const string JobName = "light_and_time_update";

    private readonly IPlayerSessionService _playerSessions;
    private readonly Lazy<IMobileService> _mobiles;
    private readonly IOutgoingPacketQueue _outgoing;
    private readonly IRegionResolverService _regions;
    private readonly IJobService _jobs;
    private readonly Lock _sync = new();
    private readonly Dictionary<long, int> _lastBySession = [];
    private readonly DateTime _worldStartUtc;
    private readonly double _secondsPerUoMinute;
    private volatile int _forcedGlobalLightLevel = -1;
    private string? _jobId;

    public LightAndTimeService(
        IPlayerSessionService playerSessions,
        Lazy<IMobileService> mobiles,
        IOutgoingPacketQueue outgoing,
        IRegionResolverService regions,
        IJobService jobs,
        ServerConfig config
    )
    {
        ArgumentNullException.ThrowIfNull(playerSessions);
        ArgumentNullException.ThrowIfNull(mobiles);
        ArgumentNullException.ThrowIfNull(outgoing);
        ArgumentNullException.ThrowIfNull(regions);
        ArgumentNullException.ThrowIfNull(jobs);
        ArgumentNullException.ThrowIfNull(config);

        _playerSessions = playerSessions;
        _mobiles = mobiles;
        _outgoing = outgoing;
        _regions = regions;
        _jobs = jobs;

        _secondsPerUoMinute = config.LightSecondsPerUoMinute > 0 ? config.LightSecondsPerUoMinute : 5.0;
        _worldStartUtc = DateTime.TryParse(
            config.LightWorldStartUtc,
            null,
            System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal,
            out var parsed
        )
            ? parsed
            : new DateTime(1997, 9, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    public int ComputeGlobalLightLevel(int mapId, Point3D location, DateTime? utcNow = null)
    {
        if (_forcedGlobalLightLevel >= 0)
        {
            return _forcedGlobalLightLevel;
        }

        var region = _regions.ResolveRegion(mapId, location);

        if (region is { Kind: RegionType.Dungeon })
        {
            return DungeonLevel;
        }

        if (region is { Kind: RegionType.Jail })
        {
            return JailLevel;
        }

        var now = utcNow?.ToUniversalTime() ?? DateTime.UtcNow;
        var minutes = LightCycle.TotalUoMinutes(now, _worldStartUtc, _secondsPerUoMinute)
                      + mapId * 320
                      + location.X / 16.0;
        var (hour, minute, _) = LightCycle.TimeOfDay(minutes);

        return Math.Clamp(LightCycle.LevelFromHourMinute(hour, minute), 0, byte.MaxValue);
    }

    public DateTime GetWorldTime(DateTime? utcNow = null)
    {
        var now = utcNow?.ToUniversalTime() ?? DateTime.UtcNow;
        var (hour, minute, second) = LightCycle.TimeOfDay(
            LightCycle.TotalUoMinutes(now, _worldStartUtc, _secondsPerUoMinute)
        );

        return now.Date.AddHours(hour).AddMinutes(minute).AddSeconds(second);
    }

    public void SetGlobalLightOverride(int? lightLevel, bool applyImmediately = true)
    {
        _forcedGlobalLightLevel = lightLevel.HasValue ? Math.Clamp(lightLevel.Value, 0, byte.MaxValue) : -1;

        if (applyImmediately)
        {
            ProcessLightAndTime();
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _jobId = _jobs.RegisterRecurring(
            JobName,
            TimeSpan.FromSeconds(10),
            ProcessLightAndTime,
            "Broadcasts day/night light changes to in-world players"
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_jobId is not null)
        {
            _jobs.Cancel(_jobId);
        }

        return Task.CompletedTask;
    }

    private void ProcessLightAndTime()
    {
        var activeSessionIds = new HashSet<long>();

        foreach (var session in _playerSessions.GetAll())
        {
            if (session.State != PlayerSessionStateType.InWorld || session.MobileSerial is not { } serial)
            {
                continue;
            }

            var mobile = _mobiles.Value.GetByIdAsync(serial).GetAwaiter().GetResult();

            if (mobile is null)
            {
                continue;
            }

            activeSessionIds.Add(session.SessionId);

            var level = ComputeGlobalLightLevel(mobile.MapId, mobile.Location);

            lock (_sync)
            {
                if (_lastBySession.TryGetValue(session.SessionId, out var last) && last == level)
                {
                    continue;
                }

                _lastBySession[session.SessionId] = level;
            }

            _outgoing.Enqueue(session.SessionId, new OverallLightLevelPacket((LightLevelType)(byte)level));
            _outgoing.Enqueue(session.SessionId, new PersonalLightLevelPacket(mobile.Id, (LightLevelType)PersonalLightLevel));
        }

        lock (_sync)
        {
            var stale = _lastBySession.Keys.Where(id => !activeSessionIds.Contains(id)).ToList();

            foreach (var id in stale)
            {
                _lastBySession.Remove(id);
            }
        }
    }
}

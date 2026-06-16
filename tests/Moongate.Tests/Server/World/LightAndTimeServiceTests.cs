using System.Linq;
using Moongate.Abstractions.Data.Player;
using Moongate.Abstractions.Types.Player;
using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Network.UO.Packets.Outgoing.World;
using Moongate.Server.Data.Config;
using Moongate.Server.Data.World;
using Moongate.Server.Services.World;
using Moongate.Tests.Support;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Interfaces.Services;
using Xunit;

namespace Moongate.Tests.Server.World;

public sealed class LightAndTimeServiceTests
{
    private static readonly DateTime Start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ComputeGlobalLightLevel_DungeonRegion_ReturnsDungeonLevel()
        => Assert.Equal(26, Build(Region("DungeonRegion")).ComputeGlobalLightLevel(0, new Point3D(10, 10, 0), Start));

    [Fact]
    public void ComputeGlobalLightLevel_JailRegion_ReturnsJailLevel()
        => Assert.Equal(9, Build(Region("JailRegion")).ComputeGlobalLightLevel(0, new Point3D(10, 10, 0), Start));

    [Fact]
    public void ComputeGlobalLightLevel_NoRegion_FollowsDayNightCycle()
    {
        var service = Build(region: null);
        Assert.Equal(12, service.ComputeGlobalLightLevel(0, new Point3D(0, 0, 0), Start));               // 00:00 night
        Assert.Equal(12, service.ComputeGlobalLightLevel(0, new Point3D(0, 0, 0), Start.AddSeconds(720))); // 02:24 night
    }

    [Fact]
    public void SetGlobalLightOverride_WinsOverEverything()
    {
        var service = Build(Region("DungeonRegion"));
        service.SetGlobalLightOverride(3, applyImmediately: false);
        Assert.Equal(3, service.ComputeGlobalLightLevel(0, new Point3D(10, 10, 0), Start));
        service.SetGlobalLightOverride(null, applyImmediately: false);
        Assert.Equal(26, service.ComputeGlobalLightLevel(0, new Point3D(10, 10, 0), Start));
    }

    [Fact]
    public void SetGlobalLightOverride_ClampsToByteRange()
    {
        var service = Build(region: null);

        service.SetGlobalLightOverride(300, applyImmediately: false);
        Assert.Equal(255, service.ComputeGlobalLightLevel(0, new Point3D(0, 0, 0), Start));

        service.SetGlobalLightOverride(-5, applyImmediately: false);
        Assert.Equal(0, service.ComputeGlobalLightLevel(0, new Point3D(0, 0, 0), Start));
    }

    [Fact]
    public void GetWorldTime_ReflectsAcceleratedClock()
    {
        var time = Build(region: null).GetWorldTime(Start.AddSeconds(5400)); // 1080 uo-min = 18:00
        Assert.Equal(18, time.Hour);
        Assert.Equal(0, time.Minute);
    }

    [Fact]
    public async Task ProcessLightAndTime_SendsOnChange_AndNotWhenUnchanged()
    {
        var mobile = new MobileEntity { Id = new Serial(7), MapId = 0, Location = new Point3D(10, 10, 0) };
        var sessions = new StubPlayerSessions(InWorld(42, mobile.Id));
        var outgoing = new RecordingOutgoingQueue();
        var jobs = new CapturingJobService();
        var service = new LightAndTimeService(
            sessions,
            new Lazy<IMobileService>(() => new MapItemMobileService(mobile)),
            outgoing,
            new StubRegionResolver(null),
            jobs,
            new ServerConfig());

        await service.StartAsync(CancellationToken.None);
        service.SetGlobalLightOverride(7, applyImmediately: false); // deterministic level

        jobs.Invoke();
        Assert.Single(outgoing.Sent.Select(s => s.Packet).OfType<OverallLightLevelPacket>());

        jobs.Invoke(); // unchanged -> no resend
        Assert.Single(outgoing.Sent.Select(s => s.Packet).OfType<OverallLightLevelPacket>());
    }

    [Fact]
    public async Task ProcessLightAndTime_RemovesStaleSessions_AndResendsOnReentry()
    {
        var mobile = new MobileEntity { Id = new Serial(7), MapId = 0, Location = new Point3D(10, 10, 0) };
        var sessions = new StubPlayerSessions(InWorld(42, mobile.Id));
        var outgoing = new RecordingOutgoingQueue();
        var jobs = new CapturingJobService();
        var service = new LightAndTimeService(
            sessions,
            new Lazy<IMobileService>(() => new MapItemMobileService(mobile)),
            outgoing,
            new StubRegionResolver(null),
            jobs,
            new ServerConfig());

        await service.StartAsync(CancellationToken.None);
        service.SetGlobalLightOverride(7, applyImmediately: false);

        jobs.Invoke();
        Assert.Single(outgoing.Sent.Select(s => s.Packet).OfType<OverallLightLevelPacket>());

        sessions.Clear();
        jobs.Invoke(); // stale cleanup, nothing new
        Assert.Single(outgoing.Sent.Select(s => s.Packet).OfType<OverallLightLevelPacket>());

        sessions.Set(InWorld(42, mobile.Id));
        jobs.Invoke(); // delta cleared -> resends
        Assert.Equal(2, outgoing.Sent.Select(s => s.Packet).OfType<OverallLightLevelPacket>().Count());
    }

    [Fact]
    public async Task ProcessLightAndTime_OneFailingSession_IsIsolated_AndOthersStillUpdated()
    {
        var bad = new Serial(7);
        var good = new MobileEntity { Id = new Serial(8), MapId = 0, Location = new Point3D(10, 10, 0) };
        var sessions = new StubPlayerSessions(InWorld(42, bad), InWorld(43, good.Id));
        var outgoing = new RecordingOutgoingQueue();
        var jobs = new CapturingJobService();
        var service = new LightAndTimeService(
            sessions,
            new Lazy<IMobileService>(() => new ThrowingForIdMobileService(bad, good)),
            outgoing,
            new StubRegionResolver(null),
            jobs,
            new ServerConfig());

        await service.StartAsync(CancellationToken.None);
        service.SetGlobalLightOverride(7, applyImmediately: false);

        var ex = Record.Exception(jobs.Invoke); // bad session throws internally; must NOT bubble out
        Assert.Null(ex);

        // the good session (43) still received its light update
        Assert.Contains(outgoing.Sent, s => s.SessionId == 43 && s.Packet is OverallLightLevelPacket);
        Assert.DoesNotContain(outgoing.Sent, s => s.SessionId == 42);
    }

    private static PlayerSession InWorld(long sessionId, Serial mobileSerial)
        => new() { SessionId = sessionId, MobileSerial = mobileSerial, State = PlayerSessionStateType.InWorld };

    private static LightAndTimeService Build(RegionEntry? region)
        => new(
            new StubPlayerSessions(),
            new Lazy<IMobileService>(() => new ThrowingMobileService()),
            new RecordingOutgoingQueue(),
            new StubRegionResolver(region),
            new CapturingJobService(),
            new ServerConfig());

    private static RegionEntry Region(string type)
        => new(type, 0, "Map", "R", 1, new[] { new RegionAreaEntry(0, 0, 100, 100) }, "", null, null);

    private sealed class ThrowingForIdMobileService : IMobileService
    {
        private readonly Serial _badId;
        private readonly MobileEntity _good;

        public ThrowingForIdMobileService(Serial badId, MobileEntity good)
        {
            _badId = badId;
            _good = good;
        }

        public ValueTask<MobileEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
        {
            if (id == _badId)
            {
                throw new InvalidOperationException("boom");
            }

            return new(id == _good.Id ? _good : null);
        }

        public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public ValueTask<MobileEntity> CreateAsync(MobileEntity mobile, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public ValueTask<bool> DeleteAsync(Serial id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public ValueTask<bool> EquipAsync(
            MobileEntity mobile,
            Moongate.UO.Data.Entities.Items.ItemEntity item,
            Moongate.UO.Data.Types.Items.ItemLayerType layer,
            CancellationToken cancellationToken = default
        )
            => throw new NotSupportedException();

        public ValueTask<IReadOnlyList<MobileEntity>> GetByAccountIdAsync(
            Serial accountId,
            CancellationToken cancellationToken = default
        )
            => throw new NotSupportedException();

        public Moongate.UO.Data.Data.Mobiles.SkillEntry GetSkill(MobileEntity mobile, Moongate.UO.Data.Types.Skills.UOSkillName skill)
            => throw new NotSupportedException();

        public ValueTask<Moongate.UO.Data.Data.Mobiles.SkillEntry> SetSkillAsync(
            MobileEntity mobile,
            Moongate.UO.Data.Types.Skills.UOSkillName skill,
            double value,
            CancellationToken cancellationToken = default
        )
            => throw new NotSupportedException();

        public ValueTask<bool> UnequipAsync(
            MobileEntity mobile,
            Moongate.UO.Data.Types.Items.ItemLayerType layer,
            CancellationToken cancellationToken = default
        )
            => throw new NotSupportedException();
    }
}

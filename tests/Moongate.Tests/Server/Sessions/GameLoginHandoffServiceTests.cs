using Moongate.Abstractions.Data.Version;
using Moongate.Network.UO.Types.Login;
using Moongate.Server.Services.Sessions;

namespace Moongate.Tests.Server.Sessions;

public sealed class GameLoginHandoffServiceTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 6, 9, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void PruneExpired_RemovesExpiredEntries()
    {
        var now = FixedNow;
        var service = new GameLoginHandoffService(now: () => now);
        service.Store(0x00000003, ClientType.Classic, null);
        Assert.Equal(1, service.Count);

        now = FixedNow.AddMinutes(6);
        service.PruneExpired();

        Assert.Equal(0, service.Count);
    }

    [Fact]
    public void Store_ThenTryConsume_ReturnsHandoff()
    {
        var service = new GameLoginHandoffService(now: () => FixedNow);
        var version = new ClientVersion("7.0.0.0");
        service.Store(0xAABBCCDD, ClientType.Classic, version);

        var consumed = service.TryConsume(0xAABBCCDD, out var handoff);

        Assert.True(consumed);
        Assert.Equal(0xAABBCCDDu, handoff.SessionKey);
        Assert.Equal(ClientType.Classic, handoff.ClientType);
        Assert.Equal(version, handoff.ClientVersion);
    }

    [Fact]
    public void TryConsume_Expired_ReturnsFalse()
    {
        var now = FixedNow;
        var service = new GameLoginHandoffService(now: () => now);
        service.Store(0x00000002, ClientType.Classic, null);

        now = FixedNow.AddMinutes(6);

        Assert.False(service.TryConsume(0x00000002, out _));
    }

    [Fact]
    public void TryConsume_Twice_SecondReturnsFalse()
    {
        var service = new GameLoginHandoffService(now: () => FixedNow);
        service.Store(0x00000001, ClientType.Classic, null);

        Assert.True(service.TryConsume(0x00000001, out _));
        Assert.False(service.TryConsume(0x00000001, out _));
    }

    [Fact]
    public void TryConsume_UnknownKey_ReturnsFalse()
    {
        var service = new GameLoginHandoffService(now: () => FixedNow);

        Assert.False(service.TryConsume(0xDEADBEEF, out _));
    }
}

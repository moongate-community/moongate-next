using Moongate.Abstractions.Data.Version;
using Moongate.Abstractions.Types.Player;
using Moongate.Core.Ids;
using Moongate.Server.Data.Events;
using Moongate.Server.Services.Player;

namespace Moongate.Tests.Server.Player;

public sealed class PlayerSessionServiceTests
{
    [Fact]
    public void Authenticate_ExistingSession_UpdatesLogicalIdentity()
    {
        var service = new PlayerSessionService();
        service.GetOrCreateConnected(10, "127.0.0.1:1234", DateTimeOffset.UtcNow);

        var session = service.Authenticate(10, "user-1", "admin", DateTimeOffset.UtcNow);

        Assert.Equal("user-1", session.UserId);
        Assert.Equal("admin", session.Username);
        Assert.Equal(PlayerSessionStateType.Authenticated, session.State);
    }

    [Fact]
    public void Disconnect_ExistingInWorldSession_MarksDisconnectedAndRemovesMobileIndex()
    {
        var service = new PlayerSessionService();
        var mobileSerial = (Serial)0x00000010u;
        service.GetOrCreateConnected(10, null, DateTimeOffset.UtcNow);
        service.EnterWorld(10, (Serial)0x00000020u, mobileSerial, DateTimeOffset.UtcNow);

        var disconnected = service.Disconnect(10, DateTimeOffset.UtcNow);

        Assert.True(disconnected);
        Assert.True(service.TryGetBySessionId(10, out var session));
        Assert.Equal(PlayerSessionStateType.Disconnected, session.State);
        Assert.False(service.TryGetByMobileSerial(mobileSerial, out _));
    }

    [Fact]
    public void EnterWorld_ExistingSession_IndexesByMobileSerial()
    {
        var service = new PlayerSessionService();
        var characterSerial = (Serial)0x00000020u;
        var mobileSerial = (Serial)0x00000010u;
        service.GetOrCreateConnected(10, null, DateTimeOffset.UtcNow);

        var session = service.EnterWorld(10, characterSerial, mobileSerial, DateTimeOffset.UtcNow);

        Assert.Equal(PlayerSessionStateType.InWorld, session.State);
        Assert.Equal(characterSerial, session.CharacterSerial);
        Assert.Equal(mobileSerial, session.MobileSerial);
        Assert.True(service.TryGetByMobileSerial(mobileSerial, out var byMobile));
        Assert.Equal(10, byMobile.SessionId);
    }

    [Fact]
    public void EventSubscriptions_PlayerConnectedAndDisconnected_UpdateSession()
    {
        var service = new PlayerSessionService();
        var connectedAt = DateTimeOffset.UtcNow;

        service.Handle(new PlayerConnectedEvent(10, "127.0.0.1:2593", connectedAt));

        Assert.True(service.TryGetBySessionId(10, out var connected));
        Assert.Equal(PlayerSessionStateType.Connected, connected.State);
        Assert.Equal("127.0.0.1:2593", connected.RemoteEndPoint);

        service.Handle(new PlayerDisconnectedEvent(10, "127.0.0.1:2593", connectedAt.AddSeconds(1)));

        Assert.True(service.TryGetBySessionId(10, out var disconnected));
        Assert.Equal(PlayerSessionStateType.Disconnected, disconnected.State);
    }

    [Fact]
    public void Remove_ExistingSession_RemovesSessionAndMobileIndex()
    {
        var service = new PlayerSessionService();
        var mobileSerial = (Serial)0x00000010u;
        service.GetOrCreateConnected(10, null, DateTimeOffset.UtcNow);
        service.EnterWorld(10, (Serial)0x00000020u, mobileSerial, DateTimeOffset.UtcNow);

        Assert.True(service.Remove(10));

        Assert.False(service.TryGetBySessionId(10, out _));
        Assert.False(service.TryGetByMobileSerial(mobileSerial, out _));
        Assert.Equal(0, service.Count);
    }

    [Fact]
    public void UpdateClient_ExistingSession_UpdatesClientMetadata()
    {
        var service = new PlayerSessionService();
        service.GetOrCreateConnected(10, null, DateTimeOffset.UtcNow);

        var clientVersion = new ClientVersion(7, 0, 98, 13);

        var session = service.UpdateClient(10, clientVersion, 18);

        Assert.Equal(clientVersion, session.ClientVersion);
        Assert.Equal(18, session.ViewRange);
    }
}

using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Data.Player;
using Moongate.Abstractions.Data.Version;
using Moongate.Abstractions.Interfaces.Network;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Core.Ids;
using Moongate.Network.UO.Packets.Incoming.Login;
using Moongate.Server.Handlers.Characters;
using Moongate.Server.Interfaces.Services;
using Moongate.Tests.Support;
using Moongate.UO.Data.Data.Mobiles;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Types.Items;
using Moongate.UO.Data.Types.Skills;
using ZLinq;
using ZLinq.Linq;

namespace Moongate.Tests.Server.Characters;

public sealed class PlayCharacterHandlerTests
{
    private const long ConnectedSessionId = 42;
    private static readonly Serial AccountId = new(99);

    [Fact]
    public async Task HandleAsync_ValidSlot_BindsSessionAndEntersWorld()
    {
        var mobile = new MobileEntity { Id = new(7), Name = "Hero", AccountId = AccountId };
        var sessions = new FakePlayerSessionService();
        var mobiles = new FakeMobileService(AccountId, mobile);
        var worldEntry = new RecordingWorldEntryService();
        var handler = new PlayCharacterHandler(
            new NoopEventBusService(),
            new NoopNetworkSessionManager(),
            sessions,
            mobiles,
            worldEntry
        );

        await handler.HandleAsync(Context(0));

        Assert.True(sessions.EnterWorldCalled);
        Assert.Equal(mobile.Id, sessions.EnteredCharacterSerial);
        Assert.Same(mobile, worldEntry.Mobile);
        Assert.Equal(ConnectedSessionId, worldEntry.SessionId);
    }

    [Fact]
    public async Task HandleAsync_InvalidSlot_DoesNothing()
    {
        var mobile = new MobileEntity { Id = new(7), Name = "Hero", AccountId = AccountId };
        var sessions = new FakePlayerSessionService();
        var mobiles = new FakeMobileService(AccountId, mobile);
        var worldEntry = new RecordingWorldEntryService();
        var handler = new PlayCharacterHandler(
            new NoopEventBusService(),
            new NoopNetworkSessionManager(),
            sessions,
            mobiles,
            worldEntry
        );

        await handler.HandleAsync(Context(5));

        Assert.False(sessions.EnterWorldCalled);
        Assert.Null(worldEntry.Mobile);
    }

    private static PacketContext<PlayCharacterPacket> Context(int slot)
        => new(
            new FakeGameSession { SessionId = ConnectedSessionId },
            new() { Slot = slot, CharacterName = "Hero" },
            DateTimeOffset.UtcNow,
            static (_, _, _) => Task.CompletedTask,
            static () => [ConnectedSessionId]
        );

    private sealed class RecordingWorldEntryService : IWorldEntryService
    {
        public long? SessionId { get; private set; }
        public MobileEntity? Mobile { get; private set; }

        public ValueTask EnterWorldAsync(
            long sessionId,
            MobileEntity mobile,
            CancellationToken cancellationToken = default
        )
        {
            SessionId = sessionId;
            Mobile = mobile;

            return ValueTask.CompletedTask;
        }
    }

    private sealed class FakePlayerSessionService : IPlayerSessionService
    {
        public bool EnterWorldCalled { get; private set; }
        public Serial EnteredCharacterSerial { get; private set; }

        public int Count => throw new NotSupportedException();

        public PlayerSession Authenticate(long sessionId, Serial userId, string username, DateTimeOffset authenticatedAt)
            => throw new NotSupportedException();

        public bool Disconnect(long sessionId, DateTimeOffset disconnectedAt)
            => throw new NotSupportedException();

        public PlayerSession EnterWorld(
            long sessionId,
            Serial characterSerial,
            Serial mobileSerial,
            DateTimeOffset enteredWorldAt
        )
        {
            EnterWorldCalled = true;
            EnteredCharacterSerial = characterSerial;

            return new PlayerSession { SessionId = sessionId, CharacterSerial = characterSerial };
        }

        public IReadOnlyCollection<PlayerSession> GetAll()
            => throw new NotSupportedException();

        public PlayerSession GetOrCreateConnected(long sessionId, string? remoteEndPoint, DateTimeOffset connectedAt)
            => throw new NotSupportedException();

        public ValueEnumerable<FromArray<PlayerSession>, PlayerSession> Query()
            => throw new NotSupportedException();

        public bool Remove(long sessionId)
            => throw new NotSupportedException();

        public bool TryGetByMobileSerial(Serial mobileSerial, out PlayerSession session)
            => throw new NotSupportedException();

        public bool TryGetBySessionId(long sessionId, out PlayerSession session)
        {
            if (sessionId == ConnectedSessionId)
            {
                session = new PlayerSession { SessionId = ConnectedSessionId, UserId = AccountId };

                return true;
            }

            session = null!;

            return false;
        }

        public PlayerSession UpdateClient(long sessionId, ClientVersion? clientVersion = null, int? viewRange = null)
            => throw new NotSupportedException();
    }

}

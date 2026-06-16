using Moongate.Abstractions.Data.Network;
using Moongate.Abstractions.Data.Player;
using Moongate.Abstractions.Data.Version;
using Moongate.Abstractions.Interfaces.EventHandlers;
using Moongate.Abstractions.Interfaces.Events;
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

    private sealed class FakeMobileService : IMobileService
    {
        private readonly Serial _accountId;
        private readonly IReadOnlyList<MobileEntity> _mobiles;

        public FakeMobileService(Serial accountId, params MobileEntity[] mobiles)
        {
            _accountId = accountId;
            _mobiles = mobiles;
        }

        public ValueTask<IReadOnlyList<MobileEntity>> GetByAccountIdAsync(
            Serial accountId,
            CancellationToken cancellationToken = default
        )
            => ValueTask.FromResult(
                accountId.Equals(_accountId) ? _mobiles : (IReadOnlyList<MobileEntity>)[]
            );

        public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public ValueTask<MobileEntity> CreateAsync(MobileEntity mobile, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public ValueTask<bool> DeleteAsync(Serial id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public ValueTask<bool> EquipAsync(
            MobileEntity mobile,
            ItemEntity item,
            ItemLayerType layer,
            CancellationToken cancellationToken = default
        )
            => throw new NotSupportedException();

        public ValueTask<MobileEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public SkillEntry GetSkill(MobileEntity mobile, UOSkillName skill)
            => throw new NotSupportedException();

        public ValueTask<SkillEntry> SetSkillAsync(
            MobileEntity mobile,
            UOSkillName skill,
            double value,
            CancellationToken cancellationToken = default
        )
            => throw new NotSupportedException();

        public ValueTask<bool> UnequipAsync(
            MobileEntity mobile,
            ItemLayerType layer,
            CancellationToken cancellationToken = default
        )
            => throw new NotSupportedException();
    }

    private sealed class NoopNetworkSessionManager : INetworkSessionManager
    {
        public int Count => throw new NotSupportedException();

        public IReadOnlyCollection<long> GetSessionIds()
            => throw new NotSupportedException();

        public ValueEnumerable<FromArray<long>, long> QuerySessionIds()
            => throw new NotSupportedException();

        public bool TryGetSession(long sessionId, out IGameSession session)
            => throw new NotSupportedException();
    }

    private sealed class NoopEventBusService : IEventBusService
    {
        public Action<Type, Exception, IMoongateEvent>? OnEventError { get; set; }

        public int CurrentTickQueueDepth => throw new NotSupportedException();

        public int DrainTickEvents(int maxItems)
            => throw new NotSupportedException();

        public void Publish<TEvent>(TEvent evt)
            where TEvent : ITickEvent
            => throw new NotSupportedException();

        public Task PublishAsync<TEvent>(TEvent evt, CancellationToken cancellationToken = default)
            where TEvent : IAsyncEvent
            => throw new NotSupportedException();

        public Task StartAsync(CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task StopAsync(CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }
}

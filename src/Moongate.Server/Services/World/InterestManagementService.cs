using Moongate.Abstractions.Data.Player;
using Moongate.Abstractions.Interfaces.Player;
using Moongate.Core.Ids;
using Moongate.Network.UO.Packets.Outgoing.Entity;
using Moongate.Server.Data.Events;
using Moongate.Server.Data.Internal.Packets;
using Moongate.Server.Interfaces.Network;
using Moongate.Server.Interfaces.Services.World;
using Moongate.UO.Data.Entities.Items;
using Moongate.UO.Data.Entities.Mobiles;
using Moongate.UO.Data.Interfaces.Services;
using Moongate.UO.Data.Types.Items;
using Moongate.UO.Data.Utils;

namespace Moongate.Server.Services.World;

/// <summary>
///     Per-session known-set interest manager: emits 0x78/0x77/0x1D/0xF3 as entities enter,
///     move within, and leave each player's view.
/// </summary>
public sealed class InterestManagementService : IInterestManagementService
{
    private readonly IWorldSpatialIndex _index;
    private readonly IItemService _items;
    private readonly Dictionary<long, HashSet<Serial>> _known = [];
    private readonly IOutgoingPacketQueue _outgoing;
    private readonly IPlayerSessionService _sessions;
    private readonly Lock _sync = new();

    public InterestManagementService(
        IWorldSpatialIndex index,
        IOutgoingPacketQueue outgoing,
        IPlayerSessionService sessions,
        IItemService items
    )
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(outgoing);
        ArgumentNullException.ThrowIfNull(sessions);
        ArgumentNullException.ThrowIfNull(items);

        _index = index;
        _outgoing = outgoing;
        _sessions = sessions;
        _items = items;
    }

    public async Task SendInitialSnapshotAsync(
        long sessionId, MobileEntity viewer, CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(viewer);

        var range = ViewRangeOf(sessionId);

        foreach (var mobile in _index.GetMobilesInRange(viewer.MapId, viewer.Location, range))
        {
            if (mobile.Id == viewer.Id)
            {
                continue;
            }

            var equipped = await ResolveEquippedAsync(mobile, cancellationToken);
            _outgoing.Enqueue(sessionId, new MobileIncomingPacket(mobile, equipped));
            Remember(sessionId, mobile.Id);
        }

        foreach (var item in _index.GetItemsInRange(viewer.MapId, viewer.Location, range))
        {
            _outgoing.Enqueue(sessionId, new ObjectInformationPacket(item));
            Remember(sessionId, item.Id);
        }

        // reciprocal: announce the newcomer to every already-present player whose view covers it
        var viewerEquipped = await ResolveEquippedAsync(viewer, cancellationToken);

        foreach (var observer in _index.GetPlayersInRange(viewer.MapId, viewer.Location, MapSectorConsts.MaxViewRange))
        {
            if (observer.Id == viewer.Id || !_sessions.TryGetByMobileSerial(observer.Id, out var observerSession))
            {
                continue;
            }

            if (!observer.Location.InRange(viewer.Location, ViewRangeOf(observerSession.SessionId)))
            {
                continue;
            }

            if (!IsKnown(observerSession.SessionId, viewer.Id))
            {
                _outgoing.Enqueue(observerSession.SessionId, new MobileIncomingPacket(viewer, viewerEquipped));
                Remember(observerSession.SessionId, viewer.Id);
            }
        }
    }

    public async Task OnMobileMovedAsync(MobileMovedEvent evt, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evt);

        if (!_index.TryGet(evt.MobileId, out var mover))
        {
            return;
        }

        // observer-side: every player near the old or new location updates its knowledge of the mover
        var observers = new Dictionary<long, (PlayerSession Session, MobileEntity Mobile)>();

        foreach (var location in new[] { evt.OldLocation, evt.NewLocation })
        {
            foreach (var player in _index.GetPlayersInRange(evt.MapId, location, MapSectorConsts.MaxViewRange))
            {
                if (player.Id == mover.Id || !_sessions.TryGetByMobileSerial(player.Id, out var session))
                {
                    continue;
                }

                observers.TryAdd(session.SessionId, (session, player));
            }
        }

        foreach (var (sessionId, (session, observer)) in observers)
        {
            var inView = observer.Location.InRange(evt.NewLocation, ViewRangeOf(sessionId));
            var wasKnown = IsKnown(sessionId, mover.Id);

            if (inView && !wasKnown)
            {
                var equipped = await ResolveEquippedAsync(mover, cancellationToken);
                _outgoing.Enqueue(sessionId, new MobileIncomingPacket(mover, equipped));
                Remember(sessionId, mover.Id);
            }
            else if (inView)
            {
                _outgoing.Enqueue(sessionId, new MobileMovingPacket(mover));
            }
            else if (wasKnown)
            {
                _outgoing.Enqueue(sessionId, new DeleteObjectPacket(mover.Id));
                Forget(sessionId, mover.Id);
            }
        }

        // mover-side: a player mover re-evaluates its own view (mobiles + items)
        if (mover.IsPlayer && _sessions.TryGetByMobileSerial(mover.Id, out var moverSession))
        {
            await RefreshMoverViewAsync(moverSession.SessionId, mover, cancellationToken);
        }
    }

    public void OnEntityRemoved(Serial entityId)
    {
        List<long> observers = [];

        lock (_sync)
        {
            foreach (var (sessionId, known) in _known)
            {
                if (known.Remove(entityId))
                {
                    observers.Add(sessionId);
                }
            }
        }

        foreach (var sessionId in observers)
        {
            _outgoing.Enqueue(sessionId, new DeleteObjectPacket(entityId));
        }
    }

    public void ForgetSession(long sessionId)
    {
        lock (_sync)
        {
            _known.Remove(sessionId);
        }
    }

    private async Task RefreshMoverViewAsync(long sessionId, MobileEntity mover, CancellationToken cancellationToken)
    {
        var range = ViewRangeOf(sessionId);
        var visible = new HashSet<Serial>();

        foreach (var mobile in _index.GetMobilesInRange(mover.MapId, mover.Location, range))
        {
            if (mobile.Id == mover.Id)
            {
                continue;
            }

            visible.Add(mobile.Id);

            if (!IsKnown(sessionId, mobile.Id))
            {
                var equipped = await ResolveEquippedAsync(mobile, cancellationToken);
                _outgoing.Enqueue(sessionId, new MobileIncomingPacket(mobile, equipped));
                Remember(sessionId, mobile.Id);
            }
        }

        foreach (var item in _index.GetItemsInRange(mover.MapId, mover.Location, range))
        {
            visible.Add(item.Id);

            if (!IsKnown(sessionId, item.Id))
            {
                _outgoing.Enqueue(sessionId, new ObjectInformationPacket(item));
                Remember(sessionId, item.Id);
            }
        }

        foreach (var goneId in KnownSnapshot(sessionId))
        {
            if (!visible.Contains(goneId) && goneId != mover.Id)
            {
                _outgoing.Enqueue(sessionId, new DeleteObjectPacket(goneId));
                Forget(sessionId, goneId);
            }
        }
    }

    private async Task<IReadOnlyList<(ItemLayerType Layer, ItemEntity Item)>> ResolveEquippedAsync(
        MobileEntity mobile, CancellationToken cancellationToken
    )
    {
        List<(ItemLayerType, ItemEntity)> result = [];

        foreach (var (layer, serial) in WornItemLayers.VisibleEquipped(mobile))
        {
            var item = await _items.GetByIdAsync(serial, cancellationToken);

            if (item is not null)
            {
                result.Add((layer, item));
            }
        }

        return result;
    }

    private int ViewRangeOf(long sessionId)
    {
        return _sessions.TryGetBySessionId(sessionId, out var session) && session.ViewRange is > 0
            ? session.ViewRange.Value
            : MapSectorConsts.MaxViewRange;
    }

    private void Remember(long sessionId, Serial entityId)
    {
        lock (_sync)
        {
            if (!_known.TryGetValue(sessionId, out var set))
            {
                set = [];
                _known[sessionId] = set;
            }

            set.Add(entityId);
        }
    }

    private bool IsKnown(long sessionId, Serial entityId)
    {
        lock (_sync)
        {
            return _known.TryGetValue(sessionId, out var set) && set.Contains(entityId);
        }
    }

    private void Forget(long sessionId, Serial entityId)
    {
        lock (_sync)
        {
            if (_known.TryGetValue(sessionId, out var set))
            {
                set.Remove(entityId);
            }
        }
    }

    private IReadOnlyList<Serial> KnownSnapshot(long sessionId)
    {
        lock (_sync)
        {
            return _known.TryGetValue(sessionId, out var set) ? set.ToArray() : [];
        }
    }
}

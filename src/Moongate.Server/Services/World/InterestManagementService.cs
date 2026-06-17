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
/// Per-session known-set interest manager: emits 0x78/0x77/0x1D/0xF3 as entities enter,
/// move within, and leave each player's view.
/// </summary>
public sealed class InterestManagementService : IInterestManagementService
{
    private readonly Lock _sync = new();
    private readonly Dictionary<long, HashSet<Serial>> _known = [];

    private readonly IWorldSpatialIndex _index;
    private readonly IOutgoingPacketQueue _outgoing;
    private readonly IPlayerSessionService _sessions;
    private readonly IItemService _items;

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

    public async Task SendInitialSnapshotAsync(long sessionId, MobileEntity viewer, CancellationToken cancellationToken = default)
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
    }

    public Task OnMobileMovedAsync(MobileMovedEvent evt, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

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

            if (_sessions.TryGetByMobileSerial(entityId, out var session))
            {
                _known.Remove(session.SessionId);
            }
        }

        foreach (var sessionId in observers)
        {
            _outgoing.Enqueue(sessionId, new DeleteObjectPacket(entityId));
        }
    }

    private async Task<IReadOnlyList<(ItemLayerType Layer, ItemEntity Item)>> ResolveEquippedAsync(MobileEntity mobile, CancellationToken cancellationToken)
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
        => _sessions.TryGetBySessionId(sessionId, out var session) && session.ViewRange is > 0
            ? session.ViewRange.Value
            : MapSectorConsts.MaxViewRange;

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
}

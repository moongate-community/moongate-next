using Moongate.Core.Ids;
using Moongate.Server.Data.Events;
using Moongate.UO.Data.Entities.Mobiles;

namespace Moongate.Server.Interfaces.Services.World;

/// <summary>
/// Tracks which entities each player's client knows about and emits draw/move/delete packets
/// as entities enter, move within, and leave each client's view.
/// </summary>
public interface IInterestManagementService
{
    /// <summary>Sends every in-range mobile and ground item to a newly-entered player and seeds its known-set.</summary>
    Task SendInitialSnapshotAsync(long sessionId, MobileEntity viewer, CancellationToken cancellationToken = default);

    /// <summary>Reacts to a mobile's completed step: updates observers and (for a player mover) its own view.</summary>
    Task OnMobileMovedAsync(MobileMovedEvent evt, CancellationToken cancellationToken = default);

    /// <summary>Removes an entity from every client that knew it (sends delete).</summary>
    void OnEntityRemoved(Serial entityId);

    /// <summary>Drops all knowledge tracked for a disconnected session.</summary>
    void ForgetSession(long sessionId);
}

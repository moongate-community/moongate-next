using Moongate.UO.Data.Entities.Mobiles;

namespace Moongate.Server.Interfaces.Services;

/// <summary>
///     Builds and sends the world-entry packet sequence for a player entering the world.
/// </summary>
public interface IWorldEntryService
{
    /// <summary>
    ///     Sends the world-entry packet sequence for <paramref name="mobile" /> to the given session.
    /// </summary>
    ValueTask EnterWorldAsync(long sessionId, MobileEntity mobile, CancellationToken cancellationToken = default);
}

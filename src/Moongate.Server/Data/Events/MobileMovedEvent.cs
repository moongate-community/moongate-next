using Moongate.Abstractions.Interfaces.Events;
using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Core.Types;

namespace Moongate.Server.Data.Events;

/// <summary>
///     Raised after a mobile completes a positional step. Broadcast seam for interest management (#3).
/// </summary>
public sealed record MobileMovedEvent(
    Serial MobileId,
    int MapId,
    Point3D OldLocation,
    Point3D NewLocation,
    DirectionType Direction
) : IAsyncEvent;

using Moongate.Core.Geometry;
using Moongate.Core.Types;
using Moongate.UO.Data.Entities.Mobiles;

namespace Moongate.Server.Interfaces.Services.Movement;

/// <summary>
///     Server-authoritative movement validation against the map (bounds, diagonal, Z, statics).
/// </summary>
public interface IMovementValidationService
{
    /// <summary>Resolves the destination for a move; returns false when the move is not allowed.</summary>
    bool TryResolveMove(MobileEntity mobile, DirectionType direction, out Point3D newLocation);
}

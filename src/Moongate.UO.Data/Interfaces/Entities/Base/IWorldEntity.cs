using Moongate.Core.Geometry;
using Moongate.Core.Ids;
using Moongate.Core.Types;

namespace Moongate.UO.Data.Interfaces.Entities.Base;

/// <summary>
/// Common contract shared by every entity that exists in the game world
/// (items and mobiles): identity, name, spatial placement, facing, coloring
/// and weight.
/// </summary>
public interface IWorldEntity
{
    /// <summary>Unique serial identifier of the entity.</summary>
    Serial Id { get; set; }

    /// <summary>Display name of the entity, or null when it has none.</summary>
    string? Name { get; set; }

    /// <summary>Direction the entity is currently facing.</summary>
    DirectionType Direction { get; set; }

    /// <summary>World coordinates (x, y, z) of the entity on its map.</summary>
    Point3D Location { get; set; }

    /// <summary>Color hue applied to the entity; <see cref="Ids.Hue.None" /> for default coloring.</summary>
    Hue Hue { get; set; }

    /// <summary>Identifier of the map facet the entity belongs to; resolve the runtime map via a map service.</summary>
    int MapId { get; set; }
}

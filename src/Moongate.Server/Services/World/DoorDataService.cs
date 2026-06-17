using Moongate.Core.Geometry;
using Moongate.Server.Data.World;
using Moongate.Server.Interfaces.Services.World;
using Moongate.Server.Services.World.Internal;
using Moongate.Server.Services.WorldData;

namespace Moongate.Server.Services.World;

/// <summary>
///     Lazy in-memory storage for door component metadata and precomputed toggle definitions.
/// </summary>
public class DoorDataService : LazyDataService, IDoorDataService
{
    private static readonly Point3D[] _offsetsByDoorFacing =
    [
        new(-1, 1, 0),
        new(1, 1, 0),
        new(-1, 0, 0),
        new(1, -1, 0),
        new(1, 1, 0),
        new(1, -1, 0),
        new(0, 0, 0),
        new(0, -1, 0)
    ];

    private static readonly int[] _pieceIndexToDoorFacing =
    [
        2,
        3,
        0,
        1,
        4,
        5,
        6,
        7
    ];

    private readonly ServerAssetDataLoader? _loader;
    private readonly Lock _sync = new();
    private List<DoorComponentEntry> _entries = [];
    private Dictionary<int, DoorToggleDefinition> _toggleByItemId = [];

    public DoorDataService()
    {
    }

    public DoorDataService(ServerAssetDataLoader loader)
    {
        ArgumentNullException.ThrowIfNull(loader);

        _loader = loader;
    }

    public IReadOnlyList<DoorComponentEntry> GetAllEntries()
    {
        EnsureLoaded();

        lock (_sync)
        {
            return [.. _entries];
        }
    }

    public void SetEntries(IReadOnlyList<DoorComponentEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var snapshot = entries.ToList();

        lock (_sync)
        {
            _entries = snapshot;
            _toggleByItemId = BuildToggleMap(snapshot);
        }

        MarkLoaded();
    }

    public bool TryGetToggleDefinition(int itemId, out DoorToggleDefinition definition)
    {
        EnsureLoaded();

        lock (_sync)
        {
            return _toggleByItemId.TryGetValue(itemId, out definition);
        }
    }

    protected override void LoadCore()
    {
        _loader?.LoadDoors(this);
    }

    private static Dictionary<int, DoorToggleDefinition> BuildToggleMap(IReadOnlyList<DoorComponentEntry> entries)
    {
        var map = new Dictionary<int, DoorToggleDefinition>();

        foreach (var entry in entries)
        {
            Span<int> pieces =
            [
                entry.Piece1,
                entry.Piece2,
                entry.Piece3,
                entry.Piece4,
                entry.Piece5,
                entry.Piece6,
                entry.Piece7,
                entry.Piece8
            ];

            for (var pieceIndex = 0; pieceIndex < pieces.Length; pieceIndex++)
            {
                var closedId = pieces[pieceIndex];

                if (closedId <= 0)
                {
                    continue;
                }

                var openedId = checked(closedId + 1);
                var doorFacingIndex = _pieceIndexToDoorFacing[pieceIndex];
                var offset = _offsetsByDoorFacing[doorFacingIndex];

                map[closedId] = new DoorToggleDefinition(closedId, openedId, true, offset);
                map[openedId] = new DoorToggleDefinition(openedId, closedId, false, InvertOffset(offset));

                var legacyClosedId = closedId - 1;

                if (legacyClosedId > 0 && !map.ContainsKey(legacyClosedId))
                {
                    map[legacyClosedId] = new DoorToggleDefinition(legacyClosedId, openedId, true, offset);
                }
            }
        }

        return map;
    }

    private static Point3D InvertOffset(Point3D offset)
    {
        return new Point3D(-offset.X, -offset.Y, -offset.Z);
    }
}

using Moongate.UO.Data.Data.Maps;
using Moongate.UO.Data.Data.Tiles;
using Moongate.UO.Data.Interfaces.Files;
using Moongate.UO.Data.Tiles;

namespace Moongate.UO.Data.Maps;

/// <summary>
/// A single map facet: its <see cref="MapDefinition" /> plus a lazily-built <see cref="TileMatrix" />
/// for querying land and static geography by coordinate.
/// </summary>
public sealed class Map
{
    private readonly IUoFileResolver _resolver;
    private readonly MapDefinition _definition;
    private TileMatrix? _tiles;

    public Map(MapDefinition definition, IUoFileResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(resolver);

        _definition = definition;
        _resolver = resolver;
    }

    public int MapId => _definition.MapId;

    public string Name => _definition.Name;

    public int Width => _definition.Width;

    public int Height => _definition.Height;

    public TileMatrix Tiles => _tiles ??= new TileMatrix(
        _resolver,
        _definition.FileIndex,
        _definition.MapId,
        _definition.Width,
        _definition.Height
    );

    public LandTile GetLandTile(int x, int y)
    {
        return Tiles.GetLandTile(x, y);
    }

    public StaticTile[] GetStaticTiles(int x, int y)
    {
        return Tiles.GetStaticTiles(x, y);
    }
}

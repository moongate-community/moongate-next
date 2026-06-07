using Moongate.UO.Data.Data.Tiles;
using Moongate.UO.Data.Interfaces.Files;
using Moongate.UO.Data.Interfaces.Maps;
using Moongate.UO.Data.Interfaces.Tiles;
using Moongate.UO.Data.Tiles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Moongate.UO.Data.Maps;

/// <summary>
/// Renders UO map tiles into radar-colour images.
/// </summary>
public sealed class MapImageService : IMapImageService
{
    private const int TileBlockSize = 8;

    private readonly IMapService _maps;
    private readonly IUoFileResolver _resolver;
    private readonly IRadarColorStore _radarColors;
    private readonly ITileDataStore _tileData;

    public MapImageService(
        IMapService maps,
        IUoFileResolver resolver,
        IRadarColorStore radarColors,
        ITileDataStore tileData
    )
    {
        ArgumentNullException.ThrowIfNull(maps);
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(radarColors);
        ArgumentNullException.ThrowIfNull(tileData);

        _maps = maps;
        _resolver = resolver;
        _radarColors = radarColors;
        _tileData = tileData;
    }

    public Image? GetMapImage(int mapId)
    {
        var map = _maps.GetMap(mapId);

        if (map is null || !HasMapFile(map.FileIndex))
        {
            return null;
        }

        var image = new Image<Rgb24>(map.Width, map.Height);
        var tiles = map.Tiles;

        for (var blockX = 0; blockX < tiles.BlockWidth; blockX++)
        {
            for (var blockY = 0; blockY < tiles.BlockHeight; blockY++)
            {
                RenderBlock(image, tiles, blockX, blockY);
            }
        }

        return image;
    }

    private bool HasMapFile(int fileIndex)
        => _resolver.Contains($"map{fileIndex}.mul") ||
           _resolver.Contains($"map{fileIndex}LegacyMUL.uop");

    private void RenderBlock(Image<Rgb24> image, TileMatrix tiles, int blockX, int blockY)
    {
        var landBlock = tiles.GetLandBlock(blockX, blockY);
        var staticBlock = tiles.GetStaticBlock(blockX, blockY);

        for (var tileX = 0; tileX < TileBlockSize; tileX++)
        {
            for (var tileY = 0; tileY < TileBlockSize; tileY++)
            {
                var px = blockX * TileBlockSize + tileX;
                var py = blockY * TileBlockSize + tileY;

                if (px >= image.Width || py >= image.Height)
                {
                    continue;
                }

                image[px, py] = ResolveTileColor(landBlock[(tileY << 3) + tileX], staticBlock[tileX][tileY]);
            }
        }
    }

    private Rgb24 ResolveTileColor(LandTile land, StaticTile[] statics)
    {
        var topZ = land.Z;
        var color = land.ID > 0 ? _radarColors.GetLandColor(land.ID) : ((byte)0, (byte)0, (byte)0);

        for (var i = 0; i < statics.Length; i++)
        {
            var tile = statics[i];
            var top = tile.Z + _tileData.GetItem(tile.ID).CalcHeight;

            if (top >= topZ)
            {
                topZ = top;
                color = _radarColors.GetStaticColor(tile.ID);
            }
        }

        return new(color.Item1, color.Item2, color.Item3);
    }
}

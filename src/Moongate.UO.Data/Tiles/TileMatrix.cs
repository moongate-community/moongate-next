using Moongate.UO.Data.Data.Internal;
using Moongate.UO.Data.Data.Tiles;
using Moongate.UO.Data.Files.Internal;
using Moongate.UO.Data.Interfaces.Files;
using Moongate.UO.Data.Tiles.Internal;
using Serilog;

namespace Moongate.UO.Data.Tiles;

/// <summary>
///     Reads land and static tile blocks for a single map facet from <c>map{n}</c> (mul or uop),
///     <c>staidx{n}.mul</c> and <c>statics{n}.mul</c>. Blocks are cached on first access.
/// </summary>
public sealed class TileMatrix : IDisposable
{
    private const int SectorShift = 3;

    private static readonly ILogger _logger = Log.ForContext<TileMatrix>();
    private readonly FileStream? _dataStream;
    private readonly StaticTile[][][] _emptyStaticBlock;
    private readonly BinaryReader? _indexReader;
    private readonly FileStream? _indexStream;
    private readonly LandTile[] _invalidLandBlock;
    private readonly LandTile[][][] _landTiles;
    private readonly FileStream? _mapStream;

    private readonly StaticTile[][][][][] _staticTiles;
    private readonly UopEntry[]? _uopMapEntries;
    private bool _landWarned;
    private TileList[][]? _lists;
    private bool _staticWarned;
    private StaticTile[] _tileBuffer = new StaticTile[128];

    public TileMatrix(IUoFileResolver resolver, int fileIndex, int mapId, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(resolver);

        BlockWidth = width >> SectorShift;
        BlockHeight = height >> SectorShift;

        if (mapId != 0x7F)
        {
            var mapPath = resolver.Resolve($"map{fileIndex}.mul");

            if (mapPath != null)
            {
                _mapStream = new FileStream(mapPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            else
            {
                mapPath = resolver.Resolve($"map{fileIndex}LegacyMUL.uop");

                if (mapPath != null)
                {
                    _mapStream = new FileStream(mapPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                    var uopEntries = UopIndexReader.ReadIndexes(_mapStream, ".dat", 0x14000, 5);
                    _uopMapEntries = new UopEntry[uopEntries.Count];
                    uopEntries.Values.CopyTo(_uopMapEntries, 0);

                    ConvertToMapEntries(_mapStream);
                }
                else
                {
                    _logger.Warning("{File} was not found.", $"map{fileIndex}.mul");
                }
            }

            var indexPath = resolver.Resolve($"staidx{fileIndex}.mul");

            if (indexPath != null)
            {
                _indexStream = new FileStream(indexPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                _indexReader = new BinaryReader(_indexStream);
            }
            else
            {
                _logger.Warning("{File} was not found.", $"staidx{fileIndex}.mul");
            }

            var staticsPath = resolver.Resolve($"statics{fileIndex}.mul");

            if (staticsPath != null)
            {
                _dataStream = new FileStream(staticsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            else
            {
                _logger.Warning("{File} was not found.", $"statics{fileIndex}.mul");
            }
        }

        _emptyStaticBlock = new StaticTile[8][][];

        for (var i = 0; i < 8; ++i)
        {
            _emptyStaticBlock[i] = new StaticTile[8][];

            for (var j = 0; j < 8; ++j)
            {
                _emptyStaticBlock[i][j] = [];
            }
        }

        _invalidLandBlock = new LandTile[196];
        _landTiles = new LandTile[BlockWidth][][];
        _staticTiles = new StaticTile[BlockWidth][][][][];
    }

    public int BlockWidth { get; }

    public int BlockHeight { get; }

    public void Dispose()
    {
        _indexReader?.Dispose();
        _mapStream?.Dispose();
        _indexStream?.Dispose();
        _dataStream?.Dispose();
    }

    public LandTile[] GetLandBlock(int x, int y)
    {
        if (x < 0 || y < 0 || x >= BlockWidth || y >= BlockHeight || _mapStream == null)
        {
            return _invalidLandBlock;
        }

        _landTiles[x] ??= new LandTile[BlockHeight][];

        var tiles = _landTiles[x][y];

        if (tiles != null)
        {
            return tiles;
        }

        tiles = ReadLandBlock(x, y);
        _landTiles[x][y] = tiles;

        return tiles;
    }

    public LandTile GetLandTile(int x, int y)
    {
        var tiles = GetLandBlock(x >> SectorShift, y >> SectorShift);

        return tiles[((y & 0x7) << 3) + (x & 0x7)];
    }

    public StaticTile[][][] GetStaticBlock(int x, int y)
    {
        if (x < 0 || y < 0 || x >= BlockWidth || y >= BlockHeight || _dataStream == null || _indexStream == null)
        {
            return _emptyStaticBlock;
        }

        _staticTiles[x] ??= new StaticTile[BlockHeight][][][];

        var tiles = _staticTiles[x][y];

        if (tiles == null)
        {
            tiles = ReadStaticBlock(x, y);
            _staticTiles[x][y] = tiles;
        }

        return tiles;
    }

    public StaticTile[] GetStaticTiles(int x, int y)
    {
        var block = GetStaticBlock(x >> SectorShift, y >> SectorShift);

        return block[x & 0x7][y & 0x7];
    }

    private void ConvertToMapEntries(FileStream stream)
    {
        Array.Sort(_uopMapEntries!, (a, b) => a.Offset.CompareTo(b.Offset));

        var reader = new BinaryReader(stream);

        for (var i = 0; i < _uopMapEntries!.Length; ++i)
        {
            var entry = _uopMapEntries[i];
            stream.Seek(entry.Offset, SeekOrigin.Begin);
            _uopMapEntries[i].Extra = reader.ReadInt32();
        }

        Array.Sort(_uopMapEntries, (a, b) => a.Extra.CompareTo(b.Extra));
    }

    private long FindOffset(long offset)
    {
        var total = 0L;

        for (var i = 0; i < _uopMapEntries!.Length; ++i)
        {
            var entry = _uopMapEntries[i];
            var newTotal = total + entry.Size;

            if (offset < newTotal)
            {
                return entry.Offset + (offset - total);
            }

            total = newTotal;
        }

        return -1;
    }

    private unsafe LandTile[] ReadLandBlock(int x, int y)
    {
        try
        {
            long offset = (x * BlockHeight + y) * 196 + 4;

            if (_uopMapEntries != null)
            {
                offset = FindOffset(offset);
            }

            _mapStream!.Seek(offset, SeekOrigin.Begin);

            var tiles = new LandTile[64];

            fixed (LandTile* pTiles = tiles)
            {
                _ = _mapStream.Read(new Span<byte>(pTiles, 192));
            }

            return tiles;
        }
        catch (Exception)
        {
            if (!_landWarned)
            {
                _logger.Warning("Land end-of-stream at block ({X}, {Y})", x, y);
                _landWarned = true;
            }

            return _invalidLandBlock;
        }
    }

    private unsafe StaticTile[][][] ReadStaticBlock(int x, int y)
    {
        try
        {
            _indexReader!.BaseStream.Seek((x * BlockHeight + y) * 12, SeekOrigin.Begin);

            var lookup = _indexReader.ReadInt32();
            var length = _indexReader.ReadInt32();

            if (lookup < 0 || length <= 0)
            {
                return _emptyStaticBlock;
            }

            var count = length / 7;

            _dataStream!.Seek(lookup, SeekOrigin.Begin);

            if (_tileBuffer.Length < count)
            {
                _tileBuffer = new StaticTile[count];
            }

            var staTiles = _tileBuffer;

            fixed (StaticTile* pTiles = staTiles)
            {
                _ = _dataStream.Read(new Span<byte>(pTiles, length));

                if (_lists == null)
                {
                    _lists = new TileList[8][];

                    for (var i = 0; i < 8; ++i)
                    {
                        _lists[i] = new TileList[8];

                        for (var j = 0; j < 8; ++j)
                        {
                            _lists[i][j] = new TileList();
                        }
                    }
                }

                var lists = _lists;

                StaticTile* pCur = pTiles,
                    pEnd = pTiles + count;

                while (pCur < pEnd)
                {
                    lists[pCur->m_X & 0x7][pCur->m_Y & 0x7].Add(*pCur);

                    pCur += 1;
                }

                var tiles = new StaticTile[8][][];

                for (var i = 0; i < 8; ++i)
                {
                    tiles[i] = new StaticTile[8][];

                    for (var j = 0; j < 8; ++j)
                    {
                        tiles[i][j] = lists[i][j].ToArray();
                    }
                }

                return tiles;
            }
        }
        catch (EndOfStreamException)
        {
            if (!_staticWarned)
            {
                _logger.Warning("Static end-of-stream at block ({X}, {Y})", x, y);
                _staticWarned = true;
            }

            return _emptyStaticBlock;
        }
    }
}

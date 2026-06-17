using System.Buffers.Binary;
using System.IO.Compression;
using Moongate.UO.Data.Data.Multi;
using Moongate.UO.Data.Files.Internal;
using Moongate.UO.Data.Interfaces.Files;
using Moongate.UO.Data.Interfaces.Multi;
using Moongate.UO.Data.Types.Tiles;
using Serilog;

namespace Moongate.UO.Data.Multi;

/// <summary>
///     Loads UO multi component lists, preferring <c>MultiCollection.uop</c> (zlib) and falling back to
///     classic <c>multi.idx</c>/<c>multi.mul</c>. A missing multi file yields an empty store.
/// </summary>
public sealed class MultiDataStore : IMultiDataStore
{
    private static readonly ILogger _logger = Log.ForContext<MultiDataStore>();

    private readonly Dictionary<int, MultiComponentList> _components;

    public MultiDataStore(IUoFileResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);

        _components = new Dictionary<int, MultiComponentList>();

        var uopPath = resolver.Resolve("MultiCollection.uop");

        if (uopPath != null)
        {
            LoadUop(uopPath);
        }
        else
        {
            var idxPath = resolver.Resolve("multi.idx");
            var mulPath = resolver.Resolve("multi.mul");

            if (idxPath != null && mulPath != null)
            {
                LoadMul(idxPath, mulPath);
            }
            else
            {
                _logger.Warning("No multi data files (MultiCollection.uop or multi.idx/mul) were found.");
            }
        }

        _logger.Information("Loaded {Count} multi components.", _components.Count);
    }

    public int Count => _components.Count;

    public MultiComponentList GetComponents(int multiId)
    {
        return _components.GetValueOrDefault(multiId & 0x3FFF, MultiComponentList.Empty);
    }

    public static List<MultiTileEntry> ParseUopEntry(ReadOnlySpan<byte> data)
    {
        var list = new List<MultiTileEntry>();
        var pos = 4; // skip first 4 bytes

        var count = BinaryPrimitives.ReadUInt32LittleEndian(data[pos..]);
        pos += 4;

        for (uint t = 0; t < count; t++)
        {
            var itemId = BinaryPrimitives.ReadUInt16LittleEndian(data[pos..]);
            pos += 2;
            var x = BinaryPrimitives.ReadInt16LittleEndian(data[pos..]);
            pos += 2;
            var y = BinaryPrimitives.ReadInt16LittleEndian(data[pos..]);
            pos += 2;
            var z = BinaryPrimitives.ReadInt16LittleEndian(data[pos..]);
            pos += 2;
            var flagValue = BinaryPrimitives.ReadUInt16LittleEndian(data[pos..]);
            pos += 2;

            var flags = flagValue switch
            {
                1 => UoTileFlag.None,
                257 => UoTileFlag.Generic,
                _ => UoTileFlag.Background
            };

            var clilocsCount = BinaryPrimitives.ReadUInt32LittleEndian(data[pos..]);
            pos += 4;
            pos += (int)clilocsCount * 4;

            list.Add(new MultiTileEntry(itemId, x, y, z, flags));
        }

        return list;
    }

    private void LoadMul(string idxPath, string mulPath)
    {
        using var idx = new FileStream(idxPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var idxReader = new BinaryReader(idx);
        using var mul = new FileStream(mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var mulReader = new BinaryReader(mul);

        var count = (int)(idx.Length / 12);

        for (var i = 0; i < count; i++)
        {
            var lookup = idxReader.ReadInt32();
            var length = idxReader.ReadInt32();
            idx.Seek(4, SeekOrigin.Current); // extra

            if (lookup < 0 || length <= 0)
            {
                continue;
            }

            mul.Seek(lookup, SeekOrigin.Begin);
            _components[i] = new MultiComponentList(mulReader, length, true);
        }
    }

    private void LoadUop(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        var entries = UopIndexReader.ReadIndexes(stream, ".bin", 0x10000, 4, 6);

        foreach (var (index, entry) in entries)
        {
            stream.Seek(entry.Offset, SeekOrigin.Begin);

            byte[] data;

            if (entry.Compressed)
            {
                var compressed = new byte[entry.CompressedSize];
                stream.ReadExactly(compressed, 0, entry.CompressedSize);

                data = new byte[entry.Size];

                using var source = new MemoryStream(compressed);
                using var zlib = new ZLibStream(source, CompressionMode.Decompress);
                zlib.ReadExactly(data, 0, entry.Size);
            }
            else
            {
                data = new byte[entry.Size];
                stream.ReadExactly(data, 0, entry.Size);
            }

            _components[index] = new MultiComponentList(ParseUopEntry(data));
        }
    }
}

using System.Text;
using Moongate.Core.Extensions.Strings;
using Moongate.UO.Data.Data.Tiles;
using Moongate.UO.Data.Interfaces.Files;
using Moongate.UO.Data.Interfaces.Tiles;
using Moongate.UO.Data.Types.Tiles;
using Serilog;

namespace Moongate.UO.Data.Tiles;

/// <summary>
/// Loads <c>tiledata.mul</c> into in-memory land and item tables and exposes them for lookup.
/// </summary>
public sealed class TileDataStore : ITileDataStore
{
    private const int LandLength = 0x4000;

    private static readonly ILogger _logger = Log.ForContext<TileDataStore>();

    private readonly LandData[] _landTable;
    private readonly ItemData[] _itemTable;

    public TileDataStore(IUoFileResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);

        var path = resolver.Resolve("tiledata.mul");

        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            throw new FileNotFoundException("tiledata.mul could not be resolved from the UO client files directory.");
        }

        _landTable = new LandData[LandLength];

        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var bin = new BinaryReader(fs);

        bool is64BitFlags;
        int itemLength;

        if (fs.Length >= 3188736) // 7.0.9.0
        {
            is64BitFlags = true;
            itemLength = 0x10000;
        }
        else if (fs.Length >= 1644544) // 7.0.0.0
        {
            is64BitFlags = false;
            itemLength = 0x8000;
        }
        else
        {
            is64BitFlags = false;
            itemLength = 0x4000;
        }

        _itemTable = new ItemData[itemLength];

        Span<byte> buffer = stackalloc byte[20];

        for (var i = 0; i < LandLength; i++)
        {
            if (is64BitFlags ? i == 1 || i > 0 && (i & 0x1F) == 0 : (i & 0x1F) == 0)
            {
                bin.ReadInt32(); // block header
            }

            var flags = (UoTileFlag)(is64BitFlags ? bin.ReadUInt64() : bin.ReadUInt32());
            bin.ReadInt16(); // textureId

            bin.Read(buffer);
            var name = ReadName(buffer);
            _landTable[i] = new(name, flags);
        }

        for (var i = 0; i < itemLength; i++)
        {
            if ((i & 0x1F) == 0)
            {
                bin.ReadInt32(); // block header
            }

            var flags = (UoTileFlag)(is64BitFlags ? bin.ReadUInt64() : bin.ReadUInt32());
            int weight = bin.ReadByte();
            int quality = bin.ReadByte();
            bin.ReadInt16(); // miscData (unknown)
            bin.ReadByte();  // unknown
            int quantity = bin.ReadByte();
            int animation = bin.ReadUInt16();
            bin.ReadByte(); // unknown
            bin.ReadByte(); // hue (unknown)
            bin.ReadByte(); // stacking offset (unknown)
            int value = bin.ReadByte();
            int height = bin.ReadByte();

            bin.Read(buffer);
            var name = ReadName(buffer);
            _itemTable[i] = new(name, flags, weight, quality, animation, quantity, value, height);
        }

        _logger.Information(
            "TileData loaded: {LandCount} land entries, {ItemCount} item entries",
            _landTable.Length,
            _itemTable.Length
        );
    }

    public IReadOnlyList<LandData> LandTable => _landTable;

    public IReadOnlyList<ItemData> ItemTable => _itemTable;

    public ItemData GetItem(int id)
        => id >= 0 && id < _itemTable.Length ? _itemTable[id] : default;

    public LandData GetLand(int id)
        => id >= 0 && id < _landTable.Length ? _landTable[id] : default;

    private static string ReadName(Span<byte> buffer)
    {
        var terminator = buffer.IndexOfTerminator(1);
        var slice = buffer[..(terminator < 0 ? buffer.Length : terminator)];

        return string.Intern(Encoding.ASCII.GetString(slice));
    }
}

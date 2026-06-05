using Moongate.UO.Data.Interfaces.Files;
using Moongate.UO.Data.Interfaces.Tiles;
using Moongate.UO.Data.Internal.Colors;
using Serilog;

namespace Moongate.UO.Data.Tiles;

/// <summary>
/// Loads <c>radarcol.mul</c> into an RGB555 colour table for land and static tiles. A missing file
/// yields a zeroed table (non-fatal).
/// </summary>
public sealed class RadarColorStore : IRadarColorStore
{
    private const int TotalEntries = 0x8000;

    private static readonly ILogger _logger = Log.ForContext<RadarColorStore>();

    private readonly ushort[] _colors;

    public RadarColorStore(IUoFileResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);

        _colors = new ushort[TotalEntries];

        var path = resolver.Resolve("radarcol.mul");

        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            _logger.Warning("radarcol.mul not found; radar colour table is empty.");

            return;
        }

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new BinaryReader(stream);

        var count = (int)Math.Min(TotalEntries, stream.Length / 2);

        for (var i = 0; i < count; i++)
        {
            _colors[i] = reader.ReadUInt16();
        }

        _logger.Information("Loaded {Count} radar colours from {Path}", count, path);
    }

    public int Count => _colors.Length;

    public (byte R, byte G, byte B) GetLandColor(int tileId)
        => Rgb555.ToRgb(_colors[tileId & 0x3FFF]);

    public (byte R, byte G, byte B) GetStaticColor(int tileId)
        => Rgb555.ToRgb(_colors[(tileId & 0x3FFF) + 0x4000]);
}

using System.Text;
using Moongate.UO.Data.Data.Hues;
using Moongate.UO.Data.Interfaces.Files;
using Moongate.UO.Data.Interfaces.Hues;
using Serilog;

namespace Moongate.UO.Data.Hues;

/// <summary>
///     Loads the UO hue table from <c>hues.mul</c> (groups of 8 entries). A missing file yields an
///     empty store (non-fatal).
/// </summary>
public sealed class HueStore : IHueStore
{
    private const int GroupSize = 708; // 4-byte header + 8 * 88-byte entries

    private static readonly ILogger _logger = Log.ForContext<HueStore>();

    private readonly List<Hue> _hues;

    public HueStore(IUoFileResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);

        _hues = [];

        var path = resolver.Resolve("hues.mul");

        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            _logger.Warning("hues.mul not found; hue table is empty.");

            return;
        }

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new BinaryReader(stream);

        while (stream.Length - stream.Position >= GroupSize)
        {
            reader.ReadUInt32(); // group header

            for (var entry = 0; entry < 8; entry++)
            {
                var colors = new ushort[32];

                for (var c = 0; c < 32; c++)
                {
                    colors[c] = reader.ReadUInt16();
                }

                var tableStart = reader.ReadUInt16();
                var tableEnd = reader.ReadUInt16();
                var nameBytes = reader.ReadBytes(20);
                var name = Encoding.ASCII.GetString(nameBytes).TrimEnd('\0', ' ');

                _hues.Add(new Hue(colors, tableStart, tableEnd, name));
            }
        }

        _logger.Information("Loaded {Count} hues from {Path}", _hues.Count, path);
    }

    public IReadOnlyList<Hue> Hues => _hues;

    public int Count => _hues.Count;

    public Hue? GetHue(int index)
    {
        return index >= 0 && index < _hues.Count ? _hues[index] : null;
    }
}

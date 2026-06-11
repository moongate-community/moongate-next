using System.Text.RegularExpressions;

namespace Moongate.UO.Data.Animations;

/// <summary>
/// Parses <c>Equipconv.def</c> into a <c>(bodyType, equipmentAnim) → (convertedAnim, hue)</c> table that
/// fits a worn equipment animation to a body type (e.g. male/female) and may override its hue. A missing
/// or malformed file yields an empty table (every <see cref="TryConvert" /> returns false).
/// </summary>
public sealed class EquipConvTable
{
    private static readonly Regex _numbers = new(@"-?\d+", RegexOptions.Compiled);

    private readonly Dictionary<(int Body, int Anim), (int AnimId, int Hue)> _map = new();

    public EquipConvTable(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!File.Exists(path))
        {
            return;
        }

        foreach (var rawLine in File.ReadLines(path))
        {
            var line = rawLine.Trim();

            if (line.Length == 0 || line.StartsWith('#') || line.StartsWith('"'))
            {
                continue;
            }

            var hash = line.IndexOf('#');

            if (hash >= 0)
            {
                line = line[..hash]; // drop the trailing comment
            }

            var matches = _numbers.Matches(line);

            // bodyType, equipAnim, convertToAnim, gumpId, hue
            if (matches.Count < 5)
            {
                continue;
            }

            var body = int.Parse(matches[0].Value);
            var equipAnim = int.Parse(matches[1].Value);
            var convertTo = int.Parse(matches[2].Value);
            var hue = int.Parse(matches[4].Value); // matches[3] = gumpId (ignored)

            _map[(body, equipAnim)] = (convertTo, hue);
        }
    }

    public int Count => _map.Count;

    public bool TryConvert(int bodyType, int equipmentAnim, out (int AnimId, int Hue) result)
        => _map.TryGetValue((bodyType, equipmentAnim), out result);
}

using System.Text.RegularExpressions;

namespace Moongate.UO.Data.Animations;

/// <summary>
/// Parses <c>Body.def</c> into a <c>body → (graphic, hue)</c> remap table. A missing or malformed file
/// yields an empty table; <see cref="Resolve" /> then returns the body unchanged with hue 0.
/// </summary>
public sealed class BodyDefTable
{
    private static readonly Regex _numbers = new(@"-?\d+", RegexOptions.Compiled);

    private readonly Dictionary<int, (int Graphic, int Hue)> _map = new();

    public BodyDefTable(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!File.Exists(path))
        {
            return;
        }

        foreach (var rawLine in File.ReadLines(path))
        {
            var line = rawLine.Trim();

            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            var matches = _numbers.Matches(line);

            // Need at least: currentBody, firstOldBody, hue.
            if (matches.Count < 3)
            {
                continue;
            }

            var current = int.Parse(matches[0].Value);
            var graphic = int.Parse(matches[1].Value);
            var hue = int.Parse(matches[^1].Value);

            _map[current] = (graphic, hue);
        }
    }

    public int Count => _map.Count;

    public (int Graphic, int Hue) Resolve(int body)
        => _map.TryGetValue(body, out var entry) ? entry : (body, 0);
}

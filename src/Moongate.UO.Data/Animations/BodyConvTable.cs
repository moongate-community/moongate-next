using System.Text.RegularExpressions;

namespace Moongate.UO.Data.Animations;

/// <summary>
///     Parses <c>Bodyconv.def</c> into a routing table: a body that lives in an expansion animation file
///     maps to <c>(fileType, translatedIndex)</c>, where fileType 2..5 = anim2.mul..anim5.mul. The first of
///     the anim2/anim3/anim4/anim5 columns with a value other than -1 wins. A missing or malformed file
///     yields an empty table (every <see cref="TryRoute" /> returns false, so the caller falls back to anim.mul).
/// </summary>
public sealed class BodyConvTable
{
    private static readonly Regex _numbers = new(@"-?\d+", RegexOptions.Compiled);

    private readonly Dictionary<int, (int FileType, int TranslatedIndex)> _map = new();

    public BodyConvTable(string path)
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

            var matches = _numbers.Matches(line);

            // Need body + the four anim2..anim5 columns.
            if (matches.Count < 5)
            {
                continue;
            }

            var body = int.Parse(matches[0].Value);

            if (body < 0)
            {
                continue;
            }

            for (var col = 0; col < 4; col++)
            {
                var value = int.Parse(matches[col + 1].Value);

                if (value != -1)
                {
                    _map[body] = (col + 2, value); // fileType 2..5

                    break;
                }
            }
        }
    }

    public int Count => _map.Count;

    public bool TryRoute(int body, out (int FileType, int TranslatedIndex) route)
    {
        return _map.TryGetValue(body, out route);
    }
}

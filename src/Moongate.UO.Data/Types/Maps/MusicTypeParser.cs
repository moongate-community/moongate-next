namespace Moongate.UO.Data.Types.Maps;

/// <summary>
///     Maps a region music name (e.g. "Dungeon9") to a <see cref="MusicType" />.
/// </summary>
public static class MusicTypeParser
{
    /// <summary>Parses a music name to a <see cref="MusicType" /> (case-insensitive; null/unknown → NoMusic).</summary>
    public static MusicType FromName(string? name)
    {
        return !string.IsNullOrWhiteSpace(name) && Enum.TryParse<MusicType>(name, true, out var music)
            ? music
            : MusicType.NoMusic;
    }
}

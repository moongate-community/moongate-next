using Moongate.UO.Data.Data.Localization;
using Moongate.UO.Data.Interfaces.Files;
using Moongate.UO.Data.Interfaces.Localization;
using Moongate.UO.Data.Internal.Localization;
using Serilog;

namespace Moongate.UO.Data.Localization;

/// <summary>
/// Loads the cliloc string table once from the resolved <c>cliloc.enu</c> and answers lookups and
/// placeholder formatting. A missing file yields an empty table (non-fatal).
/// </summary>
public sealed class LocalizationService : ILocalizationService
{
    private static readonly ILogger _logger = Log.ForContext<LocalizationService>();

    private readonly Dictionary<int, StringEntry> _entries;

    public LocalizationService(IUoFileResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);

        _entries = new Dictionary<int, StringEntry>();

        var path = resolver.Resolve("cliloc.enu");

        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            _logger.Warning("cliloc.enu was not found; localization table is empty.");

            return;
        }

        foreach (var entry in ClilocReader.Read(path))
        {
            _entries[entry.Number] = entry;
        }

        _logger.Information("Loaded {Count} localized strings from {Path}", _entries.Count, path);
    }

    public int Count => _entries.Count;

    public StringEntry? GetEntry(int number)
    {
        return _entries.GetValueOrDefault(number);
    }

    public string? GetText(int number)
    {
        return _entries.TryGetValue(number, out var entry) ? entry.Text : null;
    }

    public string Format(int number, params object[] args)
    {
        return _entries.TryGetValue(number, out var entry) ? entry.Format(args) : "";
    }
}

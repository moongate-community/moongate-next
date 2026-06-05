using Moongate.UO.Data.Data.Localization;

namespace Moongate.UO.Data.Interfaces.Localization;

/// <summary>
/// Provides access to the localized cliloc string table.
/// </summary>
public interface ILocalizationService
{
    /// <summary>Number of loaded entries.</summary>
    int Count { get; }

    /// <summary>
    /// Returns the formatted text for <paramref name="number" /> with <paramref name="args" />
    /// substituted into its placeholders, or <c>""</c> when the entry is absent.
    /// </summary>
    /// <param name="number">Cliloc number.</param>
    /// <param name="args">Placeholder arguments.</param>
    string Format(int number, params object[] args);

    /// <summary>Returns the entry for <paramref name="number" />, or <c>null</c> when absent.</summary>
    /// <param name="number">Cliloc number.</param>
    StringEntry? GetEntry(int number);

    /// <summary>Returns the raw text for <paramref name="number" />, or <c>null</c> when absent.</summary>
    /// <param name="number">Cliloc number.</param>
    string? GetText(int number);
}

using System.Text.RegularExpressions;
using Moongate.UO.Data.Types.Localization;

namespace Moongate.UO.Data.Data.Localization;

/// <summary>
///     A single localized cliloc string: its number, text and origin flag, with placeholder formatting.
/// </summary>
public sealed partial class StringEntry
{
    private string? _fmtTxt;
    private string _text;

    public StringEntry(int number, string text, byte flag)
    {
        Number = number;
        _text = text;
        Flag = (CliLocFlagType)flag;
    }

    public StringEntry(int number, string text, CliLocFlagType flag)
    {
        Number = number;
        _text = text;
        Flag = flag;
    }

    public int Number { get; }

    public string Text
    {
        get => _text;
        set => _text = value ?? "";
    }

    public CliLocFlagType Flag { get; set; }

    public string Format(params object[] args)
    {
        _fmtTxt ??= FormatPlaceholderRegex().Replace(_text, "{$1}");

        return string.Format(_fmtTxt, BuildArgs(args));
    }

    public string SplitFormat(string argString)
    {
        _fmtTxt ??= FormatPlaceholderRegex().Replace(_text, "{$1}");

        return string.Format(_fmtTxt, BuildArgs(argString.Split('\t')));
    }

    public override string ToString()
    {
        return $"{Number} - {Text} ({Flag})";
    }

    private static object[] BuildArgs(IReadOnlyList<object> args)
    {
        var result = new object[11];
        Array.Fill(result, "");

        for (var i = 0; i < args.Count && i < 10; i++)
        {
            result[i + 1] = args[i];
        }

        return result;
    }

    [GeneratedRegex(
        @"~(\d+)[_\w]+~",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant
    )]
    private static partial Regex FormatPlaceholderRegex();
}

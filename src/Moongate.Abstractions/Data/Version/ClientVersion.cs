namespace Moongate.Abstractions.Data.Version;

/// <summary>
/// A parsed Ultima Online client version (<c>Major.Minor.Revision.Patch</c>), comparable by
/// component. Supports the legacy patch-letter form (e.g. <c>5.0.2b</c>). Does not model the
/// KR/SA/UOTD client type or protocol changes.
/// </summary>
public sealed class ClientVersion : IComparable<ClientVersion>, IEquatable<ClientVersion>
{
    public ClientVersion(string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        SourceString = source;

        var text = source.Trim().ToLowerInvariant();
        var segments = text.Split('.');

        Major = ParseLeadingInt(Segment(segments, 0));
        Minor = ParseLeadingInt(Segment(segments, 1));

        var revisionSegment = Segment(segments, 2);
        Revision = ParseLeadingInt(revisionSegment);

        // Legacy patch letter, e.g. "5.0.2b" -> patch 2.
        var digitCount = CountLeadingDigits(revisionSegment);

        if (digitCount < revisionSegment.Length)
        {
            var letter = revisionSegment[digitCount];

            if (letter is >= 'a' and <= 'z')
            {
                Patch = letter - 'a' + 1;
            }
        }

        if (segments.Length > 3)
        {
            Patch = ParseLeadingInt(segments[3]);
        }
    }

    public ClientVersion(int major, int minor, int revision, int patch)
    {
        Major = major;
        Minor = minor;
        Revision = revision;
        Patch = patch;
        SourceString = $"{major}.{minor}.{revision}.{patch}";
    }

    public int Major { get; }

    public int Minor { get; }

    public int Revision { get; }

    public int Patch { get; }

    public string SourceString { get; }

    public int CompareTo(ClientVersion? other)
    {
        if (other is null)
        {
            return 1;
        }

        var result = Major.CompareTo(other.Major);

        if (result != 0)
        {
            return result;
        }

        result = Minor.CompareTo(other.Minor);

        if (result != 0)
        {
            return result;
        }

        result = Revision.CompareTo(other.Revision);

        return result != 0 ? result : Patch.CompareTo(other.Patch);
    }

    public bool Equals(ClientVersion? other)
    {
        return other is not null &&
               Major == other.Major &&
               Minor == other.Minor &&
               Revision == other.Revision &&
               Patch == other.Patch;
    }

    public override bool Equals(object? obj)
    {
        return obj is ClientVersion other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Major, Minor, Revision, Patch);
    }

    public static bool operator ==(ClientVersion? left, ClientVersion? right)
    {
        return left is null ? right is null : left.Equals(right);
    }

    public static bool operator !=(ClientVersion? left, ClientVersion? right)
    {
        return !(left == right);
    }

    public static bool operator >(ClientVersion? left, ClientVersion? right)
    {
        return Compare(left, right) > 0;
    }

    public static bool operator <(ClientVersion? left, ClientVersion? right)
    {
        return Compare(left, right) < 0;
    }

    public static bool operator >=(ClientVersion? left, ClientVersion? right)
    {
        return Compare(left, right) >= 0;
    }

    public static bool operator <=(ClientVersion? left, ClientVersion? right)
    {
        return Compare(left, right) <= 0;
    }

    public override string ToString()
    {
        return SourceString;
    }

    private static int Compare(ClientVersion? left, ClientVersion? right)
    {
        if (left is null)
        {
            return right is null ? 0 : -1;
        }

        return left.CompareTo(right);
    }

    private static string Segment(string[] segments, int index)
    {
        return index < segments.Length ? segments[index] : "";
    }

    private static int CountLeadingDigits(string value)
    {
        var count = 0;

        while (count < value.Length && char.IsDigit(value[count]))
        {
            count++;
        }

        return count;
    }

    private static int ParseLeadingInt(string value)
    {
        var digits = value.AsSpan(0, CountLeadingDigits(value));

        return digits.Length > 0 && int.TryParse(digits, out var result) ? result : 0;
    }
}

using Moongate.UO.Data.Types.Expansions;
using Moongate.Abstractions.Types.Expansions;
using Moongate.Abstractions.Data.Version;

namespace Moongate.UO.Data.Data.Expansions;

/// <summary>
/// Capability metadata for a UO expansion era: advertised client/feature/housing flags, selectable
/// facets, the minimum client version, and the mobile-status packet version.
/// </summary>
public sealed class ExpansionInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public ClientFlags ClientFlags { get; set; }
    public FeatureFlags SupportedFeatures { get; set; }
    public CharacterListFlags CharacterListFlags { get; set; }
    public HousingFlags HousingFlags { get; set; }
    public UoMapSelectionFlags MapSelectionFlags { get; set; }
    public int MobileStatusVersion { get; set; }

    /// <summary>Minimum client version string (e.g. "7.0.9.0"); empty when unspecified.</summary>
    public string RequiredClientVersion { get; set; } = "";

    /// <summary>Parsed <see cref="RequiredClientVersion" />, or <c>null</c> when empty.</summary>
    public ClientVersion? RequiredClient
        => string.IsNullOrWhiteSpace(RequiredClientVersion) ? null : new ClientVersion(RequiredClientVersion);
}

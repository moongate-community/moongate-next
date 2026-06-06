using Moongate.Abstractions.Interfaces.Config;
using Moongate.Core.Extensions.Directories;
using Moongate.Core.Geometry;
using Moongate.UO.Data.Types.Maps;

namespace Moongate.UO.Data.Data;

/// <summary>
/// Configuration for the Ultima Online client data files (idx/mul/uop). Bound from the <c>uo</c>
/// YAML section. Validation fails boot when the directory or the minimum required files are absent.
/// </summary>
public sealed class UoConfig : IValidatableConfig
{
    /// <summary>
    /// Directory containing the UO client data files. Supports <c>~</c> and environment variables.
    /// Default: <c>~/uo</c>.
    /// </summary>
    public string ClientFilesDirectory { get; set; } = "~/uo";

    /// <summary>Facet new characters start on. Default Trammel.</summary>
    public UoMapFacetType StartingMap { get; set; } = UoMapFacetType.Trammel;

    /// <summary>Starting world coordinates. Default 1496,1628,10 (Britain).</summary>
    public Point3D Starting { get; set; } = new(1496, 1628, 10);

    /// <summary>Display name of the starting city. Default "Britain".</summary>
    public string StartingCity { get; set; } = "Britain";

    /// <inheritdoc />
    public IEnumerable<string> Validate()
    {
        if (!Enum.IsDefined(StartingMap))
        {
            yield return $"starting map '{StartingMap}' must be a known UO map facet";
        }

        var directory = ClientFilesDirectory?.ResolvePathAndEnvs();

        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            yield return $"client files directory '{ClientFilesDirectory}' does not exist";

            yield break;
        }

        if (!File.Exists(Path.Combine(directory, "tiledata.mul")))
        {
            yield return $"required file 'tiledata.mul' was not found in '{directory}'";
        }
    }
}

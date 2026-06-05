using Moongate.Abstractions.Interfaces.Config;
using Moongate.Core.Extensions.Directories;

namespace Moongate.UO.Data.Data;

/// <summary>
/// Configuration for the Ultima Online client data files (idx/mul/uop). Bound from the <c>uo</c>
/// TOML section. Validation fails boot when the directory or the minimum required files are absent.
/// </summary>
public sealed class UoConfig : IValidatableConfig
{
    /// <summary>
    /// Directory containing the UO client data files. Supports <c>~</c> and environment variables.
    /// Default: <c>~/uo</c>.
    /// </summary>
    public string ClientFilesDirectory { get; set; } = "~/uo";

    /// <summary>Facet id new characters start on (0 = Felucca, 1 = Trammel, ...). Default 1.</summary>
    public int StartingMapId { get; set; } = 1;

    /// <summary>Starting world X coordinate. Default 1496 (Britain).</summary>
    public int StartingX { get; set; } = 1496;

    /// <summary>Starting world Y coordinate. Default 1628 (Britain).</summary>
    public int StartingY { get; set; } = 1628;

    /// <summary>Starting world Z coordinate. Default 10 (Britain).</summary>
    public int StartingZ { get; set; } = 10;

    /// <summary>Display name of the starting city. Default "Britain".</summary>
    public string StartingCity { get; set; } = "Britain";

    /// <inheritdoc />
    public IEnumerable<string> Validate()
    {
        if (StartingMapId is < 0 or > 5)
        {
            yield return $"starting map id {StartingMapId} must be between 0 and 5";
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

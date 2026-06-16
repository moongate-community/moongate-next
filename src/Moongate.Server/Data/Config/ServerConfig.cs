namespace Moongate.Server.Data.Config;

/// <summary>
/// Core server identity settings (the <c>server</c> config section).
/// </summary>
public sealed class ServerConfig
{
    /// <summary>Display name of the shard/server (e.g. shown in the UO server list). Default "Moongate Server".</summary>
    public string ServerName { get; set; } = "Moongate Server";

    /// <summary>Enables public account registration endpoints. Default false.</summary>
    public bool IsRegistrationAllowed { get; set; }

    /// <summary>Real seconds per in-game UO minute for the light/time clock. Default 5.0 (~12x).</summary>
    public double LightSecondsPerUoMinute { get; set; } = 5.0;

    /// <summary>UTC anchor for the accelerated world clock (ISO-8601). Default 1997-09-01.</summary>
    public string LightWorldStartUtc { get; set; } = "1997-09-01T00:00:00Z";
}

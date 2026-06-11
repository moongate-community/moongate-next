namespace Moongate.Server.Data.Config;

/// <summary>
/// Core server identity settings (the <c>server</c> config section).
/// </summary>
public sealed class ServerConfig
{
    /// <summary>Display name of the shard/server (e.g. shown in the UO server list). Default "Moongate Server".</summary>
    public string ServerName { get; set; } = "Moongate Server";

    /// <summary>Enables public account registration endpoints when implemented. Default false.</summary>
    public bool IsRegistrationAllowed { get; set; }
}

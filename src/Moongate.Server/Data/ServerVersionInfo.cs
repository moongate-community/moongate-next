namespace Moongate.Server.Data;

/// <summary>
///     Version information returned by the <c>GET /api/version</c> endpoint.
/// </summary>
public sealed record ServerVersionInfo
{
    public ServerVersionInfo(string version, string codename)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        ArgumentException.ThrowIfNullOrWhiteSpace(codename);

        Version = version;
        Codename = codename;
    }

    public string Version { get; }
    public string Codename { get; }
}

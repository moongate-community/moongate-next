namespace Moongate.Server.Data.Mobiles;

public sealed record BodyImageBuildResult
{
    public int TotalBodies { get; init; }

    public int Generated { get; init; }

    public int Cached { get; init; }

    public int Skipped { get; init; }

    public int Failed { get; init; }
}

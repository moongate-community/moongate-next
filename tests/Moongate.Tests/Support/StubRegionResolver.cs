using Moongate.Core.Geometry;
using Moongate.Server.Data.World;
using Moongate.Server.Interfaces.Services.World;
using Moongate.UO.Data.Types.Maps;

namespace Moongate.Tests.Support;

/// <summary>
///     IRegionResolverService stub returning a fixed region for every lookup; other members throw.
/// </summary>
public sealed class StubRegionResolver : IRegionResolverService
{
    private readonly RegionEntry? _region;

    public StubRegionResolver(RegionEntry? region)
    {
        _region = region;
    }

    public MusicType GetMusic(int mapId, Point3D location)
    {
        return MusicTypeParser.FromName(_region?.Music);
    }

    public RegionEntry? GetRegionByName(string name)
    {
        throw new NotSupportedException();
    }

    public RegionEntry? ResolveRegion(int mapId, Point3D location)
    {
        return _region;
    }
}

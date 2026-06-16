using Moongate.UO.Data.Types.Maps;

namespace Moongate.UO.Data.Data.Maps;

/// <summary>
/// Static metadata describing a UO map facet: its identity, the client file index it reads from,
/// its dimensions in tiles, a display name, its gameplay rules, and its default season.
/// </summary>
public sealed record MapDefinition(
    int Index,
    int MapId,
    int FileIndex,
    int Width,
    int Height,
    string Name,
    MapRulesType Rules,
    SeasonType Season
);

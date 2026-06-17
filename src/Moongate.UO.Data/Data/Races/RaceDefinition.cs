namespace Moongate.UO.Data.Data.Races;

/// <summary>
///     Data-only definition of a UO playable race: identity and the bodies used for each gender,
///     alive and ghost. Behaviour (hair/hue rules) is a content-module concern.
/// </summary>
public sealed class RaceDefinition
{
    public int Id { get; set; }
    public int Index { get; set; }
    public string Name { get; set; } = "";
    public string PluralName { get; set; } = "";
    public int MaleBody { get; set; }
    public int FemaleBody { get; set; }
    public int MaleGhostBody { get; set; }
    public int FemaleGhostBody { get; set; }

    public int RaceFlag => 1 << Index;
}

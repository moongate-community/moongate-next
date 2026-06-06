namespace Moongate.UO.Data.Data.Internal;

/// <summary>Body ids grouped by category, as bound from <c>bodies.yaml</c>.</summary>
public sealed class BodyGroups
{
    public List<int> Monster { get; set; } = [];
    public List<int> Sea { get; set; } = [];
    public List<int> Animal { get; set; } = [];
    public List<int> Human { get; set; } = [];
    public List<int> Equipment { get; set; } = [];
}

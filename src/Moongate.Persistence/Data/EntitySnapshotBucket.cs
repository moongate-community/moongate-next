namespace Moongate.Persistence.Data;

/// <summary>
///     Serialized snapshot bucket for a single registered entity type.
/// </summary>
public sealed class EntitySnapshotBucket
{
    public ushort TypeId { get; set; }
    public string TypeName { get; set; } = "";
    public int SchemaVersion { get; set; }
    public byte[] Payload { get; set; } = [];
}

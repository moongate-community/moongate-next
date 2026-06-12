namespace Moongate.Server.Data.Mobiles;

/// <summary>A single selectable hair (or facial-hair) style: graphic id + hex + display name.</summary>
public sealed record HairStyleEntry(int Style, string StyleHex, string Name, bool IsFacial);

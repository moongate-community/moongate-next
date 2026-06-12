namespace Moongate.Server.Data.Mobiles;

/// <summary>A selectable hair style for the picker: id + hex + name + facial flag + preview image url.</summary>
public sealed record HairStyleSummary(int Style, string StyleHex, string Name, bool IsFacial, string ImageUrl);

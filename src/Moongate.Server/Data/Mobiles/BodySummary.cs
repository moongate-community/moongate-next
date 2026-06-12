namespace Moongate.Server.Data.Mobiles;

/// <summary>A classified UO body for the body picker (id + hex + category + preview image url).</summary>
public sealed record BodySummary(int Body, string BodyHex, string BodyType, string ImageUrl);

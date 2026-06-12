namespace Moongate.Server.Data.Templates;

public sealed record LootTemplateNodeSummary(
    string Id,
    string ParentId,
    int Depth,
    string Kind,
    string Label,
    string? Rarity,
    double Chance,
    int Weight,
    int AmountMin,
    int AmountMax,
    string? ItemTemplateId,
    string? ItemIdHex,
    string? ImageUrl,
    bool Stackable
);

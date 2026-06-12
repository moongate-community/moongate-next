namespace Moongate.Server.Data.Templates;

public sealed record UoItemSummary(
    int ItemId,
    string ItemIdHex,
    string Name,
    string ImageUrl,
    IReadOnlyList<string> Flags,
    int Weight,
    int Quality,
    int Animation,
    int Quantity,
    int Value,
    int Height,
    bool Container,
    bool Weapon,
    bool Armor,
    bool Wearable,
    bool Door,
    bool Surface,
    bool Background,
    bool Wall
);

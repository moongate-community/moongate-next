namespace Moongate.Server.Data.Templates;

public sealed record UoItemDetail(
    int ItemId,
    string ItemIdHex,
    string Name,
    string ImageUrl,
    IReadOnlyList<string> Flags,
    ulong RawFlags,
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

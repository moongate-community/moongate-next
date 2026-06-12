/** Wowhead-style rarity → color map, shared by item/loot tooltips. */
export const rarityColors: Record<string, string> = {
  None: "#9ca3af",
  Common: "#ffffff",
  Uncommon: "#1eff00",
  Rare: "#0070dd",
  Epic: "#a335ee",
  Legendary: "#ff8000"
};

/** Resolves a rarity label to its color, falling back to Common. */
export function rarityColor(rarity: string | null): string {
  return rarity ? rarityColors[rarity] ?? rarityColors.Common : rarityColors.Common;
}

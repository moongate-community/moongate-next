import { Gem } from "lucide-react";
import { Badge } from "@/components/ui/badge";

type RarityBadgeProps = {
  rarity: string;
  mode?: "compact" | "detail";
};

type RarityStyle = {
  label: string;
  text: string;
  border: string;
  background: string;
  gem: string;
};

const rarityStyles: Record<string, RarityStyle> = {
  Common: {
    label: "Common",
    text: "#d8dde6",
    border: "rgba(154, 166, 188, 0.26)",
    background: "rgba(154, 166, 188, 0.10)",
    gem: "#aeb7c4"
  },
  Uncommon: {
    label: "Uncommon",
    text: "#4ade80",
    border: "rgba(74, 222, 128, 0.28)",
    background: "rgba(74, 222, 128, 0.10)",
    gem: "#22c55e"
  },
  Rare: {
    label: "Rare",
    text: "#f8d35c",
    border: "rgba(248, 211, 92, 0.34)",
    background: "rgba(248, 211, 92, 0.10)",
    gem: "#f59e0b"
  },
  Epic: {
    label: "Epic",
    text: "#c084fc",
    border: "rgba(192, 132, 252, 0.30)",
    background: "rgba(192, 132, 252, 0.10)",
    gem: "#a855f7"
  },
  Legendary: {
    label: "Legendary",
    text: "#fb923c",
    border: "rgba(251, 146, 60, 0.34)",
    background: "rgba(251, 146, 60, 0.10)",
    gem: "#ea580c"
  }
};

const fallbackStyle: RarityStyle = {
  label: "None",
  text: "#9aa6bc",
  border: "rgba(154, 166, 188, 0.28)",
  background: "rgba(154, 166, 188, 0.10)",
  gem: "#9aa6bc"
};

export function RarityBadge({ rarity, mode = "compact" }: RarityBadgeProps) {
  const style = rarityStyles[rarity] ?? {
    ...fallbackStyle,
    label: rarity || fallbackStyle.label
  };
  const isDetail = mode === "detail";

  return (
    <Badge
      variant="outline"
      className={`relative inline-flex max-w-full items-center gap-1.5 overflow-hidden rounded-md border font-bold leading-none ${
        isDetail ? "px-2 py-1 text-xs" : "px-1.5 py-0.5 text-[11px]"
      }`}
      style={{
        background: style.background,
        borderColor: style.border,
        color: style.text
      }}
      aria-label={`Rarity ${style.label}`}
      title={`Rarity: ${style.label}`}
    >
      <Gem size={isDetail ? 14 : 12} aria-hidden className="shrink-0" style={{ color: style.gem }} />
      <span className="truncate">{style.label}</span>
    </Badge>
  );
}

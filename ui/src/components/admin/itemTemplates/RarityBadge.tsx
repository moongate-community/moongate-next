import { Gem } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";

type RarityBadgeProps = {
  rarity: string;
  mode?: "compact" | "detail";
};

type RarityStyle = {
  label: string;
  textClass: string;
  border: string;
  background: string;
  gem: string;
};

const rarityStyles: Record<string, RarityStyle> = {
  Common: {
    label: "Common",
    textClass: "text-slate-600 dark:text-slate-300",
    border: "rgba(154, 166, 188, 0.26)",
    background: "rgba(154, 166, 188, 0.10)",
    gem: "#64748b"
  },
  Uncommon: {
    label: "Uncommon",
    textClass: "text-green-700 dark:text-green-300",
    border: "rgba(74, 222, 128, 0.28)",
    background: "rgba(74, 222, 128, 0.10)",
    gem: "#22c55e"
  },
  Rare: {
    label: "Rare",
    textClass: "text-yellow-700 dark:text-yellow-300",
    border: "rgba(248, 211, 92, 0.34)",
    background: "rgba(248, 211, 92, 0.10)",
    gem: "#f59e0b"
  },
  Epic: {
    label: "Epic",
    textClass: "text-purple-700 dark:text-purple-300",
    border: "rgba(192, 132, 252, 0.30)",
    background: "rgba(192, 132, 252, 0.10)",
    gem: "#a855f7"
  },
  Legendary: {
    label: "Legendary",
    textClass: "text-orange-700 dark:text-orange-300",
    border: "rgba(251, 146, 60, 0.34)",
    background: "rgba(251, 146, 60, 0.10)",
    gem: "#ea580c"
  }
};

const fallbackStyle: RarityStyle = {
  label: "None",
  textClass: "text-slate-600 dark:text-slate-300",
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
      className={cn(
        "relative inline-flex max-w-full items-center gap-1.5 overflow-hidden rounded-md border font-bold leading-none",
        isDetail ? "px-2 py-1 text-xs" : "px-1.5 py-0.5 text-[11px]",
        style.textClass
      )}
      style={{
        background: style.background,
        borderColor: style.border
      }}
      aria-label={`Rarity ${style.label}`}
      title={`Rarity: ${style.label}`}
    >
      <Gem size={isDetail ? 14 : 12} aria-hidden className="shrink-0" style={{ color: style.gem }} />
      <span className="truncate">{style.label}</span>
    </Badge>
  );
}

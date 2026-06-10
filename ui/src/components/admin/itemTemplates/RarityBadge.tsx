import { Gem } from "lucide-react";

type RarityBadgeProps = {
  rarity: string;
  mode?: "compact" | "detail";
};

type RarityStyle = {
  label: string;
  text: string;
  border: string;
  background: string;
  shine: string;
  gem: string;
  shadow: string;
};

const rarityStyles: Record<string, RarityStyle> = {
  Common: {
    label: "Common",
    text: "#d8dde6",
    border: "rgba(216, 221, 230, 0.32)",
    background: "linear-gradient(180deg, rgba(216, 221, 230, 0.16), rgba(96, 105, 116, 0.10))",
    shine: "linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.62), transparent)",
    gem: "#aeb7c4",
    shadow: "0 0 10px rgba(216, 221, 230, 0.18)"
  },
  Uncommon: {
    label: "Uncommon",
    text: "#4ade80",
    border: "rgba(74, 222, 128, 0.38)",
    background: "linear-gradient(180deg, rgba(74, 222, 128, 0.16), rgba(22, 101, 52, 0.12))",
    shine: "linear-gradient(90deg, transparent, rgba(134, 239, 172, 0.78), transparent)",
    gem: "#22c55e",
    shadow: "0 0 14px rgba(34, 197, 94, 0.28)"
  },
  Rare: {
    label: "Rare",
    text: "#f8d35c",
    border: "rgba(248, 211, 92, 0.48)",
    background: "linear-gradient(180deg, rgba(248, 211, 92, 0.18), rgba(146, 88, 18, 0.14))",
    shine: "linear-gradient(90deg, transparent, rgba(254, 240, 138, 0.88), transparent)",
    gem: "#f59e0b",
    shadow: "0 0 16px rgba(245, 158, 11, 0.34)"
  },
  Epic: {
    label: "Epic",
    text: "#c084fc",
    border: "rgba(192, 132, 252, 0.42)",
    background: "linear-gradient(180deg, rgba(192, 132, 252, 0.18), rgba(107, 33, 168, 0.14))",
    shine: "linear-gradient(90deg, transparent, rgba(216, 180, 254, 0.78), transparent)",
    gem: "#a855f7",
    shadow: "0 0 16px rgba(168, 85, 247, 0.30)"
  },
  Legendary: {
    label: "Legendary",
    text: "#fb923c",
    border: "rgba(251, 146, 60, 0.46)",
    background: "linear-gradient(180deg, rgba(251, 146, 60, 0.18), rgba(154, 52, 18, 0.16))",
    shine: "linear-gradient(90deg, transparent, rgba(253, 186, 116, 0.82), transparent)",
    gem: "#ea580c",
    shadow: "0 0 18px rgba(234, 88, 12, 0.34)"
  }
};

const fallbackStyle: RarityStyle = {
  label: "None",
  text: "#9aa6bc",
  border: "rgba(154, 166, 188, 0.28)",
  background: "linear-gradient(180deg, rgba(154, 166, 188, 0.12), rgba(75, 85, 99, 0.08))",
  shine: "linear-gradient(90deg, transparent, rgba(154, 166, 188, 0.42), transparent)",
  gem: "#9aa6bc",
  shadow: "none"
};

export function RarityBadge({ rarity, mode = "compact" }: RarityBadgeProps) {
  const style = rarityStyles[rarity] ?? {
    ...fallbackStyle,
    label: rarity || fallbackStyle.label
  };
  const isDetail = mode === "detail";

  return (
    <span
      className={`relative inline-flex max-w-full items-center gap-1.5 overflow-hidden rounded-md border font-bold leading-none ${
        isDetail ? "px-2.5 py-1.5 text-xs" : "px-2 py-1 text-[11px]"
      }`}
      style={{
        background: style.background,
        borderColor: style.border,
        boxShadow: style.shadow,
        color: style.text,
        textShadow: `0 0 ${isDetail ? "10px" : "8px"} ${style.text}55`
      }}
      aria-label={`Rarity ${style.label}`}
      title={`Rarity: ${style.label}`}
    >
      <span className="absolute inset-x-1 top-0 h-px opacity-80" style={{ background: style.shine }} aria-hidden />
      <Gem size={isDetail ? 14 : 12} aria-hidden className="shrink-0" style={{ color: style.gem }} />
      <span className="truncate">{style.label}</span>
    </span>
  );
}

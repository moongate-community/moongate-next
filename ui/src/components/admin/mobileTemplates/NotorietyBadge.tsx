import { Shield } from "lucide-react";

type NotorietyBadgeProps = {
  notoriety: string;
  mode?: "compact" | "detail";
};

type NotorietyStyle = {
  label: string;
  text: string;
  border: string;
  background: string;
  icon: string;
};

// Colours mirror the canonical UO name-hues documented on NotorietyType:
// Invalid none, Innocent blue, ally green, attackable/criminal gray, enemy orange, murderer red, invulnerable translucent.
const notorietyStyles: Record<string, NotorietyStyle> = {
  Invalid: { label: "Invalid", text: "#9aa6bc", border: "rgba(154, 166, 188, 0.26)", background: "rgba(154, 166, 188, 0.10)", icon: "#9aa6bc" },
  Innocent: { label: "Innocent", text: "#60a5fa", border: "rgba(96, 165, 250, 0.30)", background: "rgba(96, 165, 250, 0.10)", icon: "#3b82f6" },
  Friend: { label: "Friend", text: "#4ade80", border: "rgba(74, 222, 128, 0.28)", background: "rgba(74, 222, 128, 0.10)", icon: "#22c55e" },
  CanBeAttacked: { label: "Attackable", text: "#cbd5f5", border: "rgba(154, 166, 188, 0.26)", background: "rgba(154, 166, 188, 0.10)", icon: "#94a3b8" },
  Criminal: { label: "Criminal", text: "#9aa6bc", border: "rgba(148, 163, 184, 0.40)", background: "rgba(148, 163, 184, 0.14)", icon: "#94a3b8" },
  Enemy: { label: "Enemy", text: "#fb923c", border: "rgba(251, 146, 60, 0.34)", background: "rgba(251, 146, 60, 0.10)", icon: "#ea580c" },
  Murdered: { label: "Murderer", text: "#f87171", border: "rgba(248, 113, 113, 0.34)", background: "rgba(248, 113, 113, 0.10)", icon: "#ef4444" },
  // Invulnerable = "unknown use", rendered translucent (like the 0x4000 hue): faint low-opacity neutral.
  Invulnerable: { label: "Invulnerable", text: "rgba(226, 232, 240, 0.55)", border: "rgba(226, 232, 240, 0.18)", background: "rgba(226, 232, 240, 0.06)", icon: "rgba(226, 232, 240, 0.45)" }
};

const fallbackStyle: NotorietyStyle = {
  label: "Unknown",
  text: "#9aa6bc",
  border: "rgba(154, 166, 188, 0.28)",
  background: "rgba(154, 166, 188, 0.10)",
  icon: "#9aa6bc"
};

export function NotorietyBadge({ notoriety, mode = "compact" }: NotorietyBadgeProps) {
  const style = notorietyStyles[notoriety] ?? { ...fallbackStyle, label: notoriety || fallbackStyle.label };
  const isDetail = mode === "detail";

  return (
    <span
      className={`relative inline-flex max-w-full items-center gap-1.5 overflow-hidden rounded-md border font-bold leading-none ${
        isDetail ? "px-2 py-1 text-xs" : "px-1.5 py-0.5 text-[11px]"
      }`}
      style={{ background: style.background, borderColor: style.border, color: style.text }}
      aria-label={`Notoriety ${style.label}`}
      title={`Notoriety: ${style.label}`}
    >
      <Shield size={isDetail ? 14 : 12} aria-hidden className="shrink-0" style={{ color: style.icon }} />
      <span className="truncate">{style.label}</span>
    </span>
  );
}

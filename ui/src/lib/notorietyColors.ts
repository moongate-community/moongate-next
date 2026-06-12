/** Visual style for a notoriety value, mirroring the canonical UO name-hues. */
export type NotorietyStyle = {
  label: string;
  text: string;
  border: string;
  background: string;
  icon: string;
};

// Invalid none, Innocent blue, ally green, attackable/criminal gray, enemy orange, murderer red, invulnerable translucent.
export const notorietyStyles: Record<string, NotorietyStyle> = {
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

export const fallbackNotorietyStyle: NotorietyStyle = {
  label: "Unknown",
  text: "#9aa6bc",
  border: "rgba(154, 166, 188, 0.28)",
  background: "rgba(154, 166, 188, 0.10)",
  icon: "#9aa6bc"
};

/** Resolves a notoriety value to its style, falling back to a neutral style labelled with the raw value. */
export function notorietyStyle(notoriety: string): NotorietyStyle {
  return notorietyStyles[notoriety] ?? { ...fallbackNotorietyStyle, label: notoriety || fallbackNotorietyStyle.label };
}

/** Resolves a notoriety value to its text color. */
export function notorietyColor(notoriety: string): string {
  return notorietyStyle(notoriety).text;
}

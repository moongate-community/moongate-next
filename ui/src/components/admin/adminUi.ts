import type { AdminStatus, TrendPolarity } from "../../types/admin";

/** Background color token for status accent bars and dots. */
export const statusAccentClass: Record<AdminStatus, string> = {
  healthy: "bg-success",
  warning: "bg-warning",
  offline: "bg-danger"
};

/** Foreground color token for status text. */
export const statusTextClass: Record<AdminStatus, string> = {
  healthy: "text-success",
  warning: "text-warning",
  offline: "text-danger"
};

/** Soft pill (text + tinted background) for status labels. */
export const statusPillClass: Record<AdminStatus, string> = {
  healthy: "text-success bg-success/10 border-success/20",
  warning: "text-warning bg-warning/10 border-warning/20",
  offline: "text-danger bg-danger/10 border-danger/20"
};

/** Trend chip color by polarity (good/bad/neutral movement). */
export const trendChipClass: Record<TrendPolarity, string> = {
  positive: "text-success bg-success/10",
  negative: "text-danger bg-danger/10",
  neutral: "text-fg-muted bg-muted"
};

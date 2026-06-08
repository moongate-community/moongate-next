import type { Theme } from "./useTheme";

export type ChartTheme = {
  axisLine: string;
  axisLabel: string;
  splitLine: string;
  tooltipBg: string;
  tooltipBorder: string;
  tooltipText: string;
};

function cssVar(name: string, fallback: string): string {
  if (typeof window === "undefined") {
    return fallback;
  }

  const value = getComputedStyle(document.documentElement).getPropertyValue(name).trim();

  return value || fallback;
}

/**
 * Reads the live theme tokens from the document so ECharts canvases match the
 * active palette. Pass the current theme as a dependency key from callers so
 * the option is rebuilt when the user toggles.
 */
export function readChartTheme(_theme: Theme): ChartTheme {
  return {
    axisLine: cssVar("--border", "#e3e7ed"),
    axisLabel: cssVar("--fg-subtle", "#8a958c"),
    splitLine: cssVar("--border", "#e3e7ed"),
    tooltipBg: cssVar("--surface-raised", "#ffffff"),
    tooltipBorder: cssVar("--border", "#cdd5da"),
    tooltipText: cssVar("--fg", "#16201b")
  };
}

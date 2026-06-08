export type AdminNavId = "overview" | "runtime" | "metrics" | "persistence" | "security";

export type AdminStatus = "healthy" | "warning" | "offline";

export type ServerVersionInfo = {
  version: string;
  codename: string;
};

export type OpenMetricSample = {
  name: string;
  value: number;
  labels: Record<string, string>;
};

export type RuntimeServiceStatus = {
  id: string;
  label: string;
  status: AdminStatus;
  primary: string;
  secondary: string;
};

export type TrendPolarity = "positive" | "negative" | "neutral";

export type AdminMetricTrend = {
  direction: "up" | "down" | "flat";
  label: string;
  polarity: TrendPolarity;
};

export type AdminMetricCard = {
  label: string;
  value: string;
  detail: string;
  status: AdminStatus;
  series?: number[];
  trend?: AdminMetricTrend;
};

export type AdminRuntimeSnapshot = {
  server: ServerVersionInfo | null;
  collectedAt: string | null;
  reachable: boolean;
  metrics: Record<string, number>;
};

export type AdminMetricHistoryPoint = {
  at: string;
  metrics: Record<string, number>;
};

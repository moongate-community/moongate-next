export type AdminNavId = "overview" | "runtime" | "persistence" | "security" | "diagnostics";

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

export type AdminMetricCard = {
  label: string;
  value: string;
  detail: string;
  status: AdminStatus;
};

export type AdminActivityEvent = {
  id: string;
  label: string;
  detail: string;
  at: string;
  status: AdminStatus;
};

export type AdminRuntimeSnapshot = {
  server: ServerVersionInfo | null;
  collectedAt: string | null;
  reachable: boolean;
  metrics: Record<string, number>;
};

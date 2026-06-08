import type { AdminRuntimeSnapshot, ServerVersionInfo } from "../types/admin";
import { readJson } from "./authClient";
import { parseOpenMetrics, toMetricMap } from "./openMetrics";

export async function getServerVersion(): Promise<ServerVersionInfo> {
  const response = await fetch("/api/version");

  return readJson<ServerVersionInfo>(response);
}

export async function getMetricsText(): Promise<string> {
  const response = await fetch("/metrics");

  if (!response.ok) {
    throw new Error(`${response.status} ${response.statusText}`);
  }

  return response.text();
}

export async function getAdminRuntimeSnapshot(): Promise<AdminRuntimeSnapshot> {
  const [server, metricsText] = await Promise.all([getServerVersion(), getMetricsText()]);
  const metrics = toMetricMap(parseOpenMetrics(metricsText));

  return {
    server,
    collectedAt: new Date().toISOString(),
    reachable: true,
    metrics
  };
}

export function getOfflineSnapshot(): AdminRuntimeSnapshot {
  return {
    server: null,
    collectedAt: null,
    reachable: false,
    metrics: {}
  };
}

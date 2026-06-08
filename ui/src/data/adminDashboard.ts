import type { AdminMetricCard, AdminRuntimeSnapshot, RuntimeServiceStatus } from "../types/admin";

function metric(snapshot: AdminRuntimeSnapshot, name: string): number {
  return snapshot.metrics[name] ?? 0;
}

function formatNumber(value: number): string {
  return new Intl.NumberFormat("en-US", { maximumFractionDigits: 0 }).format(value);
}

function formatMs(value: number): string {
  return `${value.toFixed(value >= 10 ? 0 : 2)} ms`;
}

export function buildMetricCards(snapshot: AdminRuntimeSnapshot): AdminMetricCard[] {
  return [
    {
      label: "Server",
      value: snapshot.reachable ? "Online" : "Offline",
      detail: snapshot.server ? `${snapshot.server.version} ${snapshot.server.codename}` : "No response from API",
      status: snapshot.reachable ? "healthy" : "offline"
    },
    {
      label: "Sessions",
      value: formatNumber(metric(snapshot, "network_active_sessions")),
      detail: "Active game network sessions",
      status: "healthy"
    },
    {
      label: "Tick avg",
      value: formatMs(metric(snapshot, "gameloop_tick_avg_ms")),
      detail: "Game loop moving average",
      status: metric(snapshot, "gameloop_tick_avg_ms") > 50 ? "warning" : "healthy"
    },
    {
      label: "Journal",
      value: formatNumber(metric(snapshot, "persistence_last_sequence_id")),
      detail: "Last persistence sequence",
      status: "healthy"
    }
  ];
}

export function buildRuntimeServices(snapshot: AdminRuntimeSnapshot): RuntimeServiceStatus[] {
  const parserErrors = metric(snapshot, "network_parser_errors_total");
  const handlerErrors = metric(snapshot, "bus_handler_errors_total");
  const timerErrors = metric(snapshot, "timer_callback_errors_total");

  return [
    {
      id: "gameloop",
      label: "Game loop",
      status: metric(snapshot, "gameloop_tick_avg_ms") > 50 ? "warning" : "healthy",
      primary: formatMs(metric(snapshot, "gameloop_tick_avg_ms")),
      secondary: `Max ${formatMs(metric(snapshot, "gameloop_tick_max_ms"))}`
    },
    {
      id: "network",
      label: "Network",
      status: parserErrors > 0 ? "warning" : "healthy",
      primary: `${formatNumber(metric(snapshot, "network_active_sessions"))} sessions`,
      secondary: `${formatNumber(metric(snapshot, "network_ingress_queue_depth"))} ingress queue`
    },
    {
      id: "event-bus",
      label: "Event bus",
      status: handlerErrors > 0 ? "warning" : "healthy",
      primary: `${formatNumber(metric(snapshot, "bus_tick_queue_depth"))} queued`,
      secondary: `${formatNumber(handlerErrors)} handler errors`
    },
    {
      id: "timer",
      label: "Timer wheel",
      status: timerErrors > 0 ? "warning" : "healthy",
      primary: `${formatNumber(metric(snapshot, "timer_active"))} active`,
      secondary: `${formatMs(metric(snapshot, "timer_callback_avg_ms"))} callback avg`
    }
  ];
}

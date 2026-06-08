import type {
  AdminMetricCard,
  AdminMetricHistoryPoint,
  AdminMetricTrend,
  AdminRuntimeSnapshot,
  RuntimeServiceStatus
} from "../types/admin";

function metric(snapshot: AdminRuntimeSnapshot, name: string): number {
  return snapshot.metrics[name] ?? 0;
}

function formatNumber(value: number): string {
  return new Intl.NumberFormat("en-US", { maximumFractionDigits: 0 }).format(value);
}

function formatMs(value: number): string {
  return `${value.toFixed(value >= 10 ? 0 : 2)} ms`;
}

function seriesFor(history: AdminMetricHistoryPoint[], name: string): number[] {
  return history.map((point) => point.metrics[name] ?? 0);
}

function trendFor(series: number[], higherIsBetter: boolean, unit: "ms" | "count"): AdminMetricTrend | undefined {
  if (series.length < 2) {
    return undefined;
  }

  const latest = series[series.length - 1];
  const previous = series[series.length - 2];
  const delta = latest - previous;

  if (delta === 0) {
    return { direction: "flat", label: "No change", polarity: "neutral" };
  }

  const direction = delta > 0 ? "up" : "down";
  const isGood = delta > 0 === higherIsBetter;
  const magnitude = Math.abs(delta);
  const formatted = unit === "ms" ? `${magnitude.toFixed(magnitude >= 10 ? 0 : 2)} ms` : formatNumber(magnitude);

  return {
    direction,
    label: `${delta > 0 ? "+" : "-"}${formatted}`,
    polarity: isGood ? "positive" : "negative"
  };
}

export function buildMetricCards(
  snapshot: AdminRuntimeSnapshot,
  history: AdminMetricHistoryPoint[] = []
): AdminMetricCard[] {
  const sessions = seriesFor(history, "network_active_sessions");
  const tickAvg = seriesFor(history, "gameloop_tick_avg_ms");
  const journal = seriesFor(history, "persistence_last_sequence_id");

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
      status: "healthy",
      series: sessions,
      trend: trendFor(sessions, true, "count")
    },
    {
      label: "Tick avg",
      value: formatMs(metric(snapshot, "gameloop_tick_avg_ms")),
      detail: "Game loop moving average",
      status: metric(snapshot, "gameloop_tick_avg_ms") > 50 ? "warning" : "healthy",
      series: tickAvg,
      trend: trendFor(tickAvg, false, "ms")
    },
    {
      label: "Journal",
      value: formatNumber(metric(snapshot, "persistence_last_sequence_id")),
      detail: "Last persistence sequence",
      status: "healthy",
      series: journal,
      trend: trendFor(journal, true, "count")
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

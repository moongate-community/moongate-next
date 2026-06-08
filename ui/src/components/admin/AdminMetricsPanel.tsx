import { useEffect, useMemo, useRef } from "react";
import * as echarts from "echarts/core";
import { GridComponent, TooltipComponent } from "echarts/components";
import { LineChart } from "echarts/charts";
import { CanvasRenderer } from "echarts/renderers";
import type { ComposeOption } from "echarts/core";
import type { GridComponentOption, TooltipComponentOption } from "echarts/components";
import type { LineSeriesOption } from "echarts/charts";
import { ChartSpline } from "lucide-react";
import type { AdminMetricHistoryPoint, AdminRuntimeSnapshot } from "../../types/admin";

echarts.use([CanvasRenderer, GridComponent, LineChart, TooltipComponent]);

type AdminMetricsPanelProps = {
  history: AdminMetricHistoryPoint[];
  snapshot: AdminRuntimeSnapshot;
};

type MetricChartConfig = {
  color: string;
  description: string;
  label: string;
  metricName: string;
  precision: number;
  unit: string;
};

type MetricChartOption = ComposeOption<
  GridComponentOption | LineSeriesOption | TooltipComponentOption
>;

const chartConfigs: MetricChartConfig[] = [
  {
    label: "Game loop tick avg",
    metricName: "gameloop_tick_avg_ms",
    unit: "ms",
    precision: 2,
    color: "#2f5c3b",
    description: "Moving average tick duration"
  },
  {
    label: "Game loop tick max",
    metricName: "gameloop_tick_max_ms",
    unit: "ms",
    precision: 2,
    color: "#b9802d",
    description: "Worst tick duration in the window"
  },
  {
    label: "Active sessions",
    metricName: "network_active_sessions",
    unit: "",
    precision: 0,
    color: "#487274",
    description: "Connected game network sessions"
  },
  {
    label: "Ingress queue",
    metricName: "network_ingress_queue_depth",
    unit: "",
    precision: 0,
    color: "#6f916c",
    description: "Pending client data waiting for parsing"
  },
  {
    label: "Event bus queue",
    metricName: "bus_tick_queue_depth",
    unit: "",
    precision: 0,
    color: "#9a2d2d",
    description: "Tick events waiting for dispatch"
  },
  {
    label: "Timer callback avg",
    metricName: "timer_callback_avg_ms",
    unit: "ms",
    precision: 2,
    color: "#536f74",
    description: "Average timer callback duration"
  },
  {
    label: "Persisted entities",
    metricName: "persistence_entities_total",
    unit: "",
    precision: 0,
    color: "#7b6b47",
    description: "Total persisted entities across types"
  },
  {
    label: "Journal sequence",
    metricName: "persistence_last_sequence_id",
    unit: "",
    precision: 0,
    color: "#7a5268",
    description: "Latest persistence journal sequence"
  }
];

export function AdminMetricsPanel({ history, snapshot }: AdminMetricsPanelProps) {
  const points = history.length > 0
                   ? history
                   : [
                       {
                         at: snapshot.collectedAt ?? new Date().toISOString(),
                         metrics: snapshot.metrics
                       }
                     ];

  const labels = points.map((point) => new Date(point.at).toLocaleTimeString());
  const lastSample = points.at(-1)?.at;

  return (
    <article className="admin-panel admin-metrics-board">
      <header>
        <div>
          <h3>Metrics</h3>
          <p>Live panels from the OpenMetrics endpoint.</p>
        </div>
        <div className="admin-metrics-board-status">
          <ChartSpline size={17} aria-hidden />
          5s refresh
        </div>
      </header>

      <div className="admin-metrics-toolbar">
        <span>{history.length} samples</span>
        <span>{lastSample ? `Last ${new Date(lastSample).toLocaleTimeString()}` : "Waiting for sample"}</span>
        <span>{snapshot.reachable ? "Source online" : "Source offline"}</span>
      </div>

      <div className="admin-chart-grid">
        {chartConfigs.map((config) => (
          <MetricChart key={config.metricName} config={config} labels={labels} points={points} />
        ))}
      </div>
    </article>
  );
}

function MetricChart({
  config,
  labels,
  points
}: {
  config: MetricChartConfig;
  labels: string[];
  points: AdminMetricHistoryPoint[];
}) {
  const chartRef = useRef<HTMLDivElement | null>(null);
  const values = useMemo(() => points.map((point) => point.metrics[config.metricName] ?? 0), [config.metricName, points]);
  const latest = values.at(-1) ?? 0;
  const chartOption = useMemo(() => buildChartOption(config, labels, values), [config, labels, values]);

  useEffect(() => {
    if (!chartRef.current) {
      return;
    }

    const chart = echarts.init(chartRef.current, undefined, { renderer: "canvas" });
    const resizeObserver = new ResizeObserver(() => chart.resize());

    resizeObserver.observe(chartRef.current);
    chart.setOption(chartOption, true);

    return () => {
      resizeObserver.disconnect();
      chart.dispose();
    };
  }, [chartOption]);

  return (
    <section className="admin-chart-panel">
      <header>
        <div>
          <h4>{config.label}</h4>
          <span>{config.description}</span>
        </div>
        <strong>{formatMetric(latest, config)}</strong>
      </header>
      <div ref={chartRef} className="admin-chart-surface" role="img" aria-label={`${config.label} chart`} />
    </section>
  );
}

function buildChartOption(config: MetricChartConfig, labels: string[], values: number[]): MetricChartOption {
  return {
    animation: true,
    animationDuration: 250,
    backgroundColor: "transparent",
    color: [config.color],
    grid: {
      top: 18,
      right: 18,
      bottom: 26,
      left: 44,
      containLabel: false
    },
    tooltip: {
      trigger: "axis",
      backgroundColor: "#ffffff",
      borderColor: "#cdd5cc",
      borderWidth: 1,
      textStyle: {
        color: "#27342d",
        fontSize: 12
      },
      valueFormatter: (value) => formatMetric(Number(value), config)
    },
    xAxis: {
      type: "category",
      boundaryGap: false,
      data: labels,
      axisLine: {
        lineStyle: {
          color: "#cdd5cc"
        }
      },
      axisLabel: {
        color: "#66736a",
        hideOverlap: true,
        fontSize: 10
      },
      axisTick: {
        show: false
      }
    },
    yAxis: {
      type: "value",
      min: "dataMin",
      max: "dataMax",
      splitLine: {
        lineStyle: {
          color: "#e3e8df",
          type: "dashed"
        }
      },
      axisLabel: {
        color: "#66736a",
        fontSize: 10,
        formatter: (value: number) => formatAxisMetric(value, config)
      }
    },
    series: [
      {
        type: "line",
        name: config.label,
        data: values,
        showSymbol: values.length <= 1,
        symbolSize: 5,
        smooth: true,
        lineStyle: {
          color: config.color,
          width: 2
        },
        itemStyle: {
          color: config.color
        },
        areaStyle: {
          color: {
            type: "linear",
            x: 0,
            y: 0,
            x2: 0,
            y2: 1,
            colorStops: [
              {
                offset: 0,
                color: withAlpha(config.color, "55")
              },
              {
                offset: 1,
                color: withAlpha(config.color, "05")
              }
            ]
          }
        }
      }
    ]
  };
}

function formatAxisMetric(value: number, config: MetricChartConfig): string {
  if (Math.abs(value) >= 1000) {
    return new Intl.NumberFormat("en-US", { notation: "compact", maximumFractionDigits: 1 }).format(value);
  }

  return formatMetric(value, { ...config, precision: Math.min(config.precision, 1) });
}

function formatMetric(value: number, config: MetricChartConfig): string {
  const formatted = new Intl.NumberFormat("en-US", {
    maximumFractionDigits: config.precision,
    minimumFractionDigits: config.precision > 0 && Math.abs(value) < 10 ? config.precision : 0
  }).format(value);

  return config.unit ? `${formatted} ${config.unit}` : formatted;
}

function withAlpha(hex: string, alpha: string): string {
  return `${hex}${alpha}`;
}

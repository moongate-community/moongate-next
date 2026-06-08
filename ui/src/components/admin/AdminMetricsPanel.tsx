import { useEffect, useMemo, useRef } from "react";
import * as echarts from "echarts/core";
import { GridComponent, TooltipComponent } from "echarts/components";
import { LineChart } from "echarts/charts";
import { CanvasRenderer } from "echarts/renderers";
import type { ComposeOption } from "echarts/core";
import type { GridComponentOption, TooltipComponentOption } from "echarts/components";
import type { LineSeriesOption } from "echarts/charts";
import { ChartSpline } from "lucide-react";
import { readChartTheme, type ChartTheme } from "../../lib/chartTheme";
import { useTheme, type Theme } from "../../lib/useTheme";
import type { AdminMetricHistoryPoint, AdminRuntimeSnapshot } from "../../types/admin";
import { Panel } from "./Panel";

echarts.use([CanvasRenderer, GridComponent, LineChart, TooltipComponent]);

type AdminMetricsPanelProps = {
  history: AdminMetricHistoryPoint[];
  snapshot: AdminRuntimeSnapshot;
};

type MetricChartConfig = {
  color: string;
  colorDark: string;
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
    colorDark: "#34d399",
    description: "Moving average tick duration"
  },
  {
    label: "Game loop tick max",
    metricName: "gameloop_tick_max_ms",
    unit: "ms",
    precision: 2,
    color: "#b9802d",
    colorDark: "#f5b14c",
    description: "Worst tick duration in the window"
  },
  {
    label: "Active sessions",
    metricName: "network_active_sessions",
    unit: "",
    precision: 0,
    color: "#487274",
    colorDark: "#5bb7ba",
    description: "Connected game network sessions"
  },
  {
    label: "Ingress queue",
    metricName: "network_ingress_queue_depth",
    unit: "",
    precision: 0,
    color: "#6f916c",
    colorDark: "#86c08a",
    description: "Pending client data waiting for parsing"
  },
  {
    label: "Event bus queue",
    metricName: "bus_tick_queue_depth",
    unit: "",
    precision: 0,
    color: "#9a2d2d",
    colorDark: "#f87171",
    description: "Tick events waiting for dispatch"
  },
  {
    label: "Timer callback avg",
    metricName: "timer_callback_avg_ms",
    unit: "ms",
    precision: 2,
    color: "#536f74",
    colorDark: "#8aa6ab",
    description: "Average timer callback duration"
  },
  {
    label: "Persisted entities",
    metricName: "persistence_entities_total",
    unit: "",
    precision: 0,
    color: "#7b6b47",
    colorDark: "#c4a86a",
    description: "Total persisted entities across types"
  },
  {
    label: "Journal sequence",
    metricName: "persistence_last_sequence_id",
    unit: "",
    precision: 0,
    color: "#7a5268",
    colorDark: "#c98bab",
    description: "Latest persistence journal sequence"
  }
];

export function AdminMetricsPanel({ history, snapshot }: AdminMetricsPanelProps) {
  const { theme } = useTheme();

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
    <Panel
      title="Metrics"
      icon={ChartSpline}
      action={
        <span className="inline-flex min-h-[32px] items-center gap-2 rounded-md border border-success/20 bg-success/10 px-2.5 text-xs font-bold text-success">
          <ChartSpline size={15} aria-hidden />
          5s refresh
        </span>
      }
    >
      <div className="grid gap-4">
        <p className="m-0 text-[13px] leading-relaxed text-fg-muted">Live panels from the OpenMetrics endpoint.</p>

        <div className="flex flex-wrap gap-2">
          {[
            `${history.length} samples`,
            lastSample ? `Last ${new Date(lastSample).toLocaleTimeString()}` : "Waiting for sample",
            snapshot.reachable ? "Source online" : "Source offline"
          ].map((chip) => (
            <span
              key={chip}
              className="inline-flex min-h-[28px] items-center rounded-md border border-border bg-muted px-2.5 font-mono text-[11px] font-medium text-fg-muted"
            >
              {chip}
            </span>
          ))}
        </div>

        <div className="grid grid-cols-1 gap-3 lg:grid-cols-2">
          {chartConfigs.map((config) => (
            <MetricChart key={config.metricName} config={config} labels={labels} points={points} theme={theme} />
          ))}
        </div>
      </div>
    </Panel>
  );
}

function MetricChart({
  config,
  labels,
  points,
  theme
}: {
  config: MetricChartConfig;
  labels: string[];
  points: AdminMetricHistoryPoint[];
  theme: Theme;
}) {
  const chartRef = useRef<HTMLDivElement | null>(null);
  const values = useMemo(() => points.map((point) => point.metrics[config.metricName] ?? 0), [config.metricName, points]);
  const latest = values.at(-1) ?? 0;
  const seriesColor = theme === "dark" ? config.colorDark : config.color;
  const chartOption = useMemo(
    () => buildChartOption(config, labels, values, seriesColor, readChartTheme(theme)),
    [config, labels, values, seriesColor, theme]
  );

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
    <section className="grid min-w-0 gap-3 rounded-lg border border-border bg-muted p-3.5">
      <header className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <h4 className="m-0 text-sm font-bold leading-tight text-fg">{config.label}</h4>
          <span className="mt-1 block text-xs leading-snug text-fg-muted">{config.description}</span>
        </div>
        <strong className="shrink-0 font-mono text-base font-bold leading-tight text-fg" style={{ color: seriesColor }}>
          {formatMetric(latest, config)}
        </strong>
      </header>
      <div
        ref={chartRef}
        className="h-[230px] min-w-0 rounded-md border border-border bg-surface"
        role="img"
        aria-label={`${config.label} chart`}
      />
    </section>
  );
}

function buildChartOption(
  config: MetricChartConfig,
  labels: string[],
  values: number[],
  seriesColor: string,
  chartTheme: ChartTheme
): MetricChartOption {
  return {
    animation: true,
    animationDuration: 250,
    backgroundColor: "transparent",
    color: [seriesColor],
    grid: {
      top: 18,
      right: 18,
      bottom: 26,
      left: 44,
      containLabel: false
    },
    tooltip: {
      trigger: "axis",
      backgroundColor: chartTheme.tooltipBg,
      borderColor: chartTheme.tooltipBorder,
      borderWidth: 1,
      textStyle: {
        color: chartTheme.tooltipText,
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
          color: chartTheme.axisLine
        }
      },
      axisLabel: {
        color: chartTheme.axisLabel,
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
          color: chartTheme.splitLine,
          type: "dashed"
        }
      },
      axisLabel: {
        color: chartTheme.axisLabel,
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
          color: seriesColor,
          width: 2
        },
        itemStyle: {
          color: seriesColor
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
                color: withAlpha(seriesColor, "55")
              },
              {
                offset: 1,
                color: withAlpha(seriesColor, "05")
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

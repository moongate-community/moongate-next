import { ArrowDownRight, ArrowUpRight, Minus } from "lucide-react";
import type { AdminMetricCard } from "../../types/admin";
import { statusAccentClass, statusTextClass, trendChipClass } from "./adminUi";
import { Sparkline } from "./Sparkline";

type AdminMetricStripProps = {
  metrics: AdminMetricCard[];
};

const trendIcon = {
  up: ArrowUpRight,
  down: ArrowDownRight,
  flat: Minus
} as const;

export function AdminMetricStrip({ metrics }: AdminMetricStripProps) {
  return (
    <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 xl:grid-cols-4">
      {metrics.map((metric, index) => {
        const TrendIcon = metric.trend ? trendIcon[metric.trend.direction] : null;

        return (
          <article
            key={metric.label}
            style={{ animationDelay: `${index * 50}ms` }}
            className="animate-rise relative grid gap-3 overflow-hidden rounded-lg border border-border bg-surface p-4 shadow-card transition-colors duration-150 hover:border-border-strong"
          >
            <span className={`absolute inset-y-0 left-0 w-1 ${statusAccentClass[metric.status]}`} aria-hidden />

            <div className="flex items-start justify-between gap-2">
              <span className="text-xs font-bold uppercase tracking-wide text-fg-subtle">{metric.label}</span>
              {metric.trend && TrendIcon && (
                <span
                  className={`inline-flex items-center gap-1 rounded-full px-1.5 py-0.5 font-mono text-[11px] font-semibold ${trendChipClass[metric.trend.polarity]}`}
                  title={metric.trend.label}
                >
                  <TrendIcon size={12} aria-hidden />
                  {metric.trend.label}
                </span>
              )}
            </div>

            <strong className="font-mono text-2xl font-bold leading-none text-fg">{metric.value}</strong>

            {metric.series && metric.series.length > 1 && (
              <Sparkline values={metric.series} className={`h-7 w-full ${statusTextClass[metric.status]}`} />
            )}

            <small className="text-xs leading-relaxed text-fg-muted">{metric.detail}</small>
          </article>
        );
      })}
    </div>
  );
}

import type { AdminMetricCard } from "../../types/admin";

type AdminMetricStripProps = {
  metrics: AdminMetricCard[];
};

export function AdminMetricStrip({ metrics }: AdminMetricStripProps) {
  return (
    <div className="admin-metrics admin-metrics-expanded">
      {metrics.map((metric) => (
        <article key={metric.label} className={`admin-status-${metric.status}`}>
          <span>{metric.label}</span>
          <strong>{metric.value}</strong>
          <small>{metric.detail}</small>
        </article>
      ))}
    </div>
  );
}

import { ScrollText } from "lucide-react";
import type { AdminRuntimeSnapshot } from "../../types/admin";

type AdminPersistencePanelProps = {
  snapshot: AdminRuntimeSnapshot;
};

function metric(snapshot: AdminRuntimeSnapshot, name: string): number {
  return snapshot.metrics[name] ?? 0;
}

function format(value: number): string {
  return new Intl.NumberFormat("en-US", { maximumFractionDigits: 0 }).format(value);
}

function formatUnixMs(value: number): string {
  if (value <= 0) {
    return "Not recorded";
  }

  return new Date(value).toLocaleString();
}

export function AdminPersistencePanel({ snapshot }: AdminPersistencePanelProps) {
  return (
    <article className="admin-panel">
      <header>
        <ScrollText size={20} aria-hidden />
        <h3>Persistence</h3>
      </header>
      <dl className="admin-definition-list">
        <div>
          <dt>Entities</dt>
          <dd>{format(metric(snapshot, "persistence_entities_total"))}</dd>
        </div>
        <div>
          <dt>Last sequence</dt>
          <dd>{format(metric(snapshot, "persistence_last_sequence_id"))}</dd>
        </div>
        <div>
          <dt>Snapshots written</dt>
          <dd>{format(metric(snapshot, "persistence_snapshots_written_total"))}</dd>
        </div>
        <div>
          <dt>Last snapshot</dt>
          <dd>{formatUnixMs(metric(snapshot, "persistence_last_snapshot_unixms"))}</dd>
        </div>
      </dl>
    </article>
  );
}

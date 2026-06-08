import { ScrollText } from "lucide-react";
import type { AdminRuntimeSnapshot } from "../../types/admin";
import { DefinitionList } from "./DefinitionList";
import { Panel } from "./Panel";

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
    <Panel title="Persistence" icon={ScrollText}>
      <DefinitionList
        items={[
          { term: "Entities", value: format(metric(snapshot, "persistence_entities_total")), mono: true },
          { term: "Last sequence", value: format(metric(snapshot, "persistence_last_sequence_id")), mono: true },
          { term: "Snapshots written", value: format(metric(snapshot, "persistence_snapshots_written_total")), mono: true },
          { term: "Last snapshot", value: formatUnixMs(metric(snapshot, "persistence_last_snapshot_unixms")), mono: true }
        ]}
      />
    </Panel>
  );
}

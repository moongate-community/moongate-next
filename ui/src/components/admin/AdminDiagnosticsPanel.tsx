import { Clipboard, ExternalLink, FileJson, HeartPulse, RefreshCw } from "lucide-react";
import type { AdminRuntimeSnapshot } from "../../types/admin";
import { Panel } from "./Panel";

type AdminDiagnosticsPanelProps = {
  snapshot: AdminRuntimeSnapshot;
  onCopyVersion: () => void;
  onRefresh: () => void;
};

const actionClass =
  "inline-flex min-h-[38px] items-center gap-2.5 rounded-md border border-border bg-surface px-3 text-[13px] font-semibold text-fg no-underline transition-[color,background-color,transform] duration-150 hover:bg-muted active:scale-[0.99] disabled:opacity-60 disabled:active:scale-100";

export function AdminDiagnosticsPanel({ snapshot, onCopyVersion, onRefresh }: AdminDiagnosticsPanelProps) {
  return (
    <Panel title="Diagnostics" icon={HeartPulse}>
      <div className="grid gap-2">
        <button type="button" onClick={onRefresh} className={actionClass}>
          <RefreshCw size={16} aria-hidden className="text-fg-subtle" />
          Refresh dashboard
        </button>
        <a href="/api/docs" target="_blank" rel="noreferrer" className={actionClass}>
          <ExternalLink size={16} aria-hidden className="text-fg-subtle" />
          Open Scalar docs
        </a>
        <a href="/metrics" target="_blank" rel="noreferrer" className={actionClass}>
          <FileJson size={16} aria-hidden className="text-fg-subtle" />
          Open raw metrics
        </a>
        <button type="button" onClick={onCopyVersion} disabled={!snapshot.server} className={actionClass}>
          <Clipboard size={16} aria-hidden className="text-fg-subtle" />
          Copy server version
        </button>
      </div>
    </Panel>
  );
}

import { Clipboard, ExternalLink, FileJson, HeartPulse, RefreshCw } from "lucide-react";
import type { AdminRuntimeSnapshot } from "../../types/admin";

type AdminDiagnosticsPanelProps = {
  snapshot: AdminRuntimeSnapshot;
  onCopyVersion: () => void;
  onRefresh: () => void;
};

export function AdminDiagnosticsPanel({ snapshot, onCopyVersion, onRefresh }: AdminDiagnosticsPanelProps) {
  return (
    <article className="admin-panel admin-diagnostics-panel">
      <header>
        <HeartPulse size={20} aria-hidden />
        <h3>Diagnostics</h3>
      </header>
      <div className="admin-action-list">
        <button onClick={onRefresh}>
          <RefreshCw size={16} aria-hidden />
          Refresh dashboard
        </button>
        <a href="/api/docs" target="_blank" rel="noreferrer">
          <ExternalLink size={16} aria-hidden />
          Open Scalar docs
        </a>
        <a href="/metrics" target="_blank" rel="noreferrer">
          <FileJson size={16} aria-hidden />
          Open raw metrics
        </a>
        <button onClick={onCopyVersion} disabled={!snapshot.server}>
          <Clipboard size={16} aria-hidden />
          Copy server version
        </button>
      </div>
    </article>
  );
}

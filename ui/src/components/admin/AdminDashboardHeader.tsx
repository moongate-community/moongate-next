import { Activity, RefreshCw } from "lucide-react";
import type { AdminRuntimeSnapshot } from "../../types/admin";

type AdminDashboardHeaderProps = {
  snapshot: AdminRuntimeSnapshot;
  loading: boolean;
  onRefresh: () => void;
};

export function AdminDashboardHeader({ snapshot, loading, onRefresh }: AdminDashboardHeaderProps) {
  return (
    <header className="dashboard-header admin-dashboard-header">
      <div>
        <h2>Admin dashboard</h2>
        <p>Runtime health, persistence, security, and diagnostics for the Moongate server.</p>
      </div>
      <div className="admin-header-actions">
        <div className={`status-pill ${snapshot.reachable ? "healthy" : "offline"}`}>
          <Activity size={16} aria-hidden />
          {snapshot.reachable ? "Live" : "Offline"}
        </div>
        <button className="admin-icon-action" onClick={onRefresh} disabled={loading} aria-label="Refresh dashboard">
          <RefreshCw size={17} aria-hidden />
        </button>
      </div>
    </header>
  );
}

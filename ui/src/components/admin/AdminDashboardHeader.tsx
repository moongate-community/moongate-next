import { Activity, RefreshCw } from "lucide-react";
import type { AdminRuntimeSnapshot } from "../../types/admin";

type AdminDashboardHeaderProps = {
  snapshot: AdminRuntimeSnapshot;
  loading: boolean;
  onRefresh: () => void;
};

export function AdminDashboardHeader({ snapshot, loading, onRefresh }: AdminDashboardHeaderProps) {
  const isLive = snapshot.reachable;

  return (
    <header className="flex flex-col gap-3 border-b border-border pb-4 sm:flex-row sm:items-center sm:justify-between">
      <div className="grid gap-1">
        <h2 className="text-[22px] font-semibold leading-tight tracking-tight text-fg">Admin dashboard</h2>
        <p className="m-0 text-[13px] text-fg-muted">{snapshot.server?.version ?? "Unknown version"}</p>
      </div>
      <div className="flex items-center gap-2">
        <span
          className={[
            "inline-flex min-h-[30px] items-center gap-2 rounded-md border px-2.5 text-xs font-medium",
            isLive
              ? "border-success/20 bg-success/10 text-success"
              : "border-danger/20 bg-danger/10 text-danger"
          ].join(" ")}
        >
          <span className={`h-2 w-2 rounded-full ${isLive ? "bg-success" : "bg-danger"}`} aria-hidden />
          <Activity size={14} aria-hidden />
          {isLive ? "Live" : "Offline"}
        </span>
        <button
          type="button"
          onClick={onRefresh}
          disabled={loading}
          aria-label="Refresh dashboard"
          title="Refresh dashboard"
          className="inline-flex h-8 w-8 items-center justify-center rounded-md text-fg-muted transition-colors duration-150 hover:bg-muted hover:text-fg disabled:opacity-60"
        >
          <RefreshCw size={16} aria-hidden className={loading ? "animate-spin" : ""} />
        </button>
      </div>
    </header>
  );
}

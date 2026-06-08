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
    <header className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
      <div className="grid gap-1.5">
        <h2 className="text-2xl font-bold leading-tight tracking-tight text-fg sm:text-[32px]">Admin dashboard</h2>
        <p className="max-w-xl text-sm leading-relaxed text-fg-muted">
          Runtime health, persistence, security, and diagnostics for the Moongate server.
        </p>
      </div>
      <div className="flex items-center gap-2.5">
        <span
          className={[
            "inline-flex min-h-[34px] items-center gap-2 rounded-full border px-3 text-xs font-bold",
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
          className="inline-flex h-9 w-9 items-center justify-center rounded-md border border-border bg-surface text-fg-muted transition-[color,background-color,transform] duration-150 hover:bg-muted hover:text-fg active:scale-[0.96] disabled:opacity-60 disabled:active:scale-100"
        >
          <RefreshCw size={17} aria-hidden className={loading ? "animate-spin" : ""} />
        </button>
      </div>
    </header>
  );
}

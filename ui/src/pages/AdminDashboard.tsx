import { Suspense, lazy, useCallback, useEffect, useMemo, useState } from "react";
import { AdminActivityTimeline } from "../components/admin/AdminActivityTimeline";
import { AdminDashboardHeader } from "../components/admin/AdminDashboardHeader";
import { AdminDiagnosticsPanel } from "../components/admin/AdminDiagnosticsPanel";
import { AdminMetricStrip } from "../components/admin/AdminMetricStrip";
import { AdminPersistencePanel } from "../components/admin/AdminPersistencePanel";
import { AdminRuntimePanel } from "../components/admin/AdminRuntimePanel";
import { AdminSecurityPanel } from "../components/admin/AdminSecurityPanel";
import { buildMetricCards, buildRuntimeServices } from "../data/adminDashboard";
import { getAdminRuntimeSnapshot, getOfflineSnapshot } from "../lib/adminClient";
import { me } from "../lib/authClient";
import type { AdminActivityEvent, AdminMetricHistoryPoint, AdminNavId, AdminRuntimeSnapshot } from "../types/admin";
import type { AuthUser } from "../types/auth";

const AdminMetricsPanel = lazy(() =>
  import("../components/admin/AdminMetricsPanel").then((module) => ({ default: module.AdminMetricsPanel }))
);

type AdminDashboardProps = {
  activeView: AdminNavId;
  accessToken: string;
  accessTokenExpiresAt: string;
  user: AuthUser;
};

export function AdminDashboard({ activeView, accessToken, accessTokenExpiresAt, user }: AdminDashboardProps) {
  const [snapshot, setSnapshot] = useState<AdminRuntimeSnapshot>(() => getOfflineSnapshot());
  const [verifiedUser, setVerifiedUser] = useState<AuthUser | null>(null);
  const [loading, setLoading] = useState(false);
  const [events, setEvents] = useState<AdminActivityEvent[]>([]);
  const [metricHistory, setMetricHistory] = useState<AdminMetricHistoryPoint[]>([]);

  const pushEvent = useCallback((label: string, detail: string, status: AdminActivityEvent["status"]) => {
    const at = new Date().toISOString();

    setEvents((current) => [
      {
        id: `${at}-${label}`,
        label,
        detail,
        at,
        status
      },
      ...current.slice(0, 7)
    ]);
  }, []);

  const refresh = useCallback(async (recordEvent = true) => {
    setLoading(true);

    const [runtimeResult, authResult] = await Promise.allSettled([
      getAdminRuntimeSnapshot(),
      me(accessToken)
    ]);

    if (runtimeResult.status === "fulfilled") {
      setSnapshot(runtimeResult.value);
      setMetricHistory((current) => appendMetricHistory(current, runtimeResult.value));

      if (recordEvent) {
        pushEvent("Dashboard refreshed", "Version and metrics loaded from the server", "healthy");
      }
    } else {
      setSnapshot(getOfflineSnapshot());

      if (recordEvent) {
        pushEvent(
          "Dashboard refresh failed",
          runtimeResult.reason instanceof Error ? runtimeResult.reason.message : "Unknown error",
          "offline"
        );
      }
    }

    if (authResult.status === "fulfilled") {
      setVerifiedUser(authResult.value);
    } else {
      setVerifiedUser(null);

      if (recordEvent) {
        pushEvent(
          "Auth check failed",
          authResult.reason instanceof Error ? authResult.reason.message : "Current token could not be verified",
          "warning"
        );
      }
    }

    setLoading(false);
  }, [accessToken, pushEvent]);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  useEffect(() => {
    if (activeView !== "metrics") {
      return;
    }

    void refresh(false);

    const timer = window.setInterval(() => {
      void refresh(false);
    }, 5000);

    return () => window.clearInterval(timer);
  }, [activeView, refresh]);

  const metrics = useMemo(() => buildMetricCards(snapshot, metricHistory), [snapshot, metricHistory]);
  const services = useMemo(() => buildRuntimeServices(snapshot), [snapshot]);

  async function copyVersion() {
    if (!snapshot.server) {
      return;
    }

    try {
      await navigator.clipboard.writeText(`${snapshot.server.version} ${snapshot.server.codename}`);
      pushEvent("Server version copied", `${snapshot.server.version} ${snapshot.server.codename}`, "healthy");
    } catch (error) {
      pushEvent("Copy failed", error instanceof Error ? error.message : "Clipboard is unavailable", "warning");
    }
  }

  return (
    <section className="grid gap-6 px-5 py-6 md:px-7">
      <AdminDashboardHeader snapshot={snapshot} loading={loading} onRefresh={refresh} />
      <AdminMetricStrip metrics={metrics} />

      <div className="grid items-start gap-4 xl:grid-cols-[minmax(0,1fr)_minmax(300px,360px)]">
        <div className="grid min-w-0 gap-4">
          {(activeView === "overview" || activeView === "runtime") && <AdminRuntimePanel services={services} />}
          {activeView === "metrics" && (
            <Suspense
              fallback={
                <div className="rounded-lg border border-border bg-surface p-5 text-sm font-semibold text-fg-muted shadow-card">
                  Loading metrics panels…
                </div>
              }
            >
              <AdminMetricsPanel snapshot={snapshot} history={metricHistory} />
            </Suspense>
          )}
          {(activeView === "overview" || activeView === "persistence") && <AdminPersistencePanel snapshot={snapshot} />}
          {(activeView === "overview" || activeView === "security") && (
            <AdminSecurityPanel user={user} verifiedUser={verifiedUser} accessTokenExpiresAt={accessTokenExpiresAt} />
          )}
          {activeView === "diagnostics" && (
            <AdminDiagnosticsPanel snapshot={snapshot} onRefresh={refresh} onCopyVersion={copyVersion} />
          )}
        </div>

        <aside className="grid min-w-0 gap-4">
          {activeView !== "diagnostics" && (
            <AdminDiagnosticsPanel snapshot={snapshot} onRefresh={refresh} onCopyVersion={copyVersion} />
          )}
          <AdminActivityTimeline events={events} />
        </aside>
      </div>
    </section>
  );
}

function appendMetricHistory(
  current: AdminMetricHistoryPoint[],
  snapshot: AdminRuntimeSnapshot
): AdminMetricHistoryPoint[] {
  const at = snapshot.collectedAt ?? new Date().toISOString();

  return [
    ...current,
    {
      at,
      metrics: snapshot.metrics
    }
  ].slice(-48);
}

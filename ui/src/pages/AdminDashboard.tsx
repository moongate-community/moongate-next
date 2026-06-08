import { Suspense, lazy, useCallback, useEffect, useMemo, useState } from "react";
import { AdminDashboardHeader } from "../components/admin/AdminDashboardHeader";
import { AdminMetricStrip } from "../components/admin/AdminMetricStrip";
import { AdminPersistencePanel } from "../components/admin/AdminPersistencePanel";
import { AdminRuntimePanel } from "../components/admin/AdminRuntimePanel";
import { AdminSecurityPanel } from "../components/admin/AdminSecurityPanel";
import { buildMetricCards, buildRuntimeServices } from "../data/adminDashboard";
import { getAdminRuntimeSnapshot, getOfflineSnapshot } from "../lib/adminClient";
import { me } from "../lib/authClient";
import type { AdminMetricHistoryPoint, AdminNavId, AdminRuntimeSnapshot } from "../types/admin";
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
  const [metricHistory, setMetricHistory] = useState<AdminMetricHistoryPoint[]>([]);

  const refresh = useCallback(async () => {
    setLoading(true);

    const [runtimeResult, authResult] = await Promise.allSettled([
      getAdminRuntimeSnapshot(),
      me(accessToken)
    ]);

    if (runtimeResult.status === "fulfilled") {
      setSnapshot(runtimeResult.value);
      setMetricHistory((current) => appendMetricHistory(current, runtimeResult.value));
    } else {
      setSnapshot(getOfflineSnapshot());
    }

    setVerifiedUser(authResult.status === "fulfilled" ? authResult.value : null);
    setLoading(false);
  }, [accessToken]);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  useEffect(() => {
    if (activeView !== "metrics") {
      return;
    }

    void refresh();

    const timer = window.setInterval(() => {
      void refresh();
    }, 5000);

    return () => window.clearInterval(timer);
  }, [activeView, refresh]);

  const metrics = useMemo(() => buildMetricCards(snapshot, metricHistory), [snapshot, metricHistory]);
  const services = useMemo(() => buildRuntimeServices(snapshot), [snapshot]);

  return (
    <section className="grid gap-6 px-5 py-6 md:px-7">
      <AdminDashboardHeader snapshot={snapshot} loading={loading} onRefresh={refresh} />
      <AdminMetricStrip metrics={metrics} />

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

        {activeView === "overview" ? (
          <div className="grid gap-4 lg:grid-cols-2">
            <AdminPersistencePanel snapshot={snapshot} />
            <AdminSecurityPanel user={user} verifiedUser={verifiedUser} accessTokenExpiresAt={accessTokenExpiresAt} />
          </div>
        ) : (
          <>
            {activeView === "persistence" && <AdminPersistencePanel snapshot={snapshot} />}
            {activeView === "security" && (
              <AdminSecurityPanel user={user} verifiedUser={verifiedUser} accessTokenExpiresAt={accessTokenExpiresAt} />
            )}
          </>
        )}
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

import { useCallback, useEffect, useMemo, useState } from "react";
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
import type { AdminActivityEvent, AdminNavId, AdminRuntimeSnapshot } from "../types/admin";
import type { AuthUser } from "../types/auth";

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

  const refresh = useCallback(async () => {
    setLoading(true);

    const [runtimeResult, authResult] = await Promise.allSettled([
      getAdminRuntimeSnapshot(),
      me(accessToken)
    ]);

    if (runtimeResult.status === "fulfilled") {
      setSnapshot(runtimeResult.value);
      pushEvent("Dashboard refreshed", "Version and metrics loaded from the server", "healthy");
    } else {
      setSnapshot(getOfflineSnapshot());
      pushEvent(
        "Dashboard refresh failed",
        runtimeResult.reason instanceof Error ? runtimeResult.reason.message : "Unknown error",
        "offline"
      );
    }

    if (authResult.status === "fulfilled") {
      setVerifiedUser(authResult.value);
    } else {
      setVerifiedUser(null);
      pushEvent(
        "Auth check failed",
        authResult.reason instanceof Error ? authResult.reason.message : "Current token could not be verified",
        "warning"
      );
    }

    setLoading(false);
  }, [accessToken, pushEvent]);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  const metrics = useMemo(() => buildMetricCards(snapshot), [snapshot]);
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
    <section className={`workspace admin-dashboard admin-view-${activeView}`}>
      <AdminDashboardHeader snapshot={snapshot} loading={loading} onRefresh={refresh} />
      <AdminMetricStrip metrics={metrics} />

      <div className="admin-layout">
        <div className="admin-main-column">
          {(activeView === "overview" || activeView === "runtime") && <AdminRuntimePanel services={services} />}
          {(activeView === "overview" || activeView === "persistence") && <AdminPersistencePanel snapshot={snapshot} />}
          {(activeView === "overview" || activeView === "security") && (
            <AdminSecurityPanel user={user} verifiedUser={verifiedUser} accessTokenExpiresAt={accessTokenExpiresAt} />
          )}
          {activeView === "diagnostics" && (
            <AdminDiagnosticsPanel snapshot={snapshot} onRefresh={refresh} onCopyVersion={copyVersion} />
          )}
        </div>

        <aside className="admin-side-column">
          {activeView !== "diagnostics" && (
            <AdminDiagnosticsPanel snapshot={snapshot} onRefresh={refresh} onCopyVersion={copyVersion} />
          )}
          <AdminActivityTimeline events={events} />
        </aside>
      </div>
    </section>
  );
}

import { Suspense, lazy, useCallback, useEffect, useMemo, useState } from "react";
import { AdminDashboardHeader } from "../components/admin/AdminDashboardHeader";
import { AdminPersistencePanel } from "../components/admin/AdminPersistencePanel";
import { AdminRuntimePanel } from "../components/admin/AdminRuntimePanel";
import { AdminSecurityPanel } from "../components/admin/AdminSecurityPanel";
import { ConsolePanel } from "../components/admin/ConsolePanel";
import { ItemTemplateCatalogPanel } from "../components/admin/itemTemplates/ItemTemplateCatalogPanel";
import { JobsPanel } from "../components/admin/jobs/JobsPanel";
import { LootTemplateCatalogPanel } from "../components/admin/lootTemplates/LootTemplateCatalogPanel";
import { MobileTemplateCatalogPanel } from "../components/admin/mobileTemplates/MobileTemplateCatalogPanel";
import { PluginManagementPanel } from "../components/admin/plugins/PluginManagementPanel";
import { UserManagementPanel } from "../components/admin/users/UserManagementPanel";
import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { buildRuntimeServices } from "../data/adminDashboard";
import { getAdminRuntimeSnapshot, getOfflineSnapshot } from "../lib/adminClient";
import { me } from "../lib/authClient";
import type { AdminMetricHistoryPoint, AdminNavId, AdminRuntimeSnapshot } from "../types/admin";
import type { AdminCommandTarget } from "../types/adminCommandTarget";
import type { AuthUser } from "../types/auth";

const AdminMetricsPanel = lazy(() =>
  import("../components/admin/AdminMetricsPanel").then((module) => ({ default: module.AdminMetricsPanel }))
);

type AdminDashboardProps = {
  activeView: AdminNavId;
  detailId?: string | null;
  accessToken: string;
  accessTokenExpiresAt: string;
  commandTarget?: AdminCommandTarget | null;
  user: AuthUser;
  onLootTemplateOpen?: (id: string) => void;
  onRuntimeSnapshotChange?: (snapshot: AdminRuntimeSnapshot) => void;
};

export function AdminDashboard({
  activeView,
  detailId,
  accessToken,
  accessTokenExpiresAt,
  commandTarget,
  user,
  onLootTemplateOpen,
  onRuntimeSnapshotChange
}: AdminDashboardProps) {
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
      onRuntimeSnapshotChange?.(runtimeResult.value);
      setMetricHistory((current) => appendMetricHistory(current, runtimeResult.value));
    } else {
      const offlineSnapshot = getOfflineSnapshot();
      setSnapshot(offlineSnapshot);
      onRuntimeSnapshotChange?.(offlineSnapshot);
    }

    setVerifiedUser(authResult.status === "fulfilled" ? authResult.value : null);
    setLoading(false);
  }, [accessToken, onRuntimeSnapshotChange]);

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

  const services = useMemo(() => buildRuntimeServices(snapshot), [snapshot]);

  if (activeView === "console") {
    return <ConsolePanel accessToken={accessToken} />;
  }

  return (
    <section className="grid gap-5 px-4 py-5 md:px-6">
      <AdminDashboardHeader loading={loading} onRefresh={refresh} />

      <div className="grid min-w-0 gap-4">
        {activeView === "overview" && <AdminRuntimePanel services={services} />}

        {activeView === "metrics" && (
          <Suspense
            fallback={
              <Card className="rounded-md border-border bg-surface py-0 shadow-none">
                <CardContent className="grid gap-3 p-4">
                  <Skeleton className="h-5 w-48" />
                  <Skeleton className="h-[220px] w-full" />
                </CardContent>
              </Card>
            }
          >
            <AdminMetricsPanel snapshot={snapshot} history={metricHistory} />
          </Suspense>
        )}

        {activeView === "users" && (
          <UserManagementPanel
            accessToken={accessToken}
            commandTarget={commandTarget?.kind === "user" ? commandTarget : null}
          />
        )}
        {activeView === "itemTemplates" && (
          <ItemTemplateCatalogPanel accessToken={accessToken} detailId={detailId} />
        )}
        {activeView === "mobileTemplates" && (
          <MobileTemplateCatalogPanel
            accessToken={accessToken}
            detailId={detailId}
            onLootTemplateOpen={onLootTemplateOpen}
          />
        )}
        {activeView === "lootTemplates" && <LootTemplateCatalogPanel accessToken={accessToken} detailId={detailId} />}
        {activeView === "plugins" && <PluginManagementPanel accessToken={accessToken} />}
        {activeView === "jobs" && <JobsPanel accessToken={accessToken} />}

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

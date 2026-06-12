import { useRef, useState } from "react";
import { Navigate, useLocation, useNavigate } from "react-router-dom";
import { AppShell } from "../components/AppShell";
import { AdminDashboard } from "../pages/AdminDashboard";
import { useSession } from "../lib/SessionContext";
import { adminPathFor, parseAdminLocation } from "../data/navigation";
import type { AdminCommandTarget } from "../types/adminCommandTarget";
import type { AdminRuntimeSnapshot } from "../types/admin";
import type { AdminUser } from "../types/users";

/** Admin shell: derives the active view from the URL and renders the dashboard. */
export function AdminLayout() {
  const { session, signOut } = useSession();
  const location = useLocation();
  const navigate = useNavigate();
  const [runtimeSnapshot, setRuntimeSnapshot] = useState<AdminRuntimeSnapshot | null>(null);
  const sequence = useRef(0);

  if (session && session.user.level === "Player") {
    return <Navigate to="/player/profile" replace />;
  }

  if (!session) {
    return <Navigate to="/login" replace />;
  }

  const { view } = parseAdminLocation(location.pathname);
  const openUser = (location.state as { openUser?: AdminUser } | null)?.openUser ?? null;
  let commandTarget: AdminCommandTarget | null = null;

  if (view === "users" && openUser) {
    sequence.current += 1;
    commandTarget = { kind: "user", user: openUser, sequence: sequence.current };
  }

  return (
    <AppShell
      user={session.user}
      section="admin"
      runtimeSnapshot={runtimeSnapshot}
      onLogout={async () => {
        await signOut();
        navigate("/login", { replace: true });
      }}
    >
      <AdminDashboard
        activeView={view}
        accessToken={session.accessToken}
        accessTokenExpiresAt={session.accessTokenExpiresAt}
        user={session.user}
        commandTarget={commandTarget}
        onLootTemplateOpen={(id) => navigate(adminPathFor("lootTemplates", id))}
        onRuntimeSnapshotChange={setRuntimeSnapshot}
      />
    </AppShell>
  );
}

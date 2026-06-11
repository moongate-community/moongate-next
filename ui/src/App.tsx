import { useMemo, useState } from "react";
import type { Action } from "kbar";
import { LogOut } from "lucide-react";
import { CommandPalette } from "./components/CommandPalette";
import { TooltipProvider } from "@/components/ui/tooltip";
import { AppShell } from "./components/AppShell";
import { LoginView } from "./components/LoginView";
import { adminGroups, adminItems, playerGroups, playerItems } from "./data/navigation";
import { login, logout } from "./lib/authClient";
import { clearStoredAuth, readStoredAuth, writeStoredAuth } from "./lib/authStorage";
import { AdminDashboard } from "./pages/AdminDashboard";
import { PlayerDashboard } from "./pages/PlayerDashboard";
import type { AdminNavId, AdminRuntimeSnapshot } from "./types/admin";
import type { AuthTokenResponse } from "./types/auth";

type AppSection = "admin" | "player";
type PlayerNavId = "profile" | "adventures";

function sectionFromPath(): AppSection {
  return window.location.pathname.startsWith("/admin") ? "admin" : "player";
}

export default function App() {
  const [session, setSession] = useState<AuthTokenResponse | null>(() => readStoredAuth());
  const [section, setSection] = useState<AppSection>(() => sectionFromPath());
  const [adminNav, setAdminNav] = useState<AdminNavId>("overview");
  const [playerNav, setPlayerNav] = useState<PlayerNavId>("profile");
  const [adminRuntimeSnapshot, setAdminRuntimeSnapshot] = useState<AdminRuntimeSnapshot | null>(null);

  async function handleLogin(nextSection: AppSection, username: string, password: string) {
    const next = await login(username, password);
    writeStoredAuth(next);
    setSession(next);
    setSection(nextSection);
    setAdminRuntimeSnapshot(null);
  }

  async function handleLogout() {
    if (session) {
      await logout(session.refreshToken);
    }

    clearStoredAuth();
    setSession(null);
    setAdminRuntimeSnapshot(null);
  }

  const commandActions = useMemo<Action[]>(() => {
    if (!session) {
      return [];
    }

    const actions: Action[] = [];
    const canUseAdmin = session.user.level !== "Player";
    const adminSections = new Map(adminGroups.flatMap((group) => group.itemIds.map((itemId) => [itemId, group.label])));
    const playerSections = new Map(playerGroups.flatMap((group) => group.itemIds.map((itemId) => [itemId, group.label])));

    if (canUseAdmin) {
      adminItems.forEach((item) => {
        actions.push({
          id: `admin:${item.id}`,
          name: item.label,
          subtitle: "Admin console",
          keywords: `admin ${item.label}`,
          section: adminSections.get(item.id) ?? "Admin",
          icon: <item.icon size={16} aria-hidden />,
          perform: () => {
            window.history.replaceState(null, "", "/admin");
            setSection("admin");
            setAdminNav(item.id);
          }
        });
      });
    }

    playerItems.forEach((item) => {
      actions.push({
        id: `player:${item.id}`,
        name: item.label,
        subtitle: "Player portal",
        keywords: `player ${item.label}`,
        section: playerSections.get(item.id) ?? "Portal",
        icon: <item.icon size={16} aria-hidden />,
        perform: () => {
          window.history.replaceState(null, "", "/");
          setSection("player");
          setPlayerNav(item.id);
        }
      });
    });

    actions.push({
      id: "session:logout",
      name: "Logout",
      subtitle: session.user.username,
      keywords: "sign out session",
      section: "Session",
      icon: <LogOut size={16} aria-hidden />,
      perform: () => {
        void handleLogout();
      }
    });

    return actions;
  }, [session]);

  if (!session) {
    return <LoginView section={sectionFromPath()} onLogin={handleLogin} />;
  }

  const activeItemId = section === "admin" ? adminNav : playerNav;

  return (
    <TooltipProvider delayDuration={200}>
      <CommandPalette actions={commandActions}>
        <AppShell
          user={session.user}
          section={section}
          activeItemId={activeItemId}
          runtimeSnapshot={section === "admin" ? adminRuntimeSnapshot : null}
          onItemChange={(itemId) => {
            if (section === "admin") {
              setAdminNav(itemId as AdminNavId);

              return;
            }

            setPlayerNav(itemId as PlayerNavId);
          }}
          onLogout={handleLogout}
        >
          {section === "admin" ? (
            <AdminDashboard
              activeView={adminNav}
              accessToken={session.accessToken}
              accessTokenExpiresAt={session.accessTokenExpiresAt}
              user={session.user}
              onRuntimeSnapshotChange={setAdminRuntimeSnapshot}
            />
          ) : (
            <PlayerDashboard user={session.user} />
          )}
        </AppShell>
      </CommandPalette>
    </TooltipProvider>
  );
}

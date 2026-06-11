import { useState } from "react";
import { TooltipProvider } from "@/components/ui/tooltip";
import { AppShell } from "./components/AppShell";
import { LoginView } from "./components/LoginView";
import { login, logout } from "./lib/authClient";
import { clearStoredAuth, readStoredAuth, writeStoredAuth } from "./lib/authStorage";
import { AdminDashboard } from "./pages/AdminDashboard";
import { PlayerDashboard } from "./pages/PlayerDashboard";
import type { AdminNavId } from "./types/admin";
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

  async function handleLogin(nextSection: AppSection, username: string, password: string) {
    const next = await login(username, password);
    writeStoredAuth(next);
    setSession(next);
    setSection(nextSection);
  }

  async function handleLogout() {
    if (session) {
      await logout(session.refreshToken);
    }

    clearStoredAuth();
    setSession(null);
  }

  if (!session) {
    return <LoginView section={sectionFromPath()} onLogin={handleLogin} />;
  }

  const activeItemId = section === "admin" ? adminNav : playerNav;

  return (
    <TooltipProvider delayDuration={200}>
      <AppShell
        user={session.user}
        section={section}
        activeItemId={activeItemId}
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
          />
        ) : (
          <PlayerDashboard user={session.user} />
        )}
      </AppShell>
    </TooltipProvider>
  );
}

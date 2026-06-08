import { useState } from "react";
import { AppShell } from "./components/AppShell";
import { LoginView } from "./components/LoginView";
import { login, logout } from "./lib/authClient";
import { clearStoredAuth, readStoredAuth, writeStoredAuth } from "./lib/authStorage";
import { AdminDashboard } from "./pages/AdminDashboard";
import { PlayerDashboard } from "./pages/PlayerDashboard";
import type { AuthTokenResponse } from "./types/auth";

type AppSection = "admin" | "player";

export default function App() {
  const [session, setSession] = useState<AuthTokenResponse | null>(() => readStoredAuth());
  const [section, setSection] = useState<AppSection>("admin");

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
    return <LoginView onLogin={handleLogin} />;
  }

  return (
    <AppShell user={session.user} section={section} onSectionChange={setSection} onLogout={handleLogout}>
      {section === "admin" ? <AdminDashboard /> : <PlayerDashboard user={session.user} />}
    </AppShell>
  );
}

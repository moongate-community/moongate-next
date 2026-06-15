import { Navigate, useNavigate } from "react-router-dom";
import { AppShell } from "../components/AppShell";
import { PlayerDashboard } from "../pages/PlayerDashboard";
import { useSession } from "../lib/SessionContext";

/** Player shell: renders the player dashboard. */
export function PlayerLayout() {
  const { session, signOut } = useSession();
  const navigate = useNavigate();

  if (!session) {
    return <Navigate to="/login" replace />;
  }

  return (
    <AppShell
      user={session.user}
      section="player"
      onLogout={async () => {
        await signOut();
        navigate("/login", { replace: true });
      }}
    >
      <PlayerDashboard user={session.user} />
    </AppShell>
  );
}

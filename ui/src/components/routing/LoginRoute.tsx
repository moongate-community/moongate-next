import { Navigate, useLocation, useNavigate } from "react-router-dom";
import { LoginView } from "../LoginView";
import { useSession } from "../../lib/SessionContext";

function defaultHomeFor(level: string): string {
  return level === "Player" ? "/player/profile" : "/admin/overview";
}

/** The /login route: renders LoginView, redirects home (or to ?from) after auth. */
export function LoginRoute() {
  const { session, signIn } = useSession();
  const navigate = useNavigate();
  const location = useLocation();
  const params = new URLSearchParams(location.search);
  const from = params.get("from");
  const section = from && from.startsWith("/player") ? "player" : "admin";

  if (session) {
    return <Navigate to={from ?? defaultHomeFor(session.user.level)} replace />;
  }

  return (
    <LoginView
      section={section}
      onLogin={async (_section, username, password) => {
        const next = await signIn(username, password);
        navigate(from ?? defaultHomeFor(next.user.level), { replace: true });
      }}
    />
  );
}

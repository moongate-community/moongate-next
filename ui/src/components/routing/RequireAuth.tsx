import { Navigate, useLocation } from "react-router-dom";
import type { ReactNode } from "react";
import { useSession } from "../../lib/SessionContext";

/** Redirects to /login (remembering the target) when there is no session. */
export function RequireAuth({ children }: { children: ReactNode }) {
  const { session } = useSession();
  const location = useLocation();

  if (!session) {
    const from = encodeURIComponent(`${location.pathname}${location.search}`);

    return <Navigate to={`/login?from=${from}`} replace />;
  }

  return <>{children}</>;
}

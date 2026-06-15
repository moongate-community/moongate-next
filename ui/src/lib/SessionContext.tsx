import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from "react";
import { login as apiLogin, logout as apiLogout } from "./authClient";
import { clearStoredAuth, readStoredAuth, writeStoredAuth } from "./authStorage";
import type { AuthTokenResponse } from "../types/auth";

type SessionContextValue = {
  session: AuthTokenResponse | null;
  signIn: (username: string, password: string) => Promise<AuthTokenResponse>;
  signOut: () => Promise<void>;
};

const SessionContext = createContext<SessionContextValue | null>(null);

export function SessionProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<AuthTokenResponse | null>(() => readStoredAuth());

  const signIn = useCallback(async (username: string, password: string) => {
    const next = await apiLogin(username, password);
    writeStoredAuth(next);
    setSession(next);

    return next;
  }, []);

  const signOut = useCallback(async () => {
    if (session) {
      await apiLogout(session.refreshToken);
    }

    clearStoredAuth();
    setSession(null);
  }, [session]);

  const value = useMemo<SessionContextValue>(() => ({ session, signIn, signOut }), [session, signIn, signOut]);

  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>;
}

export function useSession(): SessionContextValue {
  const value = useContext(SessionContext);

  if (!value) {
    throw new Error("useSession must be used within a SessionProvider");
  }

  return value;
}

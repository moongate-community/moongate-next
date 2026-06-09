import { type FormEvent, Suspense, lazy, useState } from "react";
import { LogIn, Shield, Sparkles } from "lucide-react";

// Lazy so three.js/vanta load only when the admin login renders, not in the initial bundle.
const VantaBackground = lazy(() =>
  import("./VantaBackground").then((module) => ({ default: module.VantaBackground }))
);

type LoginSection = "admin" | "player";

type LoginViewProps = {
  onLogin: (section: LoginSection, username: string, password: string) => Promise<void>;
};

export function LoginView({ onLogin }: LoginViewProps) {
  const [section, setSection] = useState<LoginSection>("admin");
  const [username, setUsername] = useState("admin");
  const [password, setPassword] = useState("admin");
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);
  const isAdmin = section === "admin";

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setBusy(true);
    setError("");

    try {
      await onLogin(section, username, password);
    } catch {
      setError("Invalid credentials");
    } finally {
      setBusy(false);
    }
  }

  return (
    <main className={`login-screen ${isAdmin ? "admin-login-screen" : "player-login-screen"}`}>
      {isAdmin ? (
        <Suspense fallback={null}>
          <VantaBackground />
        </Suspense>
      ) : null}
      <section className={`login-panel ${isAdmin ? "admin-login-panel" : "player-login-panel"}`}>
        <img src="/images/moongate_logo.png" alt="Moongate" className="login-logo" />
        <div className="login-mode-switch" role="tablist" aria-label="Login type">
          <button
            type="button"
            className={isAdmin ? "active" : ""}
            onClick={() => setSection("admin")}
            aria-selected={isAdmin}
            role="tab"
          >
            <Shield size={16} aria-hidden />
            Admin
          </button>
          <button
            type="button"
            className={!isAdmin ? "active" : ""}
            onClick={() => setSection("player")}
            aria-selected={!isAdmin}
            role="tab"
          >
            <Sparkles size={16} aria-hidden />
            Player
          </button>
        </div>
        <h1>{isAdmin ? "Admin console" : "Player portal"}</h1>
        <p className="login-copy">
          {isAdmin
            ? "Access server operations, runtime state, and shard administration."
            : "Access your account, characters, and shard entry points."}
        </p>
        <form onSubmit={submit} className="login-form">
          <label>
            Username
            <input value={username} onChange={(event) => setUsername(event.target.value)} autoComplete="username" />
          </label>
          <label>
            Password
            <input
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              type="password"
              autoComplete="current-password"
            />
          </label>
          {error ? <p className="form-error">{error}</p> : null}
          <button type="submit" disabled={busy}>
            <LogIn size={18} aria-hidden />
            {busy ? "Signing in" : isAdmin ? "Enter admin console" : "Enter player portal"}
          </button>
        </form>
      </section>
    </main>
  );
}

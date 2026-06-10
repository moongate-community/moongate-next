import { type FormEvent, Suspense, lazy, useState } from "react";
import { LogIn, Shield, Sparkles } from "lucide-react";

const VantaBackground = lazy(() =>
  import("./VantaBackground").then((module) => ({ default: module.VantaBackground }))
);

type LoginSection = "admin" | "player";

type LoginViewProps = {
  section: LoginSection;
  onLogin: (section: LoginSection, username: string, password: string) => Promise<void>;
};

export function LoginView({ section, onLogin }: LoginViewProps) {
  const [username, setUsername] = useState("admin");
  const [password, setPassword] = useState("admin");
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);
  const isAdmin = section === "admin";
  const sectionIcon = isAdmin ? <Shield size={16} aria-hidden /> : <Sparkles size={16} aria-hidden />;

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
    <main
      className={[
        "relative grid min-h-screen place-items-center overflow-hidden px-4 py-8 text-fg",
        isAdmin ? "bg-[#16231b]" : "bg-bg"
      ].join(" ")}
    >
      {isAdmin ? (
        <Suspense fallback={null}>
          <VantaBackground />
        </Suspense>
      ) : null}

      <section className="relative z-10 w-full max-w-[380px] rounded-md border border-border bg-surface/95 shadow-raised backdrop-blur">
        <header className="border-b border-border p-4">
          <div className="mb-4 flex items-center gap-2">
            <img src="/images/moongate_logo.png" alt="Moongate" className="h-8 w-8 shrink-0 object-contain" />
            <span className="truncate text-base font-semibold tracking-tight text-fg">Moongate</span>
          </div>
          <div className="mb-3 inline-flex h-8 w-8 items-center justify-center rounded-md bg-muted text-fg-muted">
            {sectionIcon}
          </div>
          <h1 className="m-0 text-xl font-semibold leading-tight tracking-tight text-fg">
            {isAdmin ? "Admin console" : "Player portal"}
          </h1>
        </header>

        <form onSubmit={submit} className="grid gap-3 p-4">
          <label className="grid gap-1.5 text-[13px] font-medium text-fg-muted">
            Username
            <input
              value={username}
              onChange={(event) => setUsername(event.target.value)}
              autoComplete="username"
              className="h-9 rounded-md border border-border bg-bg px-2.5 text-[13px] text-fg outline-none transition-colors focus:border-border-strong focus:bg-surface"
            />
          </label>
          <label className="grid gap-1.5 text-[13px] font-medium text-fg-muted">
            Password
            <input
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              type="password"
              autoComplete="current-password"
              className="h-9 rounded-md border border-border bg-bg px-2.5 text-[13px] text-fg outline-none transition-colors focus:border-border-strong focus:bg-surface"
            />
          </label>
          {error ? <p className="m-0 text-[13px] font-medium text-danger">{error}</p> : null}
          <button
            type="submit"
            disabled={busy}
            className="mt-1 inline-flex min-h-[36px] items-center justify-center gap-2 rounded-md bg-accent px-3 text-[13px] font-medium text-accent-fg transition-opacity duration-150 hover:opacity-90 disabled:opacity-60"
          >
            <LogIn size={16} aria-hidden />
            {busy ? "Signing in" : isAdmin ? "Enter admin console" : "Enter player portal"}
          </button>
          <a
            href={isAdmin ? "/" : "/admin"}
            className="justify-self-center rounded-md px-2 py-1 text-xs font-medium text-fg-muted transition-colors hover:bg-muted hover:text-fg"
          >
            {isAdmin ? "Player portal" : "Admin console"}
          </a>
        </form>
      </section>
    </main>
  );
}

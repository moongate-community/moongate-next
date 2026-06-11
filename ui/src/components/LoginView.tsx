import { type FormEvent, Suspense, lazy, useState } from "react";
import { LogIn, Shield, Sparkles } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

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

      <Card className="relative z-10 w-full max-w-[380px] gap-0 rounded-md border-border bg-surface/95 py-0 shadow-raised backdrop-blur">
        <CardHeader className="border-b border-border p-4">
          <div className="mb-4 flex items-center gap-2">
            <img src="/images/moongate_logo.png" alt="Moongate" className="h-8 w-8 shrink-0 object-contain" />
            <span className="truncate text-base font-semibold tracking-tight text-fg">Moongate</span>
          </div>
          <div className="mb-3 inline-flex h-8 w-8 items-center justify-center rounded-md bg-muted text-fg-muted">
            {sectionIcon}
          </div>
          <CardTitle className="m-0 text-xl font-semibold leading-tight tracking-tight text-fg">
            {isAdmin ? "Admin console" : "Player portal"}
          </CardTitle>
        </CardHeader>

        <CardContent className="p-4">
          <form onSubmit={submit} className="grid gap-3">
            <div className="grid gap-1.5">
              <Label htmlFor="login-username" className="text-[13px] text-fg-muted">
                Username
              </Label>
              <Input
                id="login-username"
                value={username}
                onChange={(event) => setUsername(event.target.value)}
                autoComplete="username"
                className="bg-bg text-[13px] text-fg focus-visible:bg-surface"
              />
            </div>
            <div className="grid gap-1.5">
              <Label htmlFor="login-password" className="text-[13px] text-fg-muted">
                Password
              </Label>
              <Input
                id="login-password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                type="password"
                autoComplete="current-password"
                className="bg-bg text-[13px] text-fg focus-visible:bg-surface"
              />
            </div>
            {error ? <p className="m-0 text-[13px] font-medium text-danger">{error}</p> : null}
            <Button type="submit" disabled={busy} className="mt-1 min-h-[36px] gap-2 text-[13px]">
              <LogIn size={16} aria-hidden />
              {busy ? "Signing in" : isAdmin ? "Enter admin console" : "Enter player portal"}
            </Button>
            <Button asChild variant="ghost" size="sm" className="justify-self-center text-xs text-fg-muted hover:bg-muted hover:text-fg">
              <a href={isAdmin ? "/" : "/admin"}>{isAdmin ? "Player portal" : "Admin console"}</a>
            </Button>
          </form>
        </CardContent>
      </Card>
    </main>
  );
}

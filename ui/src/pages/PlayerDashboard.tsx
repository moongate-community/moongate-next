import { Sparkles, UserRound } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import type { AuthUser } from "../types/auth";

type PlayerDashboardProps = {
  user: AuthUser;
};

const characterSlots = [
  {
    name: "No character selected",
    meta: "Roster unavailable"
  },
  {
    name: "Shard access",
    meta: "UO client account ready"
  }
];

export function PlayerDashboard({ user }: PlayerDashboardProps) {
  return (
    <section className="grid gap-5 px-4 py-5 md:px-6">
      <header className="flex max-w-3xl items-center gap-4">
        <div className="grid h-14 w-14 shrink-0 place-items-center rounded-md bg-info/10 text-info" aria-hidden>
          <UserRound size={34} />
        </div>
        <div className="min-w-0">
          <h2 className="m-0 text-2xl font-semibold tracking-tight text-fg">Player dashboard</h2>
          <p className="m-0 mt-1 truncate text-sm text-fg-muted">{user.username}</p>
        </div>
      </header>

      <div className="grid max-w-3xl grid-cols-1 gap-3 sm:grid-cols-2">
        <SummaryCard label="Account" value={user.isActive ? "Ready" : "Inactive"} tone={user.isActive ? "success" : "danger"} />
        <SummaryCard label="Access level" value={user.level} />
      </div>

      <div className="grid gap-4 lg:grid-cols-[minmax(0,1fr)_340px]">
        <Card className="rounded-md border-border bg-surface py-0 shadow-none">
          <CardHeader className="flex flex-row items-center justify-between gap-3 border-b border-border px-4 py-3">
            <h3 className="m-0 text-sm font-semibold tracking-tight text-fg">Characters</h3>
            <Badge variant="outline" className="rounded-md border-border bg-bg text-fg-muted">0 linked</Badge>
          </CardHeader>
          <CardContent className="grid gap-2 p-4">
            {characterSlots.map((slot) => (
              <Card key={slot.name} className="grid grid-cols-[28px_minmax(0,1fr)] items-start gap-3 rounded-md border-border bg-bg p-3 py-3 shadow-none">
                <Sparkles size={18} aria-hidden />
                <div>
                  <strong className="block text-sm font-semibold text-fg">{slot.name}</strong>
                  <p className="m-0 mt-1 text-[13px] leading-relaxed text-fg-muted">{slot.meta}</p>
                </div>
              </Card>
            ))}
          </CardContent>
        </Card>

        <Card className="rounded-md border-info/20 bg-info text-white shadow-none">
          <CardContent className="grid gap-3 p-5">
            <span className="text-xs font-bold uppercase text-white/75">Next step</span>
            <strong className="text-2xl font-semibold leading-tight">Connect a game client</strong>
            <p className="m-0 text-sm leading-relaxed text-white/85">Characters and session state will appear here.</p>
          </CardContent>
        </Card>
      </div>
    </section>
  );
}

function SummaryCard({ label, tone, value }: { label: string; tone?: "success" | "danger"; value: string }) {
  return (
    <Card className="rounded-md border-border bg-surface py-0 shadow-none">
      <CardContent className="grid min-h-[104px] content-start gap-2 p-4">
        <span className="text-xs font-bold uppercase text-fg-subtle">{label}</span>
        <strong className={`text-2xl font-semibold leading-tight ${tone === "success" ? "text-success" : tone === "danger" ? "text-danger" : "text-fg"}`}>
          {value}
        </strong>
      </CardContent>
    </Card>
  );
}

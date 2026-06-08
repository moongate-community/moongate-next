import { useState, type ReactNode } from "react";
import { LogOut, PanelLeftClose, PanelLeftOpen } from "lucide-react";
import { adminItems, playerItems } from "../data/navigation";
import { useTheme } from "../lib/useTheme";
import type { AdminNavId } from "../types/admin";
import type { AuthUser } from "../types/auth";
import { ThemeToggle } from "./ThemeToggle";

type AppSection = "admin" | "player";
type PlayerNavId = (typeof playerItems)[number]["id"];

type AppShellProps = {
  user: AuthUser;
  section: AppSection;
  activeItemId: AdminNavId | PlayerNavId;
  onItemChange: (itemId: AdminNavId | PlayerNavId) => void;
  onLogout: () => Promise<void>;
  children: ReactNode;
};

function initialsOf(value: string): string {
  const trimmed = value.trim();

  if (trimmed.length === 0) {
    return "?";
  }

  return trimmed.slice(0, 2).toUpperCase();
}

export function AppShell({
  user,
  section,
  activeItemId,
  onItemChange,
  onLogout,
  children
}: AppShellProps) {
  const items = section === "admin" ? adminItems : playerItems;
  const [isSidebarCollapsed, setIsSidebarCollapsed] = useState(false);
  const { theme, toggleTheme } = useTheme();

  const columns = isSidebarCollapsed
    ? "md:grid-cols-[76px_minmax(0,1fr)]"
    : "md:grid-cols-[248px_minmax(0,1fr)]";

  return (
    <div className={`grid min-h-screen grid-cols-1 ${columns} bg-bg text-fg transition-[grid-template-columns] duration-200 ease-out`}>
      <aside className="z-20 flex flex-col gap-5 border-b border-border bg-surface px-3 py-3 md:sticky md:top-0 md:z-10 md:h-screen md:gap-6 md:border-b-0 md:border-r md:px-3 md:py-5">
        <div className={`flex items-center gap-2.5 ${isSidebarCollapsed ? "md:justify-center md:px-0" : "justify-between px-2"}`}>
          <div className={`flex min-w-0 items-center gap-2.5 ${isSidebarCollapsed ? "md:justify-center" : ""}`}>
            <img src="/images/moongate_logo.png" alt="Moongate" className="h-9 w-9 shrink-0 object-contain" />
            <span className={`truncate text-[17px] font-bold tracking-tight text-fg ${isSidebarCollapsed ? "md:hidden" : ""}`}>
              Moongate
            </span>
          </div>
          <button
            type="button"
            onClick={() => setIsSidebarCollapsed((current) => !current)}
            aria-expanded={!isSidebarCollapsed}
            aria-controls="portal-side-nav"
            aria-label={isSidebarCollapsed ? "Expand sidebar" : "Collapse sidebar"}
            title={isSidebarCollapsed ? "Expand sidebar" : "Collapse sidebar"}
            className="hidden h-8 w-8 items-center justify-center rounded-md text-fg-subtle transition-colors duration-150 hover:bg-muted hover:text-fg md:inline-flex"
          >
            {isSidebarCollapsed ? <PanelLeftOpen size={17} aria-hidden /> : <PanelLeftClose size={17} aria-hidden />}
          </button>
        </div>

        <nav
          id="portal-side-nav"
          aria-label={`${section} navigation`}
          className="flex flex-row gap-1.5 overflow-x-auto pb-0.5 md:flex-col md:overflow-visible md:pb-0"
        >
          {items.map((item) => {
            const isActive = activeItemId === item.id;
            const collapsed = isSidebarCollapsed;

            return (
              <button
                key={item.id}
                type="button"
                onClick={() => onItemChange(item.id)}
                aria-label={item.label}
                aria-current={isActive ? "page" : undefined}
                title={collapsed ? item.label : undefined}
                className={[
                  "group relative flex min-h-[38px] shrink-0 items-center gap-2.5 rounded-md px-2.5 text-sm font-medium transition-[color,background-color,transform] duration-150 active:scale-[0.98]",
                  collapsed ? "md:justify-center md:px-0" : "",
                  isActive
                    ? "bg-muted text-fg before:absolute before:left-0 before:top-1/2 before:h-5 before:w-[3px] before:-translate-y-1/2 before:rounded-full before:bg-accent before:content-['']"
                    : "text-fg-muted hover:bg-muted hover:text-fg"
                ].join(" ")}
              >
                <item.icon size={18} aria-hidden className="shrink-0" />
                <span className={collapsed ? "md:hidden" : ""}>{item.label}</span>
              </button>
            );
          })}
        </nav>
      </aside>

      <main className="min-w-0">
        <header className="sticky top-0 z-10 flex min-h-[70px] items-center justify-between gap-4 border-b border-border bg-bg/85 px-5 backdrop-blur-md md:px-7">
          <div className="flex min-w-0 items-center gap-3">
            <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-md bg-accent/10 text-xs font-bold text-accent">
              {initialsOf(user.username)}
            </div>
            <div className="grid min-w-0 gap-0.5">
              <strong className="truncate text-sm font-semibold leading-tight text-fg">{user.username}</strong>
              <span className="truncate text-xs font-medium text-fg-muted">{user.level}</span>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <ThemeToggle theme={theme} onToggle={toggleTheme} />
            <button
              type="button"
              onClick={onLogout}
              aria-label="Logout"
              title="Logout"
              className="inline-flex h-9 w-9 items-center justify-center rounded-md border border-border bg-surface text-fg-muted transition-[color,background-color,transform] duration-150 hover:bg-muted hover:text-fg active:scale-[0.96]"
            >
              <LogOut size={18} aria-hidden />
            </button>
          </div>
        </header>
        {children}
      </main>
    </div>
  );
}

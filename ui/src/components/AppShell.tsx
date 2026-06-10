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
    ? "md:grid-cols-[68px_minmax(0,1fr)]"
    : "md:grid-cols-[238px_minmax(0,1fr)]";

  return (
    <div className={`grid min-h-screen grid-cols-1 ${columns} bg-bg text-fg transition-[grid-template-columns] duration-200 ease-out`}>
      <aside className="z-20 flex flex-col gap-3 border-b border-border bg-surface-raised px-2 py-2 md:sticky md:top-0 md:z-10 md:h-screen md:border-b-0 md:border-r md:px-2 md:py-3">
        <div className={`flex h-10 items-center gap-2 ${isSidebarCollapsed ? "md:justify-center md:px-0" : "justify-between px-1.5"}`}>
          <div className={`flex min-w-0 items-center gap-2.5 ${isSidebarCollapsed ? "md:justify-center" : ""}`}>
            <img src="/images/moongate_logo.png" alt="Moongate" className="h-7 w-7 shrink-0 object-contain" />
            <span className={`truncate text-sm font-semibold tracking-tight text-fg ${isSidebarCollapsed ? "md:hidden" : ""}`}>
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
            className="hidden h-7 w-7 items-center justify-center rounded-md text-fg-subtle transition-colors duration-150 hover:bg-muted hover:text-fg md:inline-flex"
          >
            {isSidebarCollapsed ? <PanelLeftOpen size={16} aria-hidden /> : <PanelLeftClose size={16} aria-hidden />}
          </button>
        </div>

        <nav
          id="portal-side-nav"
          aria-label={`${section} navigation`}
          className="flex flex-row gap-1 overflow-x-auto pb-0.5 md:flex-col md:overflow-visible md:pb-0"
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
                  "group relative flex min-h-[34px] shrink-0 items-center gap-2 rounded-md px-2 text-[13px] font-medium transition-colors duration-150",
                  collapsed ? "md:justify-center md:px-0" : "",
                  isActive
                    ? "bg-surface text-fg shadow-card"
                    : "text-fg-muted hover:bg-muted hover:text-fg"
                ].join(" ")}
              >
                <item.icon size={16} aria-hidden className="shrink-0" />
                <span className={collapsed ? "md:hidden" : ""}>{item.label}</span>
              </button>
            );
          })}
        </nav>
      </aside>

      <main className="min-w-0">
        <header className="sticky top-0 z-10 flex min-h-[52px] items-center justify-end gap-3 border-b border-border bg-bg/90 px-4 backdrop-blur-md md:px-6">
          <div className="flex min-w-0 items-center gap-3">
            <div className="flex h-7 w-7 shrink-0 items-center justify-center rounded-md bg-muted text-[11px] font-semibold text-fg-muted">
              {initialsOf(user.username)}
            </div>
            <div className="hidden min-w-0 gap-0.5 sm:grid">
              <strong className="truncate text-[13px] font-medium leading-tight text-fg">{user.username}</strong>
              <span className="truncate text-[11px] font-medium text-fg-subtle">{user.level}</span>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <ThemeToggle theme={theme} onToggle={toggleTheme} />
            <button
              type="button"
              onClick={onLogout}
              aria-label="Logout"
              title="Logout"
              className="inline-flex h-8 w-8 items-center justify-center rounded-md text-fg-muted transition-colors duration-150 hover:bg-muted hover:text-fg"
            >
              <LogOut size={16} aria-hidden />
            </button>
          </div>
        </header>
        {children}
      </main>
    </div>
  );
}

import { useState, type ReactNode } from "react";
import { Activity, LogOut, PanelLeftClose, PanelLeftOpen } from "lucide-react";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";
import { cn } from "@/lib/utils";
import { adminGroups, adminItems, playerGroups, playerItems } from "../data/navigation";
import { useTheme } from "../lib/useTheme";
import type { AdminNavId, AdminRuntimeSnapshot } from "../types/admin";
import type { AuthUser } from "../types/auth";
import { CommandPaletteButton } from "./CommandPalette";
import { ThemeToggle } from "./ThemeToggle";

type AppSection = "admin" | "player";
type PlayerNavId = (typeof playerItems)[number]["id"];

type AppShellProps = {
  user: AuthUser;
  section: AppSection;
  activeItemId: AdminNavId | PlayerNavId;
  runtimeSnapshot?: AdminRuntimeSnapshot | null;
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
  runtimeSnapshot = null,
  onItemChange,
  onLogout,
  children
}: AppShellProps) {
  const items = section === "admin" ? adminItems : playerItems;
  const groups = section === "admin" ? adminGroups : playerGroups;
  const [isSidebarCollapsed, setIsSidebarCollapsed] = useState(false);
  const { theme, toggleTheme } = useTheme();

  const columns = isSidebarCollapsed
    ? "md:grid-cols-[68px_minmax(0,1fr)]"
    : "md:grid-cols-[238px_minmax(0,1fr)]";
  const isAdmin = section === "admin";
  const isLive = runtimeSnapshot?.reachable ?? false;
  const version = runtimeSnapshot?.server?.version ?? "Unknown version";

  return (
    <div className={cn("grid min-h-screen grid-cols-1 bg-bg text-fg transition-[grid-template-columns] duration-200 ease-out", columns)}>
      <aside className="z-20 flex flex-col gap-3 border-b border-border bg-surface-raised px-2 py-2 md:sticky md:top-0 md:z-10 md:h-screen md:border-b-0 md:border-r md:px-2 md:py-3">
        <div className={cn("flex h-10 items-center gap-2", isSidebarCollapsed ? "md:justify-center md:px-0" : "justify-between px-1.5")}>
          <div className={cn("flex min-w-0 items-center gap-2.5", isSidebarCollapsed && "md:justify-center")}>
            <img src="/images/moongate_logo.png" alt="Moongate" className="h-7 w-7 shrink-0 object-contain" />
            <span className={cn("truncate text-sm font-semibold tracking-tight text-fg", isSidebarCollapsed && "md:hidden")}>
              Moongate
            </span>
          </div>
          <Tooltip>
            <TooltipTrigger asChild>
              <Button
                type="button"
                variant="ghost"
                size="icon-xs"
                onClick={() => setIsSidebarCollapsed((current) => !current)}
                aria-expanded={!isSidebarCollapsed}
                aria-controls="portal-side-nav"
                aria-label={isSidebarCollapsed ? "Expand sidebar" : "Collapse sidebar"}
                className="hidden text-fg-subtle hover:bg-muted hover:text-fg md:inline-flex"
              >
                {isSidebarCollapsed ? <PanelLeftOpen size={16} aria-hidden /> : <PanelLeftClose size={16} aria-hidden />}
              </Button>
            </TooltipTrigger>
            <TooltipContent side="right" sideOffset={8}>
              {isSidebarCollapsed ? "Expand sidebar" : "Collapse sidebar"}
            </TooltipContent>
          </Tooltip>
        </div>

        <nav
          id="portal-side-nav"
          aria-label={`${section} navigation`}
          className="flex flex-row gap-1 overflow-x-auto pb-0.5 md:flex-col md:gap-3 md:overflow-visible md:pb-0"
        >
          {groups.map((group) => {
            const groupItems = group.itemIds
              .map((itemId) => items.find((item) => item.id === itemId))
              .filter((item): item is (typeof items)[number] => Boolean(item));

            return (
              <div key={group.label} className="flex shrink-0 flex-row gap-1 md:flex-col md:gap-1">
                <div
                  className={cn(
                    "hidden px-2 pt-1 text-[10px] font-semibold uppercase text-fg-subtle md:block",
                    isSidebarCollapsed && "md:mx-auto md:h-px md:w-7 md:bg-border md:p-0 md:text-transparent"
                  )}
                  aria-hidden={isSidebarCollapsed}
                >
                  {group.label}
                </div>
                {groupItems.map((item) => {
                  const isActive = activeItemId === item.id;
                  const collapsed = isSidebarCollapsed;

                  return (
                    <Button
                      key={item.id}
                      type="button"
                      variant="ghost"
                      size="sm"
                      onClick={() => onItemChange(item.id)}
                      aria-label={item.label}
                      aria-current={isActive ? "page" : undefined}
                      title={collapsed ? item.label : undefined}
                      className={cn(
                        "group relative h-auto min-h-[34px] shrink-0 justify-start gap-2 px-2 text-[13px] font-medium",
                        collapsed && "md:justify-center md:px-0",
                        isActive
                          ? "bg-surface text-fg shadow-card"
                          : "text-fg-muted hover:bg-muted hover:text-fg"
                      )}
                    >
                      <item.icon size={16} aria-hidden className="shrink-0" />
                      <span className={cn(collapsed && "md:hidden")}>{item.label}</span>
                    </Button>
                  );
                })}
              </div>
            );
          })}
        </nav>
      </aside>

      <main className="min-w-0">
        <header className="sticky top-0 z-10 flex min-h-[52px] items-center justify-end gap-3 border-b border-border bg-bg/90 px-4 backdrop-blur-md md:px-6">
          {isAdmin && (
            <Badge
              variant="outline"
              className={cn(
                "mr-auto min-h-[30px] gap-2 rounded-md px-2.5 text-xs font-medium",
                isLive
                  ? "border-success/20 bg-success/10 text-success"
                  : "border-danger/20 bg-danger/10 text-danger"
              )}
            >
              <span className={`h-2 w-2 rounded-full ${isLive ? "bg-success" : "bg-danger"}`} aria-hidden />
              <Activity size={14} aria-hidden />
              {isLive ? "Live" : "Offline"}
            </Badge>
          )}
          <div className="flex min-w-0 items-center gap-3">
            <Avatar size="sm" className="rounded-md">
              <AvatarFallback className="rounded-md text-[11px] font-semibold">{initialsOf(user.username)}</AvatarFallback>
            </Avatar>
            <div className="hidden min-w-0 gap-0.5 sm:grid">
              <strong className="truncate text-[13px] font-medium leading-tight text-fg">{user.username}</strong>
              <span className="truncate text-[11px] font-medium text-fg-subtle">{user.level}</span>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <Tooltip>
              <TooltipTrigger asChild>
                <CommandPaletteButton />
              </TooltipTrigger>
              <TooltipContent side="bottom" sideOffset={8}>Command palette</TooltipContent>
            </Tooltip>
            <ThemeToggle theme={theme} onToggle={toggleTheme} />
            <Tooltip>
              <TooltipTrigger asChild>
                <Button
                  type="button"
                  variant="ghost"
                  size="icon-sm"
                  onClick={onLogout}
                  aria-label="Logout"
                  className="text-fg-muted hover:bg-muted hover:text-fg"
                >
                  <LogOut size={16} aria-hidden />
                </Button>
              </TooltipTrigger>
              <TooltipContent side="bottom" sideOffset={8}>Logout</TooltipContent>
            </Tooltip>
          </div>
        </header>
        {children}
        {isAdmin && (
          <footer className="border-t border-border px-4 py-3 text-[11px] text-fg-subtle md:px-6">
            <div className="flex flex-wrap items-center justify-center gap-2 text-center">
              <span>
                Built with love by{" "}
                <a className="font-medium text-fg underline-offset-4 hover:underline" href="https://github.com/tgiachi" target="_blank" rel="noreferrer">
                  squid
                </a>
              </span>
              <span aria-hidden>❤️</span>
              <span className="font-mono">{version}</span>
            </div>
          </footer>
        )}
      </main>
    </div>
  );
}

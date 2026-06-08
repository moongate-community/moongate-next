import { useState, type ReactNode } from "react";
import { LogOut, PanelLeftClose, PanelLeftOpen } from "lucide-react";
import { adminItems, playerItems } from "../data/navigation";
import type { AdminNavId } from "../types/admin";
import type { AuthUser } from "../types/auth";

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

  return (
    <div className={`shell ${isSidebarCollapsed ? "shell-collapsed" : ""}`}>
      <aside className="sidebar">
        <div className="brand">
          <div className="brand-identity">
            <img src="/images/moongate_logo.png" alt="Moongate" className="sidebar-logo" />
            <span className="brand-name">Moongate</span>
          </div>
          <button
            className="sidebar-toggle"
            type="button"
            onClick={() => setIsSidebarCollapsed((current) => !current)}
            aria-expanded={!isSidebarCollapsed}
            aria-controls="portal-side-nav"
            aria-label={isSidebarCollapsed ? "Expand sidebar" : "Collapse sidebar"}
            title={isSidebarCollapsed ? "Expand sidebar" : "Collapse sidebar"}
          >
            {isSidebarCollapsed ? <PanelLeftOpen size={17} aria-hidden /> : <PanelLeftClose size={17} aria-hidden />}
          </button>
        </div>
        <nav id="portal-side-nav" className="side-nav" aria-label={`${section} navigation`}>
          {items.map((item) => (
            <button
              key={item.id}
              className={activeItemId === item.id ? "active" : ""}
              onClick={() => onItemChange(item.id)}
              aria-label={item.label}
              title={isSidebarCollapsed ? item.label : undefined}
            >
              <item.icon size={18} aria-hidden />
              <span className="side-nav-label">{item.label}</span>
            </button>
          ))}
        </nav>
      </aside>
      <main className="content">
        <header className="topbar">
          <div className="user-summary">
            <strong>{user.username}</strong>
            <span>{user.level}</span>
          </div>
          <button onClick={onLogout} className="icon-button" aria-label="Logout">
            <LogOut size={18} aria-hidden />
          </button>
        </header>
        {children}
      </main>
    </div>
  );
}

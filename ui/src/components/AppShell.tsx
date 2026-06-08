import type { ReactNode } from "react";
import { LogOut } from "lucide-react";
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
  onSectionChange: (section: AppSection) => void;
  onLogout: () => Promise<void>;
  children: ReactNode;
};

export function AppShell({
  user,
  section,
  activeItemId,
  onItemChange,
  onSectionChange,
  onLogout,
  children
}: AppShellProps) {
  const items = section === "admin" ? adminItems : playerItems;

  return (
    <div className="shell">
      <aside className="sidebar">
        <div className="brand">
          <img src="/images/moongate_logo.png" alt="Moongate" className="sidebar-logo" />
          <span>Moongate</span>
        </div>
        <nav className="section-tabs" aria-label="Main sections">
          <button className={section === "admin" ? "active" : ""} onClick={() => onSectionChange("admin")}>
            Admin
          </button>
          <button className={section === "player" ? "active" : ""} onClick={() => onSectionChange("player")}>
            Player
          </button>
        </nav>
        <nav className="side-nav" aria-label={`${section} navigation`}>
          {items.map((item) => (
            <button key={item.id} className={activeItemId === item.id ? "active" : ""} onClick={() => onItemChange(item.id)}>
              <item.icon size={18} aria-hidden />
              {item.label}
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

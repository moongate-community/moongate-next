import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import type { Action } from "kbar";
import { Bot, Box, LogOut, UserRound } from "lucide-react";
import { CommandPalette } from "./components/CommandPalette";
import { TooltipProvider } from "@/components/ui/tooltip";
import { AppShell } from "./components/AppShell";
import { LoginView } from "./components/LoginView";
import { adminGroups, adminItems, playerGroups, playerItems } from "./data/navigation";
import { listItemTemplates } from "./lib/adminItemTemplatesClient";
import { listMobileTemplates } from "./lib/adminMobileTemplatesClient";
import { listUsers } from "./lib/adminUsersClient";
import { login, logout } from "./lib/authClient";
import { clearStoredAuth, readStoredAuth, writeStoredAuth } from "./lib/authStorage";
import { AdminDashboard } from "./pages/AdminDashboard";
import { PlayerDashboard } from "./pages/PlayerDashboard";
import type { AdminNavId, AdminRuntimeSnapshot } from "./types/admin";
import type { AdminCommandTarget } from "./types/adminCommandTarget";
import type { AuthTokenResponse } from "./types/auth";
import type { ItemTemplateFilters } from "./types/itemTemplates";
import type { MobileTemplateFilters } from "./types/mobileTemplates";
import type { AdminUser } from "./types/users";

type AppSection = "admin" | "player";
type PlayerNavId = "profile" | "adventures";

type AdminRouteTarget =
  | { kind: "itemTemplate"; id: string }
  | { kind: "mobileTemplate"; id: string }
  | null;

type AdminRoute = {
  view: AdminNavId;
  target: AdminRouteTarget;
};

const COMMAND_SEARCH_PAGE_SIZE = 8;
const adminNavIds = new Set<string>(adminItems.map((item) => item.id));

function isAdminNavId(value: string | null): value is AdminNavId {
  return Boolean(value && adminNavIds.has(value));
}

function itemTemplateCommandFilters(search: string): ItemTemplateFilters {
  return {
    page: 1,
    pageSize: COMMAND_SEARCH_PAGE_SIZE,
    search,
    tag: "",
    rarity: "",
    layer: "",
    abstract: "all"
  };
}

function mobileTemplateCommandFilters(search: string): MobileTemplateFilters {
  return {
    page: 1,
    pageSize: COMMAND_SEARCH_PAGE_SIZE,
    search,
    tag: "",
    notoriety: "",
    abstract: "all"
  };
}

function commandKeywords(values: Array<string | null | undefined>): string {
  return values.filter((value): value is string => Boolean(value && value.length > 0)).join(" ");
}

function sectionFromPath(): AppSection {
  return window.location.pathname.startsWith("/admin") ? "admin" : "player";
}

function readAdminRoute(): AdminRoute {
  const params = new URLSearchParams(window.location.search);
  const itemTemplateId = params.get("itemTemplate");
  const mobileTemplateId = params.get("mobileTemplate");
  const view = params.get("view");

  if (itemTemplateId) {
    return {
      view: "itemTemplates",
      target: { kind: "itemTemplate", id: itemTemplateId }
    };
  }

  if (mobileTemplateId) {
    return {
      view: "mobileTemplates",
      target: { kind: "mobileTemplate", id: mobileTemplateId }
    };
  }

  return {
    view: isAdminNavId(view) ? view : "overview",
    target: null
  };
}

function adminUrl(view: AdminNavId, target?: AdminRouteTarget): string {
  const params = new URLSearchParams({ view });

  if (target?.kind === "itemTemplate") {
    params.set("itemTemplate", target.id);
  }

  if (target?.kind === "mobileTemplate") {
    params.set("mobileTemplate", target.id);
  }

  return `/admin?${params.toString()}`;
}

function targetFromRoute(route: AdminRoute, sequence: number): AdminCommandTarget | null {
  if (route.target?.kind === "itemTemplate") {
    return {
      kind: "itemTemplate",
      id: route.target.id,
      sequence
    };
  }

  if (route.target?.kind === "mobileTemplate") {
    return {
      kind: "mobileTemplate",
      id: route.target.id,
      sequence
    };
  }

  return null;
}

export default function App() {
  const initialAdminRoute = useRef<AdminRoute | null>(null);

  if (initialAdminRoute.current === null) {
    initialAdminRoute.current = readAdminRoute();
  }

  const [session, setSession] = useState<AuthTokenResponse | null>(() => readStoredAuth());
  const [section, setSection] = useState<AppSection>(() => sectionFromPath());
  const [adminNav, setAdminNav] = useState<AdminNavId>(() => initialAdminRoute.current?.view ?? "overview");
  const [playerNav, setPlayerNav] = useState<PlayerNavId>("profile");
  const [adminRuntimeSnapshot, setAdminRuntimeSnapshot] = useState<AdminRuntimeSnapshot | null>(null);
  const [adminCommandTarget, setAdminCommandTarget] = useState<AdminCommandTarget | null>(() =>
    targetFromRoute(initialAdminRoute.current ?? { view: "overview", target: null }, 1)
  );
  const [commandSearch, setCommandSearch] = useState("");
  const [commandSearchActions, setCommandSearchActions] = useState<Action[]>([]);
  const adminCommandTargetSequence = useRef(adminCommandTarget ? 1 : 0);

  async function handleLogin(nextSection: AppSection, username: string, password: string) {
    const next = await login(username, password);
    writeStoredAuth(next);
    setSession(next);
    setSection(nextSection);
    setAdminRuntimeSnapshot(null);

    if (nextSection === "admin") {
      const route = readAdminRoute();
      adminCommandTargetSequence.current += 1;
      setAdminNav(route.view);
      setAdminCommandTarget(targetFromRoute(route, adminCommandTargetSequence.current));

      return;
    }

    setAdminCommandTarget(null);
  }

  async function handleLogout() {
    if (session) {
      await logout(session.refreshToken);
    }

    clearStoredAuth();
    setSession(null);
    setAdminRuntimeSnapshot(null);
    setAdminCommandTarget(null);
  }

  const showAdminView = useCallback((itemId: AdminNavId) => {
    window.history.replaceState(null, "", adminUrl(itemId));
    setSection("admin");
    setAdminNav(itemId);
    setAdminCommandTarget(null);
  }, []);

  const openItemTemplateFromCommand = useCallback((id: string) => {
    adminCommandTargetSequence.current += 1;
    window.history.replaceState(null, "", adminUrl("itemTemplates", { kind: "itemTemplate", id }));
    setSection("admin");
    setAdminNav("itemTemplates");
    setAdminCommandTarget({
      kind: "itemTemplate",
      id,
      sequence: adminCommandTargetSequence.current
    });
  }, []);

  const openMobileTemplateFromCommand = useCallback((id: string) => {
    adminCommandTargetSequence.current += 1;
    window.history.replaceState(null, "", adminUrl("mobileTemplates", { kind: "mobileTemplate", id }));
    setSection("admin");
    setAdminNav("mobileTemplates");
    setAdminCommandTarget({
      kind: "mobileTemplate",
      id,
      sequence: adminCommandTargetSequence.current
    });
  }, []);

  const openUserFromCommand = useCallback((user: AdminUser) => {
    adminCommandTargetSequence.current += 1;
    window.history.replaceState(null, "", `/admin?view=users&user=${encodeURIComponent(user.id)}`);
    setSection("admin");
    setAdminNav("users");
    setAdminCommandTarget({
      kind: "user",
      user,
      sequence: adminCommandTargetSequence.current
    });
  }, []);

  useEffect(() => {
    if (!session || session.user.level === "Player") {
      setCommandSearchActions([]);

      return;
    }

    const search = commandSearch.trim();
    const accessToken = session.accessToken;

    if (search.length < 2) {
      setCommandSearchActions([]);

      return;
    }

    let cancelled = false;
    const timer = window.setTimeout(() => {
      async function loadCommandResults() {
        const [itemTemplatesResult, mobileTemplatesResult, usersResult] = await Promise.allSettled([
          listItemTemplates(accessToken, itemTemplateCommandFilters(search)),
          listMobileTemplates(accessToken, mobileTemplateCommandFilters(search)),
          listUsers(accessToken, 1, COMMAND_SEARCH_PAGE_SIZE, search)
        ]);

        if (cancelled) {
          return;
        }

        const nextActions: Action[] = [];

        if (itemTemplatesResult.status === "fulfilled") {
          itemTemplatesResult.value.items.forEach((template) => {
            nextActions.push({
              id: `search:item-template:${template.id}`,
              name: template.name || template.id,
              subtitle: `Item Template - ${template.id} - ${template.itemIdHex}`,
              keywords: commandKeywords([
                template.id,
                template.name,
                template.itemIdHex,
                template.rarity,
                template.layer,
                ...template.tags
              ]),
              section: { name: "Item Templates", priority: 30 },
              icon: <Box size={16} aria-hidden />,
              priority: 30,
              perform: () => openItemTemplateFromCommand(template.id)
            });
          });
        }

        if (mobileTemplatesResult.status === "fulfilled") {
          mobileTemplatesResult.value.items.forEach((template) => {
            nextActions.push({
              id: `search:mobile-template:${template.id}`,
              name: template.name || template.id,
              subtitle: `Mobile Template - ${template.id} - ${template.bodyHex}`,
              keywords: commandKeywords([
                template.id,
                template.name,
                template.title,
                template.bodyHex,
                template.gender,
                template.notoriety,
                template.brain,
                ...template.tags
              ]),
              section: { name: "Mobile Templates", priority: 29 },
              icon: <Bot size={16} aria-hidden />,
              priority: 29,
              perform: () => openMobileTemplateFromCommand(template.id)
            });
          });
        }

        if (usersResult.status === "fulfilled") {
          usersResult.value.items.forEach((user) => {
            nextActions.push({
              id: `search:user:${user.id}`,
              name: user.username,
              subtitle: `${user.email} - ${user.level}${user.isActive ? "" : " - Locked"}`,
              keywords: commandKeywords([user.id, user.username, user.email, user.level, user.isActive ? "active" : "locked"]),
              section: { name: "Users", priority: 28 },
              icon: <UserRound size={16} aria-hidden />,
              priority: 28,
              perform: () => openUserFromCommand(user)
            });
          });
        }

        setCommandSearchActions(nextActions);
      }

      void loadCommandResults();
    }, 180);

    return () => {
      cancelled = true;
      window.clearTimeout(timer);
    };
  }, [commandSearch, openItemTemplateFromCommand, openMobileTemplateFromCommand, openUserFromCommand, session]);

  const commandActions = useMemo<Action[]>(() => {
    if (!session) {
      return [];
    }

    const actions: Action[] = [];
    const canUseAdmin = session.user.level !== "Player";
    const adminSections = new Map(adminGroups.flatMap((group) => group.itemIds.map((itemId) => [itemId, group.label])));
    const playerSections = new Map(playerGroups.flatMap((group) => group.itemIds.map((itemId) => [itemId, group.label])));

    if (canUseAdmin) {
      adminItems.forEach((item) => {
        actions.push({
          id: `admin:${item.id}`,
          name: item.label,
          subtitle: "Admin console",
          keywords: `admin ${item.label}`,
          section: adminSections.get(item.id) ?? "Admin",
          icon: <item.icon size={16} aria-hidden />,
          perform: () => {
            showAdminView(item.id);
          }
        });
      });
    }

    playerItems.forEach((item) => {
      actions.push({
        id: `player:${item.id}`,
        name: item.label,
        subtitle: "Player portal",
        keywords: `player ${item.label}`,
        section: playerSections.get(item.id) ?? "Portal",
        icon: <item.icon size={16} aria-hidden />,
        perform: () => {
          window.history.replaceState(null, "", "/");
          setSection("player");
          setPlayerNav(item.id);
          setAdminCommandTarget(null);
        }
      });
    });

    actions.push({
      id: "session:logout",
      name: "Logout",
      subtitle: session.user.username,
      keywords: "sign out session",
      section: "Session",
      icon: <LogOut size={16} aria-hidden />,
      perform: () => {
        void handleLogout();
      }
    });

    return [...actions, ...commandSearchActions];
  }, [commandSearchActions, session, showAdminView]);

  if (!session) {
    return <LoginView section={sectionFromPath()} onLogin={handleLogin} />;
  }

  const activeItemId = section === "admin" ? adminNav : playerNav;

  return (
    <TooltipProvider delayDuration={200}>
      <CommandPalette actions={commandActions} onSearchChange={setCommandSearch}>
        <AppShell
          user={session.user}
          section={section}
          activeItemId={activeItemId}
          runtimeSnapshot={section === "admin" ? adminRuntimeSnapshot : null}
          onItemChange={(itemId) => {
            if (section === "admin") {
              const nextAdminNav = itemId as AdminNavId;
              window.history.replaceState(null, "", adminUrl(nextAdminNav));
              setAdminNav(nextAdminNav);
              setAdminCommandTarget(null);

              return;
            }

            window.history.replaceState(null, "", "/");
            setPlayerNav(itemId as PlayerNavId);
            setAdminCommandTarget(null);
          }}
          onLogout={handleLogout}
        >
          {section === "admin" ? (
            <AdminDashboard
              activeView={adminNav}
              accessToken={session.accessToken}
              accessTokenExpiresAt={session.accessTokenExpiresAt}
              user={session.user}
              commandTarget={adminCommandTarget}
              onRuntimeSnapshotChange={setAdminRuntimeSnapshot}
            />
          ) : (
            <PlayerDashboard user={session.user} />
          )}
        </AppShell>
      </CommandPalette>
    </TooltipProvider>
  );
}

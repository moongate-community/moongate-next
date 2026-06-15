import { useEffect, useMemo, useState, type ReactNode } from "react";
import type { Action } from "kbar";
import { Bot, Box, LogOut, UserRound } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { CommandPalette } from "./CommandPalette";
import { useSession } from "../lib/SessionContext";
import { adminGroups, adminItems, adminPathFor, playerGroups, playerItems, playerPathFor } from "../data/navigation";
import { listItemTemplates } from "../lib/adminItemTemplatesClient";
import { listMobileTemplates } from "../lib/adminMobileTemplatesClient";
import { listUsers } from "../lib/adminUsersClient";
import type { ItemTemplateFilters } from "../types/itemTemplates";
import type { MobileTemplateFilters } from "../types/mobileTemplates";

const COMMAND_SEARCH_PAGE_SIZE = 8;

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

/** Wraps the app in the command palette, building session-aware actions that navigate via react-router. */
export function CommandPaletteHost({ children }: { children: ReactNode }) {
  const { session, signOut } = useSession();
  const navigate = useNavigate();
  const [commandSearch, setCommandSearch] = useState("");
  const [commandSearchActions, setCommandSearchActions] = useState<Action[]>([]);

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
              perform: () => navigate(adminPathFor("itemTemplates", template.id))
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
              perform: () => navigate(adminPathFor("mobileTemplates", template.id))
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
              perform: () => navigate(adminPathFor("users"), { state: { openUser: user } })
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
  }, [commandSearch, navigate, session]);

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
          perform: () => navigate(adminPathFor(item.id))
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
        perform: () => navigate(playerPathFor(item.id))
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
        void signOut().then(() => navigate("/login", { replace: true }));
      }
    });

    return [...actions, ...commandSearchActions];
  }, [commandSearchActions, navigate, session, signOut]);

  return (
    <CommandPalette actions={commandActions} onSearchChange={setCommandSearch}>
      {children}
    </CommandPalette>
  );
}

import {
  Activity,
  Bot,
  ChartSpline,
  KeyRound,
  PackageOpen,
  PackageSearch,
  Plug,
  ScrollText,
  Sparkles,
  TerminalSquare,
  Users,
  UserRound
} from "lucide-react";
import type { AdminNavId } from "../types/admin";

export const adminItems = [
  {
    id: "overview",
    label: "Overview",
    icon: Activity
  },
  {
    id: "metrics",
    label: "Metrics",
    icon: ChartSpline
  },
  {
    id: "persistence",
    label: "Persistence",
    icon: ScrollText
  },
  {
    id: "plugins",
    label: "Plugins",
    icon: Plug
  },
  {
    id: "security",
    label: "Security",
    icon: KeyRound
  },
  {
    id: "users",
    label: "Users",
    icon: Users
  },
  {
    id: "itemTemplates",
    label: "Item Templates",
    icon: PackageSearch
  },
  {
    id: "mobileTemplates",
    label: "Mobile Templates",
    icon: Bot
  },
  {
    id: "lootTemplates",
    label: "Loot Templates",
    icon: PackageOpen
  },
  {
    id: "console",
    label: "Console",
    icon: TerminalSquare
  }
] as const;

export const adminGroups = [
  {
    label: "Overview",
    itemIds: ["overview", "metrics"]
  },
  {
    label: "Operations",
    itemIds: ["persistence", "plugins", "console"]
  },
  {
    label: "World data",
    itemIds: ["itemTemplates", "mobileTemplates", "lootTemplates"]
  },
  {
    label: "Access",
    itemIds: ["users", "security"]
  }
] as const;

export const playerItems = [
  {
    id: "profile",
    label: "Profile",
    icon: UserRound
  },
  {
    id: "adventures",
    label: "Adventures",
    icon: Sparkles
  }
] as const;

export const playerGroups = [
  {
    label: "Portal",
    itemIds: ["profile", "adventures"]
  }
] as const;

export type PlayerNavId = (typeof playerItems)[number]["id"];

/** Path segment for each admin nav id (under /admin). */
export const adminPathSegments: Record<AdminNavId, string> = {
  overview: "overview",
  metrics: "metrics",
  persistence: "persistence",
  plugins: "plugins",
  security: "security",
  users: "users",
  itemTemplates: "item-templates",
  mobileTemplates: "mobile-templates",
  lootTemplates: "loot-templates",
  console: "console"
};

const adminNavIdBySegment = new Map<string, AdminNavId>(
  (Object.entries(adminPathSegments) as Array<[AdminNavId, string]>).map(([id, segment]) => [segment, id])
);

/** Absolute path for an admin nav id (optionally a template detail id). */
export function adminPathFor(navId: AdminNavId, detailId?: string): string {
  const base = `/admin/${adminPathSegments[navId]}`;

  return detailId ? `${base}/${encodeURIComponent(detailId)}` : base;
}

/** Absolute path for a player nav id. */
export function playerPathFor(navId: PlayerNavId): string {
  return `/player/${navId}`;
}

/** Parses an /admin pathname into its nav id (default "overview") and optional detail id. */
export function parseAdminLocation(pathname: string): { view: AdminNavId; detailId: string | null } {
  const rest = pathname.replace(/^\/admin\/?/, "");
  const [segment, rawDetail] = rest.split("/");
  const view = adminNavIdBySegment.get(segment) ?? "overview";
  const detailId = rawDetail ? decodeURIComponent(rawDetail) : null;

  return { view, detailId };
}

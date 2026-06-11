import {
  Activity,
  Bot,
  ChartSpline,
  Gauge,
  KeyRound,
  PackageSearch,
  Plug,
  ScrollText,
  Sparkles,
  TerminalSquare,
  Users,
  UserRound
} from "lucide-react";

export const adminItems = [
  {
    id: "overview",
    label: "Overview",
    icon: Activity
  },
  {
    id: "runtime",
    label: "Runtime",
    icon: Gauge
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
    id: "console",
    label: "Console",
    icon: TerminalSquare
  }
] as const;

export const adminGroups = [
  {
    label: "Overview",
    itemIds: ["overview", "runtime", "metrics"]
  },
  {
    label: "Operations",
    itemIds: ["persistence", "plugins", "console"]
  },
  {
    label: "World data",
    itemIds: ["itemTemplates", "mobileTemplates"]
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

import {
  Activity,
  Bot,
  ChartSpline,
  Gauge,
  KeyRound,
  PackageSearch,
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

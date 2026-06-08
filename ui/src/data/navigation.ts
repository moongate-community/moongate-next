import { Activity, ChartSpline, Gauge, KeyRound, ScrollText, Sparkles, Users, UserRound } from "lucide-react";

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

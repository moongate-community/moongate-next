import { Activity, Gauge, HeartPulse, KeyRound, ScrollText, Sparkles, UserRound } from "lucide-react";

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
    id: "diagnostics",
    label: "Diagnostics",
    icon: HeartPulse
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

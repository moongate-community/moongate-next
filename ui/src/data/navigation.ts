import { Activity, Map, Shield, Sparkles, UserRound } from "lucide-react";

export const adminItems = [
  {
    id: "overview",
    label: "Overview",
    icon: Activity
  },
  {
    id: "world",
    label: "World",
    icon: Map
  },
  {
    id: "security",
    label: "Security",
    icon: Shield
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

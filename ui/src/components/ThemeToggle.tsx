import { Moon, Sun } from "lucide-react";
import { Button } from "@/components/ui/button";
import type { Theme } from "../lib/useTheme";

type ThemeToggleProps = {
  theme: Theme;
  onToggle: () => void;
};

export function ThemeToggle({ theme, onToggle }: ThemeToggleProps) {
  const isDark = theme === "dark";

  return (
    <Button
      type="button"
      variant="ghost"
      size="icon-sm"
      onClick={onToggle}
      aria-label={isDark ? "Switch to light theme" : "Switch to dark theme"}
      title={isDark ? "Light theme" : "Dark theme"}
      className="text-fg-muted hover:bg-muted hover:text-fg"
    >
      {isDark ? <Sun size={16} aria-hidden /> : <Moon size={16} aria-hidden />}
    </Button>
  );
}

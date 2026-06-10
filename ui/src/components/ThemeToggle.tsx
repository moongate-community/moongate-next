import { Moon, Sun } from "lucide-react";
import type { Theme } from "../lib/useTheme";

type ThemeToggleProps = {
  theme: Theme;
  onToggle: () => void;
};

export function ThemeToggle({ theme, onToggle }: ThemeToggleProps) {
  const isDark = theme === "dark";

  return (
    <button
      type="button"
      onClick={onToggle}
      aria-label={isDark ? "Switch to light theme" : "Switch to dark theme"}
      title={isDark ? "Light theme" : "Dark theme"}
      className="inline-flex h-8 w-8 items-center justify-center rounded-md text-fg-muted transition-colors duration-150 hover:bg-muted hover:text-fg"
    >
      {isDark ? <Sun size={16} aria-hidden /> : <Moon size={16} aria-hidden />}
    </button>
  );
}

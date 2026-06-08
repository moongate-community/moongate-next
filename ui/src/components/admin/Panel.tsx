import type { LucideIcon } from "lucide-react";
import type { ReactNode } from "react";

type PanelProps = {
  title: string;
  icon?: LucideIcon;
  /** Optional content rendered at the right edge of the header (status, badge). */
  action?: ReactNode;
  children: ReactNode;
  className?: string;
};

/** Shared surface card used by every admin panel for a consistent look. */
export function Panel({ title, icon: Icon, action, children, className }: PanelProps) {
  return (
    <article className={`animate-rise rounded-lg border border-border bg-surface p-5 shadow-card ${className ?? ""}`}>
      <header className="mb-4 flex items-center justify-between gap-3">
        <div className="flex items-center gap-2.5">
          {Icon && <Icon size={18} aria-hidden className="text-fg-subtle" />}
          <h3 className="text-base font-bold tracking-tight text-fg">{title}</h3>
        </div>
        {action}
      </header>
      {children}
    </article>
  );
}

import type { LucideIcon } from "lucide-react";
import type { ReactNode } from "react";
import { Card, CardAction, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { cn } from "@/lib/utils";

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
    <Card className={cn("animate-rise gap-0 rounded-md border-border bg-surface py-0 shadow-none", className)}>
      <CardHeader className="min-h-[48px] border-b border-border px-4 py-0">
        <div className="flex items-center gap-2.5">
          {Icon && <Icon size={16} aria-hidden className="text-fg-subtle" />}
          <CardTitle className="text-sm tracking-tight text-fg">{title}</CardTitle>
        </div>
        {action ? <CardAction>{action}</CardAction> : null}
      </CardHeader>
      <CardContent className="p-4">{children}</CardContent>
    </Card>
  );
}

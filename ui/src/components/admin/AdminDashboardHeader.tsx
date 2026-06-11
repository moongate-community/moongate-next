import { Activity, RefreshCw } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";
import { cn } from "@/lib/utils";
import type { AdminRuntimeSnapshot } from "../../types/admin";

type AdminDashboardHeaderProps = {
  snapshot: AdminRuntimeSnapshot;
  loading: boolean;
  onRefresh: () => void;
};

export function AdminDashboardHeader({ snapshot, loading, onRefresh }: AdminDashboardHeaderProps) {
  const isLive = snapshot.reachable;

  return (
    <header className="flex flex-col gap-3 border-b border-border pb-4 sm:flex-row sm:items-center sm:justify-between">
      <div className="grid gap-1">
        <h2 className="text-[22px] font-semibold leading-tight tracking-tight text-fg">Admin dashboard</h2>
        <p className="m-0 text-[13px] text-fg-muted">{snapshot.server?.version ?? "Unknown version"}</p>
      </div>
      <div className="flex items-center gap-2">
        <Badge
          variant="outline"
          className={cn(
            "min-h-[30px] gap-2 rounded-md px-2.5 text-xs font-medium",
            isLive
              ? "border-success/20 bg-success/10 text-success"
              : "border-danger/20 bg-danger/10 text-danger"
          )}
        >
          <span className={`h-2 w-2 rounded-full ${isLive ? "bg-success" : "bg-danger"}`} aria-hidden />
          <Activity size={14} aria-hidden />
          {isLive ? "Live" : "Offline"}
        </Badge>
        <Tooltip>
          <TooltipTrigger asChild>
            <Button
              type="button"
              variant="ghost"
              size="icon-sm"
              onClick={onRefresh}
              disabled={loading}
              aria-label="Refresh dashboard"
              className="text-fg-muted hover:bg-muted hover:text-fg"
            >
              <RefreshCw size={16} aria-hidden className={loading ? "animate-spin" : ""} />
            </Button>
          </TooltipTrigger>
          <TooltipContent side="bottom" sideOffset={8}>Refresh dashboard</TooltipContent>
        </Tooltip>
      </div>
    </header>
  );
}

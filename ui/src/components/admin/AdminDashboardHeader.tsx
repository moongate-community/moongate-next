import { RefreshCw } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";

type AdminDashboardHeaderProps = {
  loading: boolean;
  onRefresh: () => void;
};

export function AdminDashboardHeader({ loading, onRefresh }: AdminDashboardHeaderProps) {
  return (
    <header className="flex flex-col gap-3 border-b border-border pb-4 sm:flex-row sm:items-center sm:justify-between">
      <div className="grid gap-1">
        <h2 className="text-[22px] font-semibold leading-tight tracking-tight text-fg">Admin dashboard</h2>
      </div>
      <div className="flex items-center gap-2">
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

import type { ReactNode } from "react";
import {
  KBarAnimator,
  KBarPortal,
  KBarPositioner,
  KBarProvider,
  KBarResults,
  KBarSearch,
  useKBar,
  useMatches,
  useRegisterActions,
  type Action
} from "kbar";
import { Search } from "lucide-react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

type CommandPaletteProps = {
  actions: Action[];
  children: ReactNode;
};

export function CommandPalette({ actions, children }: CommandPaletteProps) {
  return (
    <KBarProvider
      options={{
        animations: { enterMs: 120, exitMs: 80 },
        enableHistory: true
      }}
    >
      <CommandPaletteActions actions={actions} />
      <KBarPortal>
        <KBarPositioner className="z-50 bg-black/10 backdrop-blur-xs">
          <KBarAnimator className="w-full max-w-[640px] overflow-hidden rounded-none bg-popover text-popover-foreground shadow-raised ring-1 ring-foreground/10">
            <KBarSearch
              defaultPlaceholder="Search commands"
              className="h-12 w-full border-0 border-b border-border bg-transparent px-4 text-sm text-fg outline-none placeholder:text-fg-subtle"
            />
            <CommandPaletteResults />
          </KBarAnimator>
        </KBarPositioner>
      </KBarPortal>
      {children}
    </KBarProvider>
  );
}

export function CommandPaletteButton() {
  const { query } = useKBar();

  return (
    <Button
      type="button"
      variant="ghost"
      size="icon-sm"
      onClick={query.toggle}
      aria-label="Open command palette"
      className="text-fg-muted hover:bg-muted hover:text-fg"
    >
      <Search size={16} aria-hidden />
    </Button>
  );
}

function CommandPaletteActions({ actions }: { actions: Action[] }) {
  useRegisterActions(actions, [actions]);

  return null;
}

function CommandPaletteResults() {
  const { results } = useMatches();

  return (
    <div className="py-2">
      <KBarResults
        items={results}
        maxHeight={280}
        onRender={({ item, active }) => {
          if (typeof item === "string") {
            return (
              <div className="px-3 pb-1.5 pt-3 text-[10px] font-semibold uppercase text-fg-subtle first:pt-1">
                {item}
              </div>
            );
          }

          return (
            <div
              className={cn(
                "mx-2 flex min-h-10 cursor-default items-center gap-3 rounded-none px-2.5 py-2 text-[13px]",
                active ? "bg-muted text-fg" : "text-fg-muted"
              )}
            >
              {item.icon && (
                <span className="flex size-5 shrink-0 items-center justify-center text-fg-subtle" aria-hidden>
                  {item.icon}
                </span>
              )}
              <span className="grid min-w-0 gap-0.5">
                <span className="truncate font-medium text-fg">{item.name}</span>
                {item.subtitle && <span className="truncate text-[11px] text-fg-subtle">{item.subtitle}</span>}
              </span>
            </div>
          );
        }}
      />
    </div>
  );
}

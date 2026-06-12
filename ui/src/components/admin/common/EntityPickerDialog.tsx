import { useCallback, useEffect, useRef, useState, type ReactNode } from "react";
import { Search } from "lucide-react";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";
import { usePagedSearch } from "../../../lib/usePagedSearch";
import type { PagedResult } from "../../../types/itemTemplates";

type EntityPickerDialogProps<T> = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSelect: (item: T) => void;
  loadPage: (page: number, search: string) => Promise<PagedResult<T>>;
  getKey: (item: T) => string;
  getImageUrl: (item: T) => string;
  getLabel: (item: T) => string;
  renderTooltip?: (item: T) => ReactNode;
  filters?: ReactNode;
  title?: string;
  searchPlaceholder?: string;
  emptyText?: string;
};

/** Reusable modal that browses a paged entity list as an image grid and returns the chosen item. */
export function EntityPickerDialog<T>({
  open,
  onOpenChange,
  onSelect,
  loadPage,
  getKey,
  getImageUrl,
  getLabel,
  renderTooltip,
  filters,
  title = "Select",
  searchPlaceholder = "Search…",
  emptyText = "No results found."
}: EntityPickerDialogProps<T>) {
  const [query, setQuery] = useState("");
  const [search, setSearch] = useState("");

  useEffect(() => {
    const handle = window.setTimeout(() => setSearch(query), 250);

    return () => window.clearTimeout(handle);
  }, [query]);

  useEffect(() => {
    if (open) {
      setQuery("");
      setSearch("");
    }
  }, [open]);

  const load = useCallback((page: number) => loadPage(page, search), [loadPage, search]);
  const { items, loading, error, hasMore, loadMore, reset } = usePagedSearch(load);

  const sentinelRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    if (!open || !hasMore) {
      return;
    }

    const node = sentinelRef.current;

    if (!node) {
      return;
    }

    const observer = new IntersectionObserver((entries) => {
      if (entries[0]?.isIntersecting) {
        loadMore();
      }
    });

    observer.observe(node);

    return () => observer.disconnect();
  }, [open, hasMore, loadMore, items.length]);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="flex h-[80vh] w-[60vw] max-w-[60vw] flex-col gap-3 bg-surface sm:max-w-[60vw]">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
        </DialogHeader>

        <div className={filters ? "grid gap-2 md:grid-cols-[minmax(220px,1fr)_auto]" : undefined}>
          <div className="relative">
            <Search size={15} aria-hidden className="pointer-events-none absolute left-2.5 top-1/2 -translate-y-1/2 text-fg-subtle" />
            <Input
              value={query}
              onChange={(event) => setQuery(event.target.value)}
              placeholder={searchPlaceholder}
              aria-label="Search"
              className="h-9 bg-bg pl-8 text-[13px]"
            />
          </div>
          {filters}
        </div>

        <div className="min-h-0 flex-1 overflow-y-auto">
          {error ? (
            <div className="grid place-items-center gap-2 py-10 text-center">
              <p className="m-0 text-[13px] font-semibold text-danger">{error}</p>
              <button
                type="button"
                onClick={reset}
                className="rounded-md border border-border px-3 py-1 text-[13px] text-fg hover:bg-muted"
              >
                Retry
              </button>
            </div>
          ) : items.length === 0 && !loading ? (
            <p className="py-10 text-center text-[13px] text-fg-muted">{emptyText}</p>
          ) : (
            <div className="grid grid-cols-3 gap-2 sm:grid-cols-4 md:grid-cols-6">
              {items.map((item) => {
                const tile = (
                  <button
                    type="button"
                    onClick={() => {
                      onSelect(item);
                      onOpenChange(false);
                    }}
                    className="grid w-full cursor-pointer gap-1 rounded-md border border-border bg-muted p-2 text-center transition-colors hover:border-fg-subtle hover:bg-bg focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-fg-subtle"
                  >
                    <div className="grid h-12 place-items-center">
                      {getImageUrl(item) ? (
                        <img
                          src={getImageUrl(item)}
                          alt=""
                          className="max-h-12 max-w-full object-contain [image-rendering:pixelated]"
                        />
                      ) : null}
                    </div>
                    <span className="truncate text-[11px] text-fg-muted">{getLabel(item)}</span>
                  </button>
                );

                return renderTooltip ? (
                  <Tooltip key={getKey(item)}>
                    <TooltipTrigger asChild>{tile}</TooltipTrigger>
                    <TooltipContent
                      side="top"
                      sideOffset={10}
                      className="border-0 bg-transparent p-0 text-left shadow-none [&>svg]:bg-[#07091a] [&>svg]:fill-[#07091a]"
                    >
                      {renderTooltip(item)}
                    </TooltipContent>
                  </Tooltip>
                ) : (
                  <div key={getKey(item)}>{tile}</div>
                );
              })}
            </div>
          )}

          {hasMore ? <div ref={sentinelRef} className="h-8" /> : null}
          {loading ? <p className="py-3 text-center text-[12px] text-fg-muted">Loading…</p> : null}
        </div>
      </DialogContent>
    </Dialog>
  );
}

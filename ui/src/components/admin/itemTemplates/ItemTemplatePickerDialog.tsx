import { useEffect, useRef, useState } from "react";
import { Search } from "lucide-react";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";
import { useItemTemplateSearch } from "../../../lib/useItemTemplateSearch";
import { ItemTemplateTooltip } from "./ItemTemplateTooltip";

type ItemTemplatePickerDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  accessToken: string;
  onSelect: (id: string) => void;
  includeAbstract?: boolean;
  title?: string;
};

/** Reusable modal that browses item templates as an image grid and returns the chosen template id. */
export function ItemTemplatePickerDialog({
  open,
  onOpenChange,
  accessToken,
  onSelect,
  includeAbstract = false,
  title = "Select an item"
}: ItemTemplatePickerDialogProps) {
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

  const { items, loading, error, hasMore, loadMore, reset } = useItemTemplateSearch(accessToken, {
    search,
    includeAbstract
  });

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
      <DialogContent className="flex max-h-[80vh] w-[60vw] max-w-[60vw] flex-col gap-3 bg-surface sm:max-w-[60vw]">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
        </DialogHeader>

        <div className="relative">
          <Search size={15} aria-hidden className="pointer-events-none absolute left-2.5 top-1/2 -translate-y-1/2 text-fg-subtle" />
          <Input
            value={query}
            onChange={(event) => setQuery(event.target.value)}
            placeholder="Search items…"
            aria-label="Search item templates"
            className="h-9 bg-bg pl-8 text-[13px]"
          />
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
            <p className="py-10 text-center text-[13px] text-fg-muted">No items found.</p>
          ) : (
            <div className="grid grid-cols-3 gap-2 sm:grid-cols-4 md:grid-cols-6">
              {items.map((template) => (
                <Tooltip key={template.id}>
                  <TooltipTrigger asChild>
                    <button
                      type="button"
                      onClick={() => {
                        onSelect(template.id);
                        onOpenChange(false);
                      }}
                      className="grid cursor-pointer gap-1 rounded-md border border-border bg-muted p-2 text-center transition-colors hover:border-fg-subtle hover:bg-bg focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-fg-subtle"
                    >
                      <div className="grid h-12 place-items-center">
                        {template.imageUrl ? (
                          <img
                            src={template.imageUrl}
                            alt=""
                            className="max-h-12 max-w-full object-contain [image-rendering:pixelated]"
                          />
                        ) : null}
                      </div>
                      <span className="truncate text-[11px] text-fg-muted">{template.name}</span>
                    </button>
                  </TooltipTrigger>
                  <TooltipContent
                    side="top"
                    sideOffset={10}
                    className="border-0 bg-transparent p-0 text-left shadow-none [&>svg]:bg-[#07091a] [&>svg]:fill-[#07091a]"
                  >
                    <ItemTemplateTooltip template={template} />
                  </TooltipContent>
                </Tooltip>
              ))}
            </div>
          )}

          {hasMore ? <div ref={sentinelRef} className="h-8" /> : null}
          {loading ? <p className="py-3 text-center text-[12px] text-fg-muted">Loading…</p> : null}
        </div>
      </DialogContent>
    </Dialog>
  );
}

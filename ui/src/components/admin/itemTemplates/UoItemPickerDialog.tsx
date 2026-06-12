import { useCallback, useEffect, useState } from "react";
import { Search } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Skeleton } from "@/components/ui/skeleton";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { listUoItems } from "../../../lib/adminUoItemsClient";
import type { UoItemSummary } from "../../../types/uoItems";
import { ItemImageCell } from "./ItemImageCell";

type UoItemPickerDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  accessToken: string;
  onSelect: (item: UoItemSummary) => void;
  title?: string;
};

const PAGE_SIZE = 40;
const flagOptions = ["Container", "Weapon", "Armor", "Wearable", "Door", "Surface", "Background", "Wall"];

export function UoItemPickerDialog({
  open,
  onOpenChange,
  accessToken,
  onSelect,
  title = "Select UO item"
}: UoItemPickerDialogProps) {
  const [query, setQuery] = useState("");
  const [search, setSearch] = useState("");
  const [flag, setFlag] = useState("all");
  const [page, setPage] = useState(1);
  const [items, setItems] = useState<UoItemSummary[]>([]);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const handle = window.setTimeout(() => {
      setSearch(query);
      setPage(1);
    }, 250);

    return () => window.clearTimeout(handle);
  }, [query]);

  useEffect(() => {
    if (open) {
      setQuery("");
      setSearch("");
      setFlag("all");
      setPage(1);
    }
  }, [open]);

  const load = useCallback(async () => {
    if (!open) {
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const result = await listUoItems(accessToken, {
        page,
        pageSize: PAGE_SIZE,
        search,
        flag: flag === "all" ? "" : flag
      });

      setItems(result.items);
      setTotalPages(Math.max(1, result.totalPages));
      setTotalCount(result.totalCount);
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : "Failed to load UO items.");
      setItems([]);
      setTotalPages(1);
      setTotalCount(0);
    } finally {
      setLoading(false);
    }
  }, [accessToken, flag, open, page, search]);

  useEffect(() => {
    void load();
  }, [load]);

  function selectItem(item: UoItemSummary) {
    onSelect(item);
    onOpenChange(false);
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="flex max-h-[82vh] w-[76vw] max-w-[76vw] flex-col gap-3 bg-surface sm:max-w-[76vw]">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
        </DialogHeader>

        <div className="grid gap-2 md:grid-cols-[minmax(260px,1fr)_180px]">
          <label className="relative">
            <Search size={15} aria-hidden className="pointer-events-none absolute left-2.5 top-1/2 -translate-y-1/2 text-fg-subtle" />
            <Input
              value={query}
              onChange={(event) => setQuery(event.target.value)}
              placeholder="Search raw item id, name or flag..."
              aria-label="Search UO items"
              className="h-9 bg-bg pl-8 text-[13px]"
            />
          </label>
          <Select
            value={flag}
            onValueChange={(value) => {
              setFlag(value);
              setPage(1);
            }}
          >
            <SelectTrigger aria-label="Filter UO items by flag" className="h-9 w-full bg-bg text-[13px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All flags</SelectItem>
              {flagOptions.map((option) => (
                <SelectItem key={option} value={option}>
                  {option}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        {error && (
          <div className="flex items-center justify-between gap-3 rounded-md bg-danger/10 p-3 text-[13px] font-medium text-danger">
            <span>{error}</span>
            <Button type="button" variant="ghost" size="sm" onClick={() => void load()} className="h-7 px-2 text-danger">
              Retry
            </Button>
          </div>
        )}

        <div className="min-h-0 flex-1 overflow-auto rounded-md border border-border bg-surface">
          {loading ? (
            <div className="grid gap-2 p-4">
              <Skeleton className="h-8 w-full" />
              <Skeleton className="h-52 w-full" />
            </div>
          ) : items.length === 0 ? (
            <p className="m-0 p-8 text-center text-[13px] text-fg-muted">No UO items match this search.</p>
          ) : (
            <Table>
              <TableHeader>
                <TableRow className="border-border bg-surface-raised text-left text-[11px] font-medium text-fg-subtle hover:bg-surface-raised">
                  <TableHead className="w-14 px-2.5 py-2 text-[11px] font-medium text-fg-subtle">Art</TableHead>
                  <TableHead className="px-2.5 py-2 text-[11px] font-medium text-fg-subtle">Item</TableHead>
                  <TableHead className="px-2.5 py-2 text-[11px] font-medium text-fg-subtle">Name</TableHead>
                  <TableHead className="px-2.5 py-2 text-[11px] font-medium text-fg-subtle">Stats</TableHead>
                  <TableHead className="px-2.5 py-2 text-[11px] font-medium text-fg-subtle">Flags</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {items.map((item) => (
                  <TableRow
                    key={item.itemId}
                    onClick={() => selectItem(item)}
                    className="cursor-pointer border-border/70 transition-colors last:border-b-0 hover:bg-muted/70"
                  >
                    <TableCell className="px-2.5 py-1.5">
                      <ItemImageCell src={item.imageUrl} alt={item.name || item.itemIdHex} />
                    </TableCell>
                    <TableCell className="px-2.5 py-1.5 font-mono text-xs font-medium text-fg">{item.itemIdHex}</TableCell>
                    <TableCell className="px-2.5 py-1.5 font-medium text-fg">{item.name || "-"}</TableCell>
                    <TableCell className="px-2.5 py-1.5 font-mono text-xs text-fg-muted">
                      w:{item.weight} v:{item.value} h:{item.height}
                    </TableCell>
                    <TableCell className="px-2.5 py-1.5">
                      <div className="flex max-w-[360px] flex-wrap gap-1">
                        {item.flags.slice(0, 8).map((itemFlag) => (
                          <Badge
                            key={itemFlag}
                            variant="outline"
                            className="rounded-md border-transparent bg-muted px-1.5 py-0.5 text-[11px] font-medium text-fg-muted"
                          >
                            {itemFlag}
                          </Badge>
                        ))}
                        {item.flags.length > 8 && (
                          <Badge variant="outline" className="rounded-md border-border bg-bg px-1.5 py-0.5 text-[11px] font-medium text-fg-muted">
                            +{item.flags.length - 8}
                          </Badge>
                        )}
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </div>

        <div className="flex items-center justify-between text-xs text-fg-muted">
          <span className="font-mono">{totalCount} items</span>
          <div className="flex items-center gap-2">
            <Button
              type="button"
              variant="ghost"
              size="sm"
              disabled={page <= 1}
              onClick={() => setPage((current) => Math.max(1, current - 1))}
              className="min-h-[28px] px-2 font-medium text-fg-muted hover:bg-muted hover:text-fg"
            >
              Prev
            </Button>
            <span className="font-mono">
              Page {page} of {totalPages}
            </span>
            <Button
              type="button"
              variant="ghost"
              size="sm"
              disabled={page >= totalPages}
              onClick={() => setPage((current) => Math.min(totalPages, current + 1))}
              className="min-h-[28px] px-2 font-medium text-fg-muted hover:bg-muted hover:text-fg"
            >
              Next
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}

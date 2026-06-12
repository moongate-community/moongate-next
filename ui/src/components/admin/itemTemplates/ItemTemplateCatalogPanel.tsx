import { useCallback, useEffect, useState } from "react";
import { ArrowLeft, Boxes, Check, ChevronsUpDown, RefreshCw, Search } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Command, CommandEmpty, CommandGroup, CommandInput, CommandItem, CommandList } from "@/components/ui/command";
import { Input } from "@/components/ui/input";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Skeleton } from "@/components/ui/skeleton";
import { getItemTemplate, listItemTemplates } from "../../../lib/adminItemTemplatesClient";
import type { AdminCommandTarget } from "../../../types/adminCommandTarget";
import type { ItemTemplateDetail, ItemTemplateFilters, ItemTemplateSummary } from "../../../types/itemTemplates";
import { Panel } from "../Panel";
import { ItemTemplateDetailPanel } from "./ItemTemplateDetailPanel";
import { ItemTemplatePickerDialog } from "./ItemTemplatePickerDialog";
import { ItemTemplateTable } from "./ItemTemplateTable";

type ItemTemplateCatalogPanelProps = {
  accessToken: string;
  commandTarget?: Extract<AdminCommandTarget, { kind: "itemTemplate" }> | null;
};

const PAGE_SIZE = 50;

const defaultFilters: ItemTemplateFilters = {
  page: 1,
  pageSize: PAGE_SIZE,
  search: "",
  tag: "",
  rarity: "",
  layer: "",
  abstract: "all"
};

const rarityOptions = ["None", "Common", "Uncommon", "Rare", "Epic", "Legendary"];
const layerOptions = [
  "OneHanded",
  "TwoHanded",
  "Shoes",
  "Pants",
  "Shirt",
  "Helm",
  "Gloves",
  "Ring",
  "Talisman",
  "Neck",
  "Hair",
  "Waist",
  "InnerTorso",
  "Bracelet",
  "Unused_xF",
  "FacialHair",
  "MiddleTorso",
  "Earrings",
  "Arms",
  "Cloak",
  "Backpack",
  "OuterTorso",
  "OuterLegs",
  "InnerLegs",
  "Mount",
  "ShopBuy",
  "ShopResale",
  "ShopSell",
  "Bank"
];

function replaceItemTemplateUrl(id?: string) {
  const params = new URLSearchParams({ view: "itemTemplates" });

  if (id) {
    params.set("itemTemplate", id);
  }

  window.history.replaceState(null, "", `/admin?${params.toString()}`);
}

export function ItemTemplateCatalogPanel({ accessToken, commandTarget }: ItemTemplateCatalogPanelProps) {
  const [filters, setFilters] = useState<ItemTemplateFilters>(defaultFilters);
  const [search, setSearch] = useState("");
  const [templates, setTemplates] = useState<ItemTemplateSummary[]>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [detail, setDetail] = useState<ItemTemplateDetail | null>(null);
  const [detailPageOpen, setDetailPageOpen] = useState(false);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [detailLoading, setDetailLoading] = useState(false);
  const [layerOpen, setLayerOpen] = useState(false);
  const [layerSearch, setLayerSearch] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [detailError, setDetailError] = useState<string | null>(null);
  const [pickerOpen, setPickerOpen] = useState(false);

  useEffect(() => {
    const timer = window.setTimeout(() => {
      setFilters((current) => ({ ...current, search, page: 1 }));
    }, 300);

    return () => window.clearTimeout(timer);
  }, [search]);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const result = await listItemTemplates(accessToken, filters);
      setTemplates(result.items);
      setTotalPages(Math.max(1, result.totalPages));
      setTotalCount(result.totalCount);
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : "Failed to load item templates");
      setTemplates([]);
    } finally {
      setLoading(false);
    }
  }, [accessToken, filters]);

  useEffect(() => {
    void load();
  }, [load]);

  const openTemplateDetail = useCallback(async (id: string, updateUrl = true) => {
    setSelectedId(id);
    setDetail(null);
    setDetailPageOpen(true);
    setDetailLoading(true);
    setDetailError(null);

    if (updateUrl) {
      replaceItemTemplateUrl(id);
    }

    try {
      setDetail(await getItemTemplate(accessToken, id));
    } catch (caught) {
      setDetailError(caught instanceof Error ? caught.message : "Failed to load template detail");
    } finally {
      setDetailLoading(false);
    }
  }, [accessToken]);

  useEffect(() => {
    if (!commandTarget) {
      return;
    }

    void openTemplateDetail(commandTarget.id, false);
  }, [commandTarget?.id, commandTarget?.sequence, openTemplateDetail]);

  async function selectTemplate(template: ItemTemplateSummary) {
    await openTemplateDetail(template.id);
  }

  function closeDetailPage() {
    setSelectedId(null);
    setDetail(null);
    setDetailError(null);
    setDetailPageOpen(false);
    replaceItemTemplateUrl();
  }

  function updateFilter<K extends keyof ItemTemplateFilters>(key: K, value: ItemTemplateFilters[K]) {
    setFilters((current) => ({ ...current, [key]: value, page: 1 }));
  }

  function selectLayer(layer: string) {
    updateFilter("layer", layer);
    setLayerSearch("");
    setLayerOpen(false);
  }

  if (detailPageOpen) {
    return (
      <div className="grid gap-3">
        <div className="flex min-h-[48px] items-center justify-between gap-3 rounded-md border border-border bg-surface px-4 py-2">
          <div className="min-w-0">
            <h3 className="m-0 truncate text-sm font-semibold tracking-tight text-fg">Item Template</h3>
            <p className="m-0 truncate font-mono text-[11px] text-fg-subtle">{selectedId}</p>
          </div>
          <Button
            type="button"
            variant="ghost"
            size="sm"
            onClick={closeDetailPage}
            className="min-h-[30px] gap-1.5 px-2.5 text-[13px] font-medium text-fg-muted hover:bg-muted hover:text-fg"
          >
            <ArrowLeft size={14} aria-hidden />
            All items
          </Button>
        </div>
        <ItemTemplateDetailPanel template={detail} loading={detailLoading} error={detailError} />
      </div>
    );
  }

  return (
    <Panel
      title="Item Templates"
      action={
        <div className="flex items-center gap-1.5">
          <Button
            type="button"
            variant="ghost"
            size="sm"
            onClick={() => setPickerOpen(true)}
            className="min-h-[30px] gap-1.5 px-2.5 text-[13px] font-medium text-fg-muted hover:bg-muted hover:text-fg"
          >
            <Boxes size={14} aria-hidden />
            Browse items
          </Button>
          <Button
            type="button"
            variant="ghost"
            size="sm"
            onClick={() => void load()}
            className="min-h-[30px] gap-1.5 px-2.5 text-[13px] font-medium text-fg-muted hover:bg-muted hover:text-fg"
          >
            <RefreshCw size={14} aria-hidden />
            Refresh
          </Button>
          <ItemTemplatePickerDialog
            open={pickerOpen}
            onOpenChange={setPickerOpen}
            accessToken={accessToken}
            onSelect={(id) => {
              setPickerOpen(false);
              void openTemplateDetail(id);
            }}
          />
        </div>
      }
    >
      <div className="grid gap-3">
        <div className="grid gap-2 lg:grid-cols-[minmax(260px,1fr)_140px_140px_140px_128px]">
          <label className="relative">
            <Search size={15} aria-hidden className="pointer-events-none absolute left-2.5 top-1/2 -translate-y-1/2 text-fg-subtle" />
            <Input
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Search id, name, comment, tag, script or item id…"
              aria-label="Search item templates"
              className="h-8 bg-bg pl-8 pr-2.5 text-[13px] text-fg focus-visible:bg-surface"
            />
          </label>
          <Input
            value={filters.tag}
            onChange={(event) => updateFilter("tag", event.target.value)}
            placeholder="Tag"
            className="h-8 bg-bg px-2.5 text-[13px] text-fg focus-visible:bg-surface"
          />
          <Select value={filters.rarity || "all"} onValueChange={(value) => updateFilter("rarity", value === "all" ? "" : value)}>
            <SelectTrigger aria-label="Filter item templates by rarity" className="h-8 w-full bg-bg text-[13px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All rarities</SelectItem>
              {rarityOptions.map((rarity) => (
                <SelectItem key={rarity} value={rarity}>
                  {rarity}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Popover
            open={layerOpen}
            onOpenChange={(open) => {
              setLayerOpen(open);
              if (!open) {
                setLayerSearch("");
              }
            }}
          >
            <PopoverTrigger asChild>
              <Button
                type="button"
                variant="outline"
                role="combobox"
                aria-expanded={layerOpen}
                aria-label="Filter item templates by layer"
                className="h-8 w-full justify-between bg-bg px-2.5 text-[13px] font-normal text-fg hover:bg-surface"
              >
                <span className="min-w-0 truncate">{filters.layer || "Layer"}</span>
                <ChevronsUpDown className="size-3.5 shrink-0 text-fg-subtle" aria-hidden />
              </Button>
            </PopoverTrigger>
            <PopoverContent align="start" className="w-(--radix-popover-trigger-width) p-0">
              <Command>
                <CommandInput value={layerSearch} onValueChange={setLayerSearch} placeholder="Search layer" />
                <CommandList>
                  <CommandEmpty>No layer found.</CommandEmpty>
                  <CommandGroup>
                    <CommandItem value="all-layers" onSelect={() => selectLayer("")}>
                      <span>All layers</span>
                      <Check className={`ml-auto size-3.5 ${filters.layer === "" ? "opacity-100" : "opacity-0"}`} aria-hidden />
                    </CommandItem>
                    {layerOptions.map((layer) => (
                      <CommandItem key={layer} value={layer} onSelect={() => selectLayer(layer)}>
                        <span>{layer}</span>
                        <Check className={`ml-auto size-3.5 ${filters.layer === layer ? "opacity-100" : "opacity-0"}`} aria-hidden />
                      </CommandItem>
                    ))}
                  </CommandGroup>
                </CommandList>
              </Command>
            </PopoverContent>
          </Popover>
          <Select
            value={filters.abstract}
            onValueChange={(value) => updateFilter("abstract", value as ItemTemplateFilters["abstract"])}
          >
            <SelectTrigger aria-label="Filter item templates by abstract state" className="h-8 w-full bg-bg text-[13px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All</SelectItem>
              <SelectItem value="false">Concrete</SelectItem>
              <SelectItem value="true">Abstract</SelectItem>
            </SelectContent>
          </Select>
        </div>

        {error && <p className="m-0 rounded-md bg-danger/10 p-3 text-[13px] font-medium text-danger">{error}</p>}

        <div className="grid gap-3">
          <div className="min-w-0">
            {loading ? (
              <div className="grid gap-2 rounded-md bg-muted p-4">
                <Skeleton className="h-4 w-48" />
                <Skeleton className="h-64 w-full" />
              </div>
            ) : (
              <ItemTemplateTable templates={templates} selectedId={selectedId} onSelect={selectTemplate} />
            )}

            <div className="mt-3 flex items-center justify-between text-xs text-fg-muted">
              <span className="font-mono">{totalCount} templates</span>
              <div className="flex items-center gap-2">
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  disabled={filters.page <= 1}
                  onClick={() => setFilters((current) => ({ ...current, page: Math.max(1, current.page - 1) }))}
                  className="min-h-[28px] px-2 font-medium text-fg-muted hover:bg-muted hover:text-fg"
                >
                  Prev
                </Button>
                <span className="font-mono">
                  Page {filters.page} of {totalPages}
                </span>
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  disabled={filters.page >= totalPages}
                  onClick={() => setFilters((current) => ({ ...current, page: Math.min(totalPages, current.page + 1) }))}
                  className="min-h-[28px] px-2 font-medium text-fg-muted hover:bg-muted hover:text-fg"
                >
                  Next
                </Button>
              </div>
            </div>
          </div>
        </div>
      </div>

    </Panel>
  );
}

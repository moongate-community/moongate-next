import { useCallback, useEffect, useState } from "react";
import { RefreshCw, Search } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogDescription, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Skeleton } from "@/components/ui/skeleton";
import { getMobileTemplate, listMobileTemplates } from "../../../lib/adminMobileTemplatesClient";
import type { MobileTemplateDetail, MobileTemplateFilters, MobileTemplateSummary } from "../../../types/mobileTemplates";
import { Panel } from "../Panel";
import { MobileTemplateDetailPanel } from "./MobileTemplateDetailPanel";
import { MobileTemplateTable } from "./MobileTemplateTable";

type MobileTemplateCatalogPanelProps = {
  accessToken: string;
};

const PAGE_SIZE = 50;

const defaultFilters: MobileTemplateFilters = {
  page: 1,
  pageSize: PAGE_SIZE,
  search: "",
  tag: "",
  notoriety: "",
  abstract: "all"
};

const notorietyOptions = [
  "Innocent",
  "Friend",
  "CanBeAttacked",
  "Criminal",
  "Enemy",
  "Murdered",
  "Invulnerable"
];

export function MobileTemplateCatalogPanel({ accessToken }: MobileTemplateCatalogPanelProps) {
  const [filters, setFilters] = useState<MobileTemplateFilters>(defaultFilters);
  const [search, setSearch] = useState("");
  const [templates, setTemplates] = useState<MobileTemplateSummary[]>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [detail, setDetail] = useState<MobileTemplateDetail | null>(null);
  const [detailOpen, setDetailOpen] = useState(false);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [detailLoading, setDetailLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [detailError, setDetailError] = useState<string | null>(null);

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
      const result = await listMobileTemplates(accessToken, filters);
      setTemplates(result.items);
      setTotalPages(Math.max(1, result.totalPages));
      setTotalCount(result.totalCount);

      setSelectedId((current) => {
        if (current && !result.items.some((item) => item.id === current)) {
          setDetail(null);
          setDetailOpen(false);

          return null;
        }

        return current;
      });
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : "Failed to load mobile templates");
      setTemplates([]);
    } finally {
      setLoading(false);
    }
  }, [accessToken, filters]);

  useEffect(() => {
    void load();
  }, [load]);

  async function selectTemplate(id: string) {
    setSelectedId(id);
    setDetail(null);
    setDetailOpen(true);
    setDetailLoading(true);
    setDetailError(null);

    try {
      setDetail(await getMobileTemplate(accessToken, id));
    } catch (caught) {
      setDetailError(caught instanceof Error ? caught.message : "Failed to load template detail");
    } finally {
      setDetailLoading(false);
    }
  }

  function updateFilter<K extends keyof MobileTemplateFilters>(key: K, value: MobileTemplateFilters[K]) {
    setFilters((current) => ({ ...current, [key]: value, page: 1 }));
  }

  return (
    <Panel
      title="Mobile Templates"
      action={
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
      }
    >
      <div className="grid gap-3">
        <div className="grid gap-2 lg:grid-cols-[minmax(260px,1fr)_140px_140px_128px]">
          <label className="relative">
            <Search size={15} aria-hidden className="pointer-events-none absolute left-2.5 top-1/2 -translate-y-1/2 text-fg-subtle" />
            <Input
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Search id, name, tag, brain…"
              aria-label="Search mobile templates"
              className="h-8 bg-bg pl-8 pr-2.5 text-[13px] text-fg focus-visible:bg-surface"
            />
          </label>
          <Input
            value={filters.tag}
            onChange={(event) => updateFilter("tag", event.target.value)}
            placeholder="Tag"
            className="h-8 bg-bg px-2.5 text-[13px] text-fg focus-visible:bg-surface"
          />
          <Select value={filters.notoriety || "all"} onValueChange={(value) => updateFilter("notoriety", value === "all" ? "" : value)}>
            <SelectTrigger aria-label="Filter mobile templates by notoriety" size="sm" className="h-8 w-full bg-bg text-[13px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All notorieties</SelectItem>
            {notorietyOptions.map((notoriety) => (
              <SelectItem key={notoriety} value={notoriety}>
                {notoriety}
              </SelectItem>
            ))}
            </SelectContent>
          </Select>
          <Select
            value={filters.abstract}
            onValueChange={(value) => updateFilter("abstract", value as MobileTemplateFilters["abstract"])}
          >
            <SelectTrigger aria-label="Filter mobile templates by abstract state" size="sm" className="h-8 w-full bg-bg text-[13px]">
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
              <MobileTemplateTable templates={templates} selectedId={selectedId} onSelect={selectTemplate} />
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

      <Dialog open={detailOpen} onOpenChange={setDetailOpen}>
        <DialogContent className="max-h-[calc(100vh-2rem)] overflow-y-auto p-0 sm:max-w-[720px] lg:max-w-[820px]">
          <DialogTitle className="sr-only">Mobile template detail</DialogTitle>
          <DialogDescription className="sr-only">Selected mobile template properties and body data.</DialogDescription>
          <MobileTemplateDetailPanel template={detail} loading={detailLoading} error={detailError} />
        </DialogContent>
      </Dialog>
    </Panel>
  );
}

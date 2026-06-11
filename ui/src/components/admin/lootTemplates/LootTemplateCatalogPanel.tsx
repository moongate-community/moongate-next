import { useCallback, useEffect, useState } from "react";
import { ArrowLeft, PackageOpen, RefreshCw, Search } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { Panel } from "../Panel";
import { getLootTemplate, listLootTemplates } from "../../../lib/adminLootTemplatesClient";
import type { AdminCommandTarget } from "../../../types/adminCommandTarget";
import type {
  LootTemplateDetail,
  LootTemplateFilters,
  LootTemplateSummary
} from "../../../types/lootTemplates";
import { LootTemplateDetailPanel } from "./LootTemplateDetailPanel";

type LootTemplateCatalogPanelProps = {
  accessToken: string;
  commandTarget?: Extract<AdminCommandTarget, { kind: "lootTemplate" }> | null;
};

type LootTemplateListProps = {
  templates: LootTemplateSummary[];
  selectedId: string | null;
  onSelect: (template: LootTemplateSummary) => void;
};

const PAGE_SIZE = 50;

const defaultFilters: LootTemplateFilters = {
  page: 1,
  pageSize: PAGE_SIZE,
  search: ""
};

function replaceLootTemplateUrl(id?: string) {
  const params = new URLSearchParams({ view: "lootTemplates" });

  if (id) {
    params.set("lootTemplate", id);
  }

  window.history.replaceState(null, "", `/admin?${params.toString()}`);
}

function LootTemplateList({ templates, selectedId, onSelect }: LootTemplateListProps) {
  if (templates.length === 0) {
    return (
      <p className="m-0 rounded-md border border-dashed border-border bg-bg p-6 text-center text-[13px] leading-relaxed text-fg-muted">
        No loot templates match this search.
      </p>
    );
  }

  return (
    <div className="overflow-hidden rounded-md border border-border bg-surface">
      {templates.map((template) => (
        <button
          key={template.id}
          type="button"
          onClick={() => onSelect(template)}
          className={`flex min-h-[44px] w-full items-center justify-between gap-3 border-b border-border/70 px-3 py-2 text-left last:border-b-0 hover:bg-muted/70 ${
            selectedId === template.id ? "bg-muted" : ""
          }`}
        >
          <span className="flex min-w-0 items-center gap-2">
            <span className="grid h-7 w-7 shrink-0 place-items-center rounded-md bg-muted text-fg-muted">
              <PackageOpen size={15} aria-hidden />
            </span>
            <span className="min-w-0 truncate font-mono text-xs font-medium text-fg">{template.id}</span>
          </span>
          <span className="shrink-0 font-mono text-[11px] text-fg-subtle">{template.rootNodeCount} roots</span>
        </button>
      ))}
    </div>
  );
}

export function LootTemplateCatalogPanel({ accessToken, commandTarget }: LootTemplateCatalogPanelProps) {
  const [filters, setFilters] = useState<LootTemplateFilters>(defaultFilters);
  const [search, setSearch] = useState("");
  const [templates, setTemplates] = useState<LootTemplateSummary[]>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [detail, setDetail] = useState<LootTemplateDetail | null>(null);
  const [detailPageOpen, setDetailPageOpen] = useState(false);
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
      const result = await listLootTemplates(accessToken, filters);
      setTemplates(result.items);
      setTotalPages(Math.max(1, result.totalPages));
      setTotalCount(result.totalCount);
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : "Failed to load loot templates");
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
      replaceLootTemplateUrl(id);
    }

    try {
      setDetail(await getLootTemplate(accessToken, id));
    } catch (caught) {
      setDetailError(caught instanceof Error ? caught.message : "Failed to load loot template detail");
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

  async function selectTemplate(template: LootTemplateSummary) {
    await openTemplateDetail(template.id);
  }

  function closeDetailPage() {
    setSelectedId(null);
    setDetail(null);
    setDetailError(null);
    setDetailPageOpen(false);
    replaceLootTemplateUrl();
  }

  if (detailPageOpen) {
    return (
      <div className="grid gap-3">
        <div className="flex min-h-[48px] items-center justify-between gap-3 rounded-md border border-border bg-surface px-4 py-2">
          <div className="min-w-0">
            <h3 className="m-0 truncate text-sm font-semibold tracking-tight text-fg">Loot Template</h3>
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
            All loot
          </Button>
        </div>
        <LootTemplateDetailPanel template={detail} loading={detailLoading} error={detailError} />
      </div>
    );
  }

  return (
    <Panel
      title="Loot Templates"
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
        <label className="relative">
          <Search size={15} aria-hidden className="pointer-events-none absolute left-2.5 top-1/2 -translate-y-1/2 text-fg-subtle" />
          <Input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Search loot templates"
            aria-label="Search loot templates"
            className="h-8 bg-bg pl-8 pr-2.5 text-[13px] text-fg focus-visible:bg-surface"
          />
        </label>

        {error && <p className="m-0 rounded-md bg-danger/10 p-3 text-[13px] font-medium text-danger">{error}</p>}

        {loading ? (
          <div className="grid gap-2 rounded-md bg-muted p-4">
            <Skeleton className="h-4 w-48" />
            <Skeleton className="h-64 w-full" />
          </div>
        ) : (
          <LootTemplateList templates={templates} selectedId={selectedId} onSelect={selectTemplate} />
        )}

        <div className="flex items-center justify-between text-xs text-fg-muted">
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
    </Panel>
  );
}

import { useCallback, useEffect, useState } from "react";
import { RefreshCw, Search } from "lucide-react";
import { getItemTemplate, listItemTemplates } from "../../../lib/adminItemTemplatesClient";
import type { ItemTemplateDetail, ItemTemplateFilters, ItemTemplateSummary } from "../../../types/itemTemplates";
import { Panel } from "../Panel";
import { ItemTemplateDetailPanel } from "./ItemTemplateDetailPanel";
import { ItemTemplateTable } from "./ItemTemplateTable";

type ItemTemplateCatalogPanelProps = {
  accessToken: string;
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

export function ItemTemplateCatalogPanel({ accessToken }: ItemTemplateCatalogPanelProps) {
  const [filters, setFilters] = useState<ItemTemplateFilters>(defaultFilters);
  const [search, setSearch] = useState("");
  const [templates, setTemplates] = useState<ItemTemplateSummary[]>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [detail, setDetail] = useState<ItemTemplateDetail | null>(null);
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
      const result = await listItemTemplates(accessToken, filters);
      setTemplates(result.items);
      setTotalPages(Math.max(1, result.totalPages));
      setTotalCount(result.totalCount);

      setSelectedId((current) => {
        if (current && !result.items.some((item) => item.id === current)) {
          setDetail(null);

          return null;
        }

        return current;
      });
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

  async function selectTemplate(template: ItemTemplateSummary) {
    setSelectedId(template.id);
    setDetailLoading(true);
    setDetailError(null);

    try {
      setDetail(await getItemTemplate(accessToken, template.id));
    } catch (caught) {
      setDetailError(caught instanceof Error ? caught.message : "Failed to load template detail");
    } finally {
      setDetailLoading(false);
    }
  }

  function updateFilter<K extends keyof ItemTemplateFilters>(key: K, value: ItemTemplateFilters[K]) {
    setFilters((current) => ({ ...current, [key]: value, page: 1 }));
  }

  return (
    <Panel
      title="Item Templates"
      action={
        <button
          type="button"
          onClick={() => void load()}
          className="inline-flex min-h-[30px] items-center gap-1.5 rounded-md px-2.5 text-[13px] font-medium text-fg-muted transition-colors duration-150 hover:bg-muted hover:text-fg"
        >
          <RefreshCw size={14} aria-hidden />
          Refresh
        </button>
      }
    >
      <div className="grid gap-3">
        <div className="grid gap-2 lg:grid-cols-[minmax(260px,1fr)_140px_140px_140px_128px]">
          <label className="relative">
            <Search size={15} aria-hidden className="pointer-events-none absolute left-2.5 top-1/2 -translate-y-1/2 text-fg-subtle" />
            <input
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Search id, name, comment, tag, script or item id…"
              aria-label="Search item templates"
              className="h-8 w-full rounded-md border border-border bg-bg pl-8 pr-2.5 text-[13px] text-fg outline-none transition-colors placeholder:text-fg-subtle focus:border-border-strong focus:bg-surface"
            />
          </label>
          <input
            value={filters.tag}
            onChange={(event) => updateFilter("tag", event.target.value)}
            placeholder="Tag"
            className="h-8 rounded-md border border-border bg-bg px-2.5 text-[13px] text-fg outline-none transition-colors placeholder:text-fg-subtle focus:border-border-strong focus:bg-surface"
          />
          <input
            value={filters.rarity}
            onChange={(event) => updateFilter("rarity", event.target.value)}
            placeholder="Rarity"
            className="h-8 rounded-md border border-border bg-bg px-2.5 text-[13px] text-fg outline-none transition-colors placeholder:text-fg-subtle focus:border-border-strong focus:bg-surface"
          />
          <input
            value={filters.layer}
            onChange={(event) => updateFilter("layer", event.target.value)}
            placeholder="Layer"
            className="h-8 rounded-md border border-border bg-bg px-2.5 text-[13px] text-fg outline-none transition-colors placeholder:text-fg-subtle focus:border-border-strong focus:bg-surface"
          />
          <select
            value={filters.abstract}
            onChange={(event) => updateFilter("abstract", event.target.value as ItemTemplateFilters["abstract"])}
            className="h-8 rounded-md border border-border bg-bg px-2.5 text-[13px] text-fg outline-none transition-colors focus:border-border-strong focus:bg-surface"
          >
            <option value="all">All</option>
            <option value="false">Concrete</option>
            <option value="true">Abstract</option>
          </select>
        </div>

        {error && <p className="m-0 rounded-md bg-danger/10 p-3 text-[13px] font-medium text-danger">{error}</p>}

        <div className="grid gap-3 xl:grid-cols-[minmax(0,1fr)_340px]">
          <div className="min-w-0">
            {loading ? (
              <p className="m-0 rounded-md bg-muted p-4 text-[13px] font-medium text-fg-muted">Loading item templates…</p>
            ) : (
              <ItemTemplateTable templates={templates} selectedId={selectedId} onSelect={selectTemplate} />
            )}

            <div className="mt-3 flex items-center justify-between text-xs text-fg-muted">
              <span className="font-mono">{totalCount} templates</span>
              <div className="flex items-center gap-2">
                <button
                  type="button"
                  disabled={filters.page <= 1}
                  onClick={() => setFilters((current) => ({ ...current, page: Math.max(1, current.page - 1) }))}
                  className="inline-flex min-h-[28px] items-center rounded-md px-2 font-medium text-fg-muted transition-colors duration-150 hover:bg-muted hover:text-fg disabled:opacity-40"
                >
                  Prev
                </button>
                <span className="font-mono">
                  Page {filters.page} of {totalPages}
                </span>
                <button
                  type="button"
                  disabled={filters.page >= totalPages}
                  onClick={() => setFilters((current) => ({ ...current, page: Math.min(totalPages, current.page + 1) }))}
                  className="inline-flex min-h-[28px] items-center rounded-md px-2 font-medium text-fg-muted transition-colors duration-150 hover:bg-muted hover:text-fg disabled:opacity-40"
                >
                  Next
                </button>
              </div>
            </div>
          </div>

          <ItemTemplateDetailPanel template={detail} loading={detailLoading} error={detailError} />
        </div>
      </div>
    </Panel>
  );
}

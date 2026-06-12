import { useCallback, useEffect, useRef, useState } from "react";
import { listItemTemplates } from "./adminItemTemplatesClient";
import type { ItemTemplateSummary } from "../types/itemTemplates";

type UseItemTemplateSearchOptions = {
  search: string;
  includeAbstract: boolean;
  pageSize?: number;
};

type UseItemTemplateSearchResult = {
  items: ItemTemplateSummary[];
  loading: boolean;
  error: string | null;
  hasMore: boolean;
  loadMore: () => void;
  reset: () => void;
};

/**
 * Server-side paged item-template search with append-on-load-more semantics.
 * Resets to page 1 whenever the token, search or abstract scope changes; stale
 * responses are dropped so fast typing never shows out-of-order results.
 */
export function useItemTemplateSearch(
  accessToken: string,
  { search, includeAbstract, pageSize = 60 }: UseItemTemplateSearchOptions
): UseItemTemplateSearchResult {
  const [items, setItems] = useState<ItemTemplateSummary[]>([]);
  const [page, setPage] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const requestRef = useRef(0);

  const fetchPage = useCallback(
    async (target: number) => {
      const token = ++requestRef.current;

      setLoading(true);
      setError(null);

      try {
        const result = await listItemTemplates(accessToken, {
          page: target,
          pageSize,
          search: search.trim(),
          tag: "",
          rarity: "",
          layer: "",
          abstract: includeAbstract ? "all" : "false"
        });

        if (token !== requestRef.current) {
          return;
        }

        setTotalPages(result.totalPages);
        setPage(result.page);
        setItems((current) => (target === 1 ? result.items : [...current, ...result.items]));
      } catch (cause) {
        if (token !== requestRef.current) {
          return;
        }

        setError(cause instanceof Error ? cause.message : "Failed to load item templates.");
      } finally {
        if (token === requestRef.current) {
          setLoading(false);
        }
      }
    },
    [accessToken, search, includeAbstract, pageSize]
  );

  useEffect(() => {
    setItems([]);
    setPage(0);
    setTotalPages(0);
    void fetchPage(1);
  }, [fetchPage]);

  const loadMore = useCallback(() => {
    if (!loading && page < totalPages) {
      void fetchPage(page + 1);
    }
  }, [loading, page, totalPages, fetchPage]);

  const reset = useCallback(() => {
    void fetchPage(1);
  }, [fetchPage]);

  return {
    items,
    loading,
    error,
    hasMore: page < totalPages,
    loadMore,
    reset
  };
}

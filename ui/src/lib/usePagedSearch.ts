import { useCallback, useEffect, useRef, useState } from "react";
import type { PagedResult } from "../types/itemTemplates";

type UsePagedSearchResult<T> = {
  items: T[];
  loading: boolean;
  error: string | null;
  hasMore: boolean;
  loadMore: () => void;
  reset: () => void;
};

/**
 * Generic server-side paged search with append-on-load-more semantics. Resets to page 1 whenever
 * the `load` callback identity changes (callers memoize it over search/filter state); stale
 * responses are dropped so fast typing never shows out-of-order results.
 */
export function usePagedSearch<T>(load: (page: number) => Promise<PagedResult<T>>): UsePagedSearchResult<T> {
  const [items, setItems] = useState<T[]>([]);
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
        const result = await load(target);

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

        setError(cause instanceof Error ? cause.message : "Failed to load results.");
      } finally {
        if (token === requestRef.current) {
          setLoading(false);
        }
      }
    },
    [load]
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

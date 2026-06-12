import { readJson } from "./authClient";
import type { PagedResult } from "../types/itemTemplates";
import type { UoItemDetail, UoItemFilters, UoItemSummary } from "../types/uoItems";

function authHeaders(accessToken: string): HeadersInit {
  return { Authorization: `Bearer ${accessToken}` };
}

export async function listUoItems(accessToken: string, filters: UoItemFilters): Promise<PagedResult<UoItemSummary>> {
  const params = new URLSearchParams({
    page: String(filters.page),
    pageSize: String(filters.pageSize)
  });

  if (filters.search.trim().length > 0) {
    params.set("search", filters.search.trim());
  }

  if (filters.flag.trim().length > 0) {
    params.set("flag", filters.flag.trim());
  }

  const response = await fetch(`/api/admin/uo/items?${params.toString()}`, {
    headers: authHeaders(accessToken)
  });

  return readJson<PagedResult<UoItemSummary>>(response);
}

export async function getUoItem(accessToken: string, itemId: string | number): Promise<UoItemDetail> {
  const response = await fetch(`/api/admin/uo/items/${encodeURIComponent(String(itemId))}`, {
    headers: authHeaders(accessToken)
  });

  return readJson<UoItemDetail>(response);
}

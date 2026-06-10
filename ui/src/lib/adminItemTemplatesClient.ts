import { readJson } from "./authClient";
import type { ItemTemplateDetail, ItemTemplateFilters, ItemTemplateSummary, PagedResult } from "../types/itemTemplates";

function authHeaders(accessToken: string): HeadersInit {
  return { Authorization: `Bearer ${accessToken}` };
}

export async function listItemTemplates(
  accessToken: string,
  filters: ItemTemplateFilters
): Promise<PagedResult<ItemTemplateSummary>> {
  const params = new URLSearchParams({
    page: String(filters.page),
    pageSize: String(filters.pageSize)
  });

  if (filters.search.trim().length > 0) {
    params.set("search", filters.search.trim());
  }

  if (filters.tag.trim().length > 0) {
    params.set("tag", filters.tag.trim());
  }

  if (filters.rarity.trim().length > 0) {
    params.set("rarity", filters.rarity.trim());
  }

  if (filters.layer.trim().length > 0) {
    params.set("layer", filters.layer.trim());
  }

  if (filters.abstract !== "all") {
    params.set("abstract", filters.abstract);
  }

  const response = await fetch(`/api/admin/item-templates?${params.toString()}`, {
    headers: authHeaders(accessToken)
  });

  return readJson<PagedResult<ItemTemplateSummary>>(response);
}

export async function getItemTemplate(accessToken: string, id: string): Promise<ItemTemplateDetail> {
  const response = await fetch(`/api/admin/item-templates/${encodeURIComponent(id)}`, {
    headers: authHeaders(accessToken)
  });

  return readJson<ItemTemplateDetail>(response);
}

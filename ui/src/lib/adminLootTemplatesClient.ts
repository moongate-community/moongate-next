import { readJson } from "./authClient";
import type {
  LootTemplateDetail,
  LootTemplateFilters,
  LootTemplateSummary,
  PagedResult
} from "../types/lootTemplates";

function authHeaders(accessToken: string): HeadersInit {
  return { Authorization: `Bearer ${accessToken}` };
}

export async function listLootTemplates(
  accessToken: string,
  filters: LootTemplateFilters
): Promise<PagedResult<LootTemplateSummary>> {
  const params = new URLSearchParams({
    page: String(filters.page),
    pageSize: String(filters.pageSize)
  });

  if (filters.search.trim().length > 0) {
    params.set("search", filters.search.trim());
  }

  const response = await fetch(`/api/admin/loot-templates?${params.toString()}`, {
    headers: authHeaders(accessToken)
  });

  return readJson<PagedResult<LootTemplateSummary>>(response);
}

export async function getLootTemplate(accessToken: string, id: string): Promise<LootTemplateDetail> {
  const response = await fetch(`/api/admin/loot-templates/${encodeURIComponent(id)}`, {
    headers: authHeaders(accessToken)
  });

  return readJson<LootTemplateDetail>(response);
}

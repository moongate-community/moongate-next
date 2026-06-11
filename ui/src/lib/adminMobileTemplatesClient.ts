import { readJson } from "./authClient";
import type {
  MobileTemplateDetail,
  MobileTemplateFilters,
  MobileTemplateSummary,
  PagedResult
} from "../types/mobileTemplates";

function authHeaders(accessToken: string): HeadersInit {
  return { Authorization: `Bearer ${accessToken}` };
}

export async function listMobileTemplates(
  accessToken: string,
  filters: MobileTemplateFilters
): Promise<PagedResult<MobileTemplateSummary>> {
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

  if (filters.notoriety.trim().length > 0) {
    params.set("notoriety", filters.notoriety.trim());
  }

  if (filters.abstract !== "all") {
    params.set("abstract", filters.abstract);
  }

  const response = await fetch(`/api/admin/mobile-templates?${params.toString()}`, {
    headers: authHeaders(accessToken)
  });

  return readJson<PagedResult<MobileTemplateSummary>>(response);
}

export async function getMobileTemplate(accessToken: string, id: string): Promise<MobileTemplateDetail> {
  const response = await fetch(`/api/admin/mobile-templates/${encodeURIComponent(id)}`, {
    headers: authHeaders(accessToken)
  });

  return readJson<MobileTemplateDetail>(response);
}

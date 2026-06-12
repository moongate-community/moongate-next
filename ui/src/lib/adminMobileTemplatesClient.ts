import { readJson } from "./authClient";
import type {
  MobileTemplateDetail,
  MobileTemplateFilters,
  MobileTemplateSummary,
  PagedResult
} from "../types/mobileTemplates";
import type { MobileTemplateEditRequest, MobileTemplateSaveResult } from "./mobileTemplateFormModel";

export type { MobileTemplateSaveResult };

function authHeaders(accessToken: string, json = false): HeadersInit {
  const headers: Record<string, string> = { Authorization: `Bearer ${accessToken}` };

  if (json) {
    headers["Content-Type"] = "application/json";
  }

  return headers;
}

async function readWriteJson<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const body = await response.text();
    throw new Error(body.trim() || `${response.status} ${response.statusText}`);
  }

  return (await response.json()) as T;
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

export async function createMobileTemplate(
  accessToken: string,
  request: MobileTemplateEditRequest
): Promise<MobileTemplateSaveResult> {
  const response = await fetch("/api/admin/mobile-templates", {
    method: "POST",
    headers: authHeaders(accessToken, true),
    body: JSON.stringify(request)
  });

  return readWriteJson<MobileTemplateSaveResult>(response);
}

export async function updateMobileTemplate(
  accessToken: string,
  id: string,
  request: MobileTemplateEditRequest
): Promise<MobileTemplateSaveResult> {
  const response = await fetch(`/api/admin/mobile-templates/${encodeURIComponent(id)}`, {
    method: "PUT",
    headers: authHeaders(accessToken, true),
    body: JSON.stringify(request)
  });

  return readWriteJson<MobileTemplateSaveResult>(response);
}

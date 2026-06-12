import { readJson } from "./authClient";
import type { HueSummary, PagedResult } from "../types/itemTemplates";

function authHeaders(accessToken: string): HeadersInit {
  return { Authorization: `Bearer ${accessToken}` };
}

export async function listHues(
  accessToken: string,
  page: number,
  search: string
): Promise<PagedResult<HueSummary>> {
  const params = new URLSearchParams({ page: String(page), pageSize: "60" });

  if (search) {
    params.set("search", search);
  }

  const response = await fetch(`/api/admin/hues?${params.toString()}`, {
    headers: authHeaders(accessToken)
  });

  return readJson<PagedResult<HueSummary>>(response);
}

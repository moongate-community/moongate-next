import { readJson } from "./authClient";
import type { HueSummary, PagedResult } from "../types/itemTemplates";

function authHeaders(accessToken: string): HeadersInit {
  return { Authorization: `Bearer ${accessToken}` };
}

export async function getHue(accessToken: string, value: number): Promise<HueSummary> {
  const response = await fetch(`/api/admin/hues/${value}`, {
    headers: authHeaders(accessToken)
  });

  return readJson<HueSummary>(response);
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

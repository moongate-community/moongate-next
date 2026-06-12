import { readJson } from "./authClient";
import type { PagedResult } from "../types/itemTemplates";

export type BodySummary = {
  body: number;
  bodyHex: string;
  bodyType: string;
  imageUrl: string;
};

function authHeaders(accessToken: string): HeadersInit {
  return { Authorization: `Bearer ${accessToken}` };
}

export async function listBodies(
  accessToken: string,
  page: number,
  search: string
): Promise<PagedResult<BodySummary>> {
  const params = new URLSearchParams({ page: String(page), pageSize: "60" });

  if (search) {
    params.set("search", search);
  }

  const response = await fetch(`/api/admin/bodies?${params.toString()}`, {
    headers: authHeaders(accessToken)
  });

  return readJson<PagedResult<BodySummary>>(response);
}

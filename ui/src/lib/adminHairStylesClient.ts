import { readJson } from "./authClient";
import type { PagedResult } from "../types/itemTemplates";

export type HairStyleSummary = {
  style: number;
  styleHex: string;
  name: string;
  isFacial: boolean;
  imageUrl: string;
};

function authHeaders(accessToken: string): HeadersInit {
  return { Authorization: `Bearer ${accessToken}` };
}

export async function listHairStyles(
  accessToken: string,
  _page: number,
  search: string,
  facial: boolean
): Promise<PagedResult<HairStyleSummary>> {
  const params = new URLSearchParams({ facial: String(facial) });

  if (search) {
    params.set("search", search);
  }

  const response = await fetch(`/api/admin/hair-styles?${params.toString()}`, {
    headers: authHeaders(accessToken)
  });

  return readJson<PagedResult<HairStyleSummary>>(response);
}

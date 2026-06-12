import type { PagedResult } from "../types/itemTemplates";

export type BodySummary = {
  body: number;
  bodyHex: string;
  bodyType: string;
  imageUrl: string;
};

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
    headers: { Authorization: `Bearer ${accessToken}` }
  });

  if (!response.ok) {
    throw new Error(`Failed to load bodies (${response.status})`);
  }

  return (await response.json()) as PagedResult<BodySummary>;
}

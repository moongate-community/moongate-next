import type { PagedResult } from "./itemTemplates";

export type { PagedResult };

export type LootTemplateSummary = {
  id: string;
  rootNodeCount: number;
};

export type LootTemplateNodeSummary = {
  id: string;
  parentId: string;
  depth: number;
  kind: "item" | "category" | "category_candidate" | "pick_one_of" | "group";
  label: string;
  rarity: string | null;
  chance: number;
  weight: number;
  amountMin: number;
  amountMax: number;
  itemTemplateId: string | null;
  itemIdHex: string | null;
  imageUrl: string | null;
  stackable: boolean;
};

export type LootTemplateDetail = {
  id: string;
  rootNodeCount: number;
  nodes: LootTemplateNodeSummary[];
  potentialItems?: LootTemplateNodeSummary[];
  previewItems: LootTemplateNodeSummary[];
};

export type LootTemplateFilters = {
  page: number;
  pageSize: number;
  search: string;
};

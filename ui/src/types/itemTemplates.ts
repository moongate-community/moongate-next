export type HueColorSummary = {
  index: number;
  r: number;
  g: number;
  b: number;
  hex: string;
};

export type HueSummary = {
  value: number;
  hex: string;
  name: string;
  isNone: boolean;
  isKnown: boolean;
  colors: HueColorSummary[];
};

export type ItemTemplateSummary = {
  id: string;
  name: string;
  itemId: number;
  itemIdHex: string;
  imageUrl: string;
  rarity: string;
  layer: string | null;
  tags: string[];
  isAbstract: boolean;
  hue: HueSummary;
};

export type ItemTemplateParamSummary = {
  key: string;
  type: string;
  value: string;
};

export type ItemTemplateDetail = ItemTemplateSummary & {
  comment: string;
  baseItem: string | null;
  scriptId: string;
  visibility: string;
  amount: number;
  weight: number;
  isStackable: boolean;
  isMovable: boolean;
  gumpId: number | null;
  params: ItemTemplateParamSummary[];
};

export type ItemTemplateFilters = {
  page: number;
  pageSize: number;
  search: string;
  tag: string;
  rarity: string;
  layer: string;
  abstract: "all" | "true" | "false";
};

export type PagedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

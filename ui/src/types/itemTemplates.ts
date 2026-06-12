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
  graphicVariants: ItemTemplateGraphicVariantSummary[];
  rarity: string;
  layer: string | null;
  tags: string[];
  isAbstract: boolean;
  value: ItemTemplateValueSummary | null;
  hue: HueSummary;
};

export type ItemTemplateGraphicVariantSummary = {
  itemId: number;
  itemIdHex: string;
  imageUrl: string;
};

export type ItemTemplateValueSummary = {
  buy: number;
  sell: number;
  rarityMultiplier: number;
  effectiveBuy: number;
  effectiveSell: number;
};

export type ItemTemplateValueEdit = {
  buy: number;
  sell: number;
};

export type ItemTemplateParamSummary = {
  key: string;
  type: string;
  value: string;
};

export type ItemTemplateParamEdit = {
  type: string;
  value: string;
};

export type ItemTemplateContentsSummary = {
  lootTemplate: string;
  generate: string;
  refillEvery: string | null;
  refillPolicy: string;
  refillScope: string;
};

export type ItemTemplateContentsEdit = {
  lootTemplate: string;
  generate: string;
  refillEvery: string | null;
  refillPolicy: string;
  refillScope: string;
};

export type ItemTemplateGraphicVariantEdit = {
  itemId: number;
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
  contents: ItemTemplateContentsSummary | null;
  params: ItemTemplateParamSummary[];
};

export type ItemTemplateEditRequest = {
  id: string;
  baseItem: string | null;
  isAbstract: boolean;
  name: string;
  comment: string;
  itemId: number;
  graphicVariants: ItemTemplateGraphicVariantEdit[];
  hue: number;
  weight: number;
  amount: number;
  isStackable: boolean;
  isMovable: boolean;
  gumpId: number | null;
  layer: string | null;
  scriptId: string;
  rarity: string;
  value: ItemTemplateValueEdit | null;
  contents: ItemTemplateContentsEdit | null;
  visibility: string;
  tags: string[];
  params: Record<string, ItemTemplateParamEdit>;
};

export type ItemTemplateSaveResult = {
  template: ItemTemplateDetail;
  sourceFile: string;
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

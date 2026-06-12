import type { ItemTemplateDetail, ItemTemplateEditRequest } from "../types/itemTemplates";
import type { UoItemSummary } from "../types/uoItems";

export type ItemTemplateParamRow = {
  key: string;
  type: string;
  value: string;
};

export type ItemTemplateGraphicVariantRow = {
  itemId: string;
};

export type ItemTemplateFormState = {
  id: string;
  baseItem: string;
  isAbstract: boolean;
  name: string;
  comment: string;
  itemId: string;
  graphicVariants: ItemTemplateGraphicVariantRow[];
  hue: string;
  weight: string;
  amount: string;
  isStackable: boolean;
  isMovable: boolean;
  gumpId: string;
  layer: string;
  scriptId: string;
  rarity: string;
  hasValue: boolean;
  valueBuy: string;
  valueSell: string;
  hasContents: boolean;
  lootTemplate: string;
  generate: string;
  refillEvery: string;
  refillPolicy: string;
  refillScope: string;
  visibility: string;
  tagsText: string;
  params: ItemTemplateParamRow[];
};

export const itemTemplateRarityOptions = ["None", "Common", "Uncommon", "Rare", "Epic", "Legendary"];

export const itemTemplateLayerOptions = [
  "OneHanded",
  "TwoHanded",
  "Shoes",
  "Pants",
  "Shirt",
  "Helm",
  "Gloves",
  "Ring",
  "Talisman",
  "Neck",
  "Hair",
  "Waist",
  "InnerTorso",
  "Bracelet",
  "Unused_xF",
  "FacialHair",
  "MiddleTorso",
  "Earrings",
  "Arms",
  "Cloak",
  "Backpack",
  "OuterTorso",
  "OuterLegs",
  "InnerLegs",
  "Mount",
  "ShopBuy",
  "ShopResale",
  "ShopSell",
  "Bank"
];

export const itemTemplateVisibilityOptions = ["Player", "GameMaster", "Administrator"];
export const itemTemplateParamTypeOptions = ["String", "Integer", "Hue", "Serial"];
export const itemTemplateGenerateOptions = ["OnOpen"];
export const itemTemplateRefillPolicyOptions = ["WhenEmpty"];
export const itemTemplateRefillScopeOptions = ["WorldOnly"];

export function createEmptyItemTemplateFormState(): ItemTemplateFormState {
  return {
    id: "",
    baseItem: "",
    isAbstract: false,
    name: "",
    comment: "",
    itemId: "0",
    graphicVariants: [],
    hue: "0",
    weight: "0",
    amount: "1",
    isStackable: false,
    isMovable: true,
    gumpId: "",
    layer: "none",
    scriptId: "",
    rarity: "Common",
    hasValue: false,
    valueBuy: "",
    valueSell: "",
    hasContents: false,
    lootTemplate: "",
    generate: "OnOpen",
    refillEvery: "",
    refillPolicy: "WhenEmpty",
    refillScope: "WorldOnly",
    visibility: "Player",
    tagsText: "",
    params: []
  };
}

export function createItemTemplateFormStateFromDetail(template: ItemTemplateDetail): ItemTemplateFormState {
  return {
    id: template.id,
    baseItem: template.baseItem ?? "",
    isAbstract: template.isAbstract,
    name: template.name,
    comment: template.comment,
    itemId: template.itemIdHex,
    graphicVariants: template.graphicVariants.map((variant) => ({ itemId: variant.itemIdHex })),
    hue: String(template.hue.value),
    weight: String(template.weight),
    amount: String(template.amount),
    isStackable: template.isStackable,
    isMovable: template.isMovable,
    gumpId: template.gumpId?.toString() ?? "",
    layer: template.layer ?? "none",
    scriptId: template.scriptId,
    rarity: template.rarity,
    hasValue: template.value !== null,
    valueBuy: template.value?.buy.toString() ?? "",
    valueSell: template.value?.sell.toString() ?? "",
    hasContents: template.contents !== null,
    lootTemplate: template.contents?.lootTemplate ?? "",
    generate: template.contents?.generate ?? "OnOpen",
    refillEvery: template.contents?.refillEvery ?? "",
    refillPolicy: template.contents?.refillPolicy ?? "WhenEmpty",
    refillScope: template.contents?.refillScope ?? "WorldOnly",
    visibility: template.visibility,
    tagsText: template.tags.join(", "),
    params: template.params.map((param) => ({ key: param.key, type: param.type, value: param.value }))
  };
}

export function applyUoItemToItemTemplateFormState(
  state: ItemTemplateFormState,
  item: UoItemSummary
): ItemTemplateFormState {
  const inferredName = item.name.trim();
  const hasValue = item.value > 0;

  return {
    ...state,
    id: state.id.trim() || slugify(inferredName || `item_${item.itemIdHex.slice(2)}`),
    name: state.name.trim() || inferredName,
    itemId: item.itemIdHex,
    weight: state.weight === "0" ? String(item.weight) : state.weight,
    hasValue: state.hasValue || hasValue,
    valueBuy: state.valueBuy || (hasValue ? String(item.value) : ""),
    valueSell: state.valueSell || (hasValue ? String(Math.max(1, Math.floor(item.value / 2))) : "")
  };
}

export function toItemTemplateEditRequest(state: ItemTemplateFormState): ItemTemplateEditRequest {
  return {
    id: state.id.trim(),
    baseItem: textOrNull(state.baseItem),
    isAbstract: state.isAbstract,
    name: state.name.trim(),
    comment: state.comment.trim(),
    itemId: parseInteger(state.itemId, 0),
    graphicVariants: state.graphicVariants
      .map((variant) => parseInteger(variant.itemId, 0))
      .filter((itemId) => itemId > 0)
      .map((itemId) => ({ itemId })),
    hue: parseInteger(state.hue, 0),
    weight: parseInteger(state.weight, 0),
    amount: parseInteger(state.amount, 1),
    isStackable: state.isStackable,
    isMovable: state.isMovable,
    gumpId: textOrNull(state.gumpId) === null ? null : parseInteger(state.gumpId, 0),
    layer: state.layer === "none" ? null : state.layer,
    scriptId: state.scriptId.trim(),
    rarity: state.rarity,
    value: state.hasValue
      ? {
          buy: parseInteger(state.valueBuy, 0),
          sell: parseInteger(state.valueSell, 0)
        }
      : null,
    contents: state.hasContents && state.lootTemplate.trim().length > 0
      ? {
          lootTemplate: state.lootTemplate.trim(),
          generate: state.generate,
          refillEvery: textOrNull(state.refillEvery),
          refillPolicy: state.refillPolicy,
          refillScope: state.refillScope
        }
      : null,
    visibility: state.visibility,
    tags: splitTags(state.tagsText),
    params: paramsToRecord(state.params)
  };
}

function paramsToRecord(rows: ItemTemplateParamRow[]): ItemTemplateEditRequest["params"] {
  const params: ItemTemplateEditRequest["params"] = {};

  for (const row of rows) {
    const key = row.key.trim();

    if (key.length === 0) {
      continue;
    }

    params[key] = {
      type: row.type,
      value: row.value
    };
  }

  return params;
}

function parseInteger(value: string, fallback: number): number {
  const trimmed = value.trim();

  if (trimmed.length === 0) {
    return fallback;
  }

  const parsed = Number(trimmed);

  return Number.isFinite(parsed) ? Math.trunc(parsed) : fallback;
}

function splitTags(value: string): string[] {
  return value
    .split(/[,\n;]/)
    .map((tag) => tag.trim())
    .filter((tag, index, tags) => tag.length > 0 && tags.findIndex((candidate) => candidate.toLowerCase() === tag.toLowerCase()) === index);
}

function textOrNull(value: string): string | null {
  const trimmed = value.trim();

  return trimmed.length === 0 ? null : trimmed;
}

function slugify(value: string): string {
  const normalized = value
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, "_")
    .replace(/^_+|_+$/g, "");

  return normalized.length > 0 ? normalized : "item_template";
}

import type { PagedResult } from "./itemTemplates";

export type { PagedResult };

export type MobileTemplateSummary = {
  id: string;
  name: string;
  title: string;
  body: number;
  bodyHex: string;
  imageUrl: string;
  gender: string;
  notoriety: string;
  karma: number;
  fame: number;
  factionId: string;
  brain: string;
  isAbstract: boolean;
  tags: string[];
  equipmentCount: number;
  lootTablesCount: number;
};

export type MobileStatsSummary = { strength: number; dexterity: number; intelligence: number };
export type MobileResourcesSummary = { hits: number; mana: number; stamina: number };
export type MobileResistancesSummary = {
  physical: number;
  fire: number;
  cold: number;
  poison: number;
  energy: number;
};
export type MobileSkillSummary = { name: string; value: number };
export type MobileTemplateParamSummary = { key: string; type: string; value: string };

export type MobileTemplateDetail = MobileTemplateSummary & {
  baseMobile: string | null;
  raceIndex: number;
  skinHue: number;
  hairHue: number;
  hairStyle: number;
  facialHairHue: number;
  facialHairStyle: number;
  stats: MobileStatsSummary | null;
  resources: MobileResourcesSummary | null;
  resistances: MobileResistancesSummary | null;
  skills: MobileSkillSummary[];
  equipment: string[];
  backpackTemplate: string | null;
  lootTables: string[];
  params: MobileTemplateParamSummary[];
};

export type MobileTemplateFilters = {
  page: number;
  pageSize: number;
  search: string;
  tag: string;
  notoriety: string;
  abstract: "all" | "true" | "false";
};

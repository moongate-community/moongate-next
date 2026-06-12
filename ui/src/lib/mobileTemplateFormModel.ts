import type { MobileTemplateDetail } from "../types/mobileTemplates";

export type MobileTemplateParamRow = {
  key: string;
  type: string;
  value: string;
};

export type MobileTemplateSkillRow = {
  name: string;
  value: number;
};

export type MobileTemplateFormState = {
  id: string;
  baseMobile: string;
  isAbstract: boolean;
  name: string;
  title: string;
  body: string;
  gender: "Male" | "Female";
  raceIndex: string;
  skinHue: string;
  hairHue: string;
  hairStyle: string;
  facialHairHue: string;
  facialHairStyle: string;
  brain: string;
  notoriety: MobileTemplateNotoriety;
  karma: string;
  fame: string;
  factionId: string;
  stats: {
    strength: string;
    dexterity: string;
    intelligence: string;
  };
  resources: {
    hits: string;
    mana: string;
    stamina: string;
  };
  resistances: {
    physical: string;
    fire: string;
    cold: string;
    poison: string;
    energy: string;
  };
  skills: MobileTemplateSkillRow[];
  equipment: string[];
  backpackTemplate: string;
  lootTables: string[];
  tags: string[];
  params: MobileTemplateParamRow[];
};

export type MobileTemplateNotoriety =
  | "Invalid"
  | "Innocent"
  | "Friend"
  | "CanBeAttacked"
  | "Criminal"
  | "Enemy"
  | "Murdered"
  | "Invulnerable";

export type MobileTemplateEditRequest = {
  id: string;
  baseMobile: string | null;
  isAbstract: boolean;
  name: string;
  title: string;
  body: number;
  gender: string;
  raceIndex: number;
  skinHue: number;
  hairHue: number;
  hairStyle: number;
  facialHairHue: number;
  facialHairStyle: number;
  brain: string;
  notoriety: string;
  karma: number;
  fame: number;
  factionId: string | null;
  stats: {
    strength: number;
    dexterity: number;
    intelligence: number;
  };
  resources: {
    hits: number;
    mana: number;
    stamina: number;
  };
  resistances: {
    physical: number;
    fire: number;
    cold: number;
    poison: number;
    energy: number;
  };
  skills: Record<string, number>;
  equipment: string[];
  backpackTemplate: string | null;
  lootTables: string[];
  tags: string[];
  params: Record<string, { type: string; value: string }>;
};

export type MobileTemplateSaveResult = {
  template: MobileTemplateDetail;
  sourceFile: string;
};

export const mobileTemplateGenderOptions: string[] = ["Male", "Female"];

export type LabeledOption = { value: string; label: string };

export const mobileTemplateRaceOptions: LabeledOption[] = [
  { value: "0", label: "Human" },
  { value: "1", label: "Elf" },
  { value: "2", label: "Gargoyle" }
];

export const mobileTemplateNotorietyOptions: MobileTemplateNotoriety[] = [
  "Invalid",
  "Innocent",
  "Friend",
  "CanBeAttacked",
  "Criminal",
  "Enemy",
  "Murdered",
  "Invulnerable"
];

export const mobileTemplateParamTypeOptions = ["String", "Integer", "Hue", "Serial"];

export function emptyMobileTemplateFormState(): MobileTemplateFormState {
  return {
    id: "",
    baseMobile: "",
    isAbstract: false,
    name: "",
    title: "",
    body: "0",
    gender: "Male",
    raceIndex: "0",
    skinHue: "0",
    hairHue: "0",
    hairStyle: "0",
    facialHairHue: "0",
    facialHairStyle: "0",
    brain: "",
    notoriety: "Innocent",
    karma: "0",
    fame: "0",
    factionId: "",
    stats: {
      strength: "0",
      dexterity: "0",
      intelligence: "0"
    },
    resources: {
      hits: "0",
      mana: "0",
      stamina: "0"
    },
    resistances: {
      physical: "0",
      fire: "0",
      cold: "0",
      poison: "0",
      energy: "0"
    },
    skills: [],
    equipment: [],
    backpackTemplate: "",
    lootTables: [],
    tags: [],
    params: []
  };
}

export function mobileTemplateFormStateFromDetail(detail: MobileTemplateDetail): MobileTemplateFormState {
  return {
    id: detail.id,
    baseMobile: detail.baseMobile ?? "",
    isAbstract: detail.isAbstract,
    name: detail.name,
    title: detail.title,
    body: String(detail.body),
    gender: detail.gender === "Female" ? "Female" : "Male",
    raceIndex: String(detail.raceIndex),
    skinHue: String(detail.skinHue),
    hairHue: String(detail.hairHue),
    hairStyle: String(detail.hairStyle),
    facialHairHue: String(detail.facialHairHue),
    facialHairStyle: String(detail.facialHairStyle),
    brain: detail.brain,
    notoriety: asMobileNotoriety(detail.notoriety),
    karma: String(detail.karma),
    fame: String(detail.fame),
    factionId: detail.factionId ?? "",
    stats: {
      strength: String(detail.stats?.strength ?? 0),
      dexterity: String(detail.stats?.dexterity ?? 0),
      intelligence: String(detail.stats?.intelligence ?? 0)
    },
    resources: {
      hits: String(detail.resources?.hits ?? 0),
      mana: String(detail.resources?.mana ?? 0),
      stamina: String(detail.resources?.stamina ?? 0)
    },
    resistances: {
      physical: String(detail.resistances?.physical ?? 0),
      fire: String(detail.resistances?.fire ?? 0),
      cold: String(detail.resistances?.cold ?? 0),
      poison: String(detail.resistances?.poison ?? 0),
      energy: String(detail.resistances?.energy ?? 0)
    },
    skills: detail.skills.map((s) => ({ name: s.name, value: s.value })),
    equipment: [...detail.equipment],
    backpackTemplate: detail.backpackTemplate ?? "",
    lootTables: [...detail.lootTables],
    tags: [...detail.tags],
    params: detail.params.map((p) => ({ key: p.key, type: p.type, value: p.value }))
  };
}

export function mobileTemplateEditRequestFromState(state: MobileTemplateFormState): MobileTemplateEditRequest {
  return {
    id: state.id.trim(),
    baseMobile: textOrNull(state.baseMobile),
    isAbstract: state.isAbstract,
    name: state.name.trim(),
    title: state.title.trim(),
    body: parseInteger(state.body, 0),
    gender: state.gender,
    raceIndex: parseInteger(state.raceIndex, 0),
    skinHue: parseInteger(state.skinHue, 0),
    hairHue: parseInteger(state.hairHue, 0),
    hairStyle: parseInteger(state.hairStyle, 0),
    facialHairHue: parseInteger(state.facialHairHue, 0),
    facialHairStyle: parseInteger(state.facialHairStyle, 0),
    brain: state.brain.trim(),
    notoriety: state.notoriety,
    karma: parseInteger(state.karma, 0),
    fame: parseInteger(state.fame, 0),
    factionId: textOrNull(state.factionId),
    stats: {
      strength: parseInteger(state.stats.strength, 0),
      dexterity: parseInteger(state.stats.dexterity, 0),
      intelligence: parseInteger(state.stats.intelligence, 0)
    },
    resources: {
      hits: parseInteger(state.resources.hits, 0),
      mana: parseInteger(state.resources.mana, 0),
      stamina: parseInteger(state.resources.stamina, 0)
    },
    resistances: {
      physical: parseInteger(state.resistances.physical, 0),
      fire: parseInteger(state.resistances.fire, 0),
      cold: parseInteger(state.resistances.cold, 0),
      poison: parseInteger(state.resistances.poison, 0),
      energy: parseInteger(state.resistances.energy, 0)
    },
    skills: skillsToRecord(state.skills),
    equipment: state.equipment.map((id) => id.trim()).filter((id) => id.length > 0),
    backpackTemplate: textOrNull(state.backpackTemplate),
    lootTables: state.lootTables.map((id) => id.trim()).filter((id) => id.length > 0),
    tags: state.tags.map((t) => t.trim()).filter((t) => t.length > 0),
    params: paramsToRecord(state.params)
  };
}

function skillsToRecord(rows: MobileTemplateSkillRow[]): Record<string, number> {
  const skills: Record<string, number> = {};

  for (const row of rows) {
    const name = row.name.trim();

    if (name.length === 0) {
      continue;
    }

    skills[name] = row.value;
  }

  return skills;
}

function paramsToRecord(rows: MobileTemplateParamRow[]): Record<string, { type: string; value: string }> {
  const params: Record<string, { type: string; value: string }> = {};

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

function textOrNull(value: string): string | null {
  const trimmed = value.trim();

  return trimmed.length === 0 ? null : trimmed;
}

function asMobileNotoriety(value: string): MobileTemplateNotoriety {
  const valid: MobileTemplateNotoriety[] = [
    "Invalid",
    "Innocent",
    "Friend",
    "CanBeAttacked",
    "Criminal",
    "Enemy",
    "Murdered",
    "Invulnerable"
  ];

  return (valid.includes(value as MobileTemplateNotoriety) ? value : "Innocent") as MobileTemplateNotoriety;
}

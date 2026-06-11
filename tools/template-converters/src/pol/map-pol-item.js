"use strict";

const {
  cleanDisplayName,
  normalizeTemplateId,
  parseBoolean,
  parseInteger,
  typedParam,
  uniqueTags
} = require("../normalize");

const MAPPED_KEYS = new Set([
  "name",
  "desc",
  "graphic",
  "color",
  "vendorsellsfor",
  "vendorbuysfor",
  "weight",
  "movable",
  "stackable",
  "pileable",
  "layer",
  "twohanded"
]);

const COVERAGE_TO_LAYER = {
  head: "Helm",
  neck: "Neck",
  arms: "Arms",
  hands: "Gloves",
  legs: "OuterLegs",
  feet: "Shoes",
  body: "MiddleTorso",
  torso: "MiddleTorso",
  chest: "MiddleTorso"
};

function mapPolItem(block, options, report) {
  const get = (key) => first(block.properties, key);
  const sourceName = get("name");
  const desc = get("desc");
  const itemId = parseInteger(get("graphic")) ?? parseInteger(block.sourceId) ?? 0;
  const template = {
    source: "pol",
    sourcePath: block.sourcePath,
    sourceKind: block.sourceKind,
    sourceId: block.sourceId,
    idCandidate: sourceName || `${block.sourceKind}_${block.sourceId}`,
    name: cleanDisplayName(desc || sourceName, sourceName || block.sourceId),
    comment: `Converted from POL ${block.sourcePath} ${block.sourceKind} ${block.sourceId}`,
    item_id: itemId,
    tags: uniqueTags([...pathTags(block.sourcePath), block.sourceKind, ...options.tags])
  };

  setNumber(template, "hue", parseInteger(get("color")), 0);
  setWeight(template, get("weight"));
  setNumber(template, "item_id", itemId, 0);

  const movable = parseBoolean(get("movable"));
  if (movable !== null) {
    template.is_movable = movable;
  }

  const stackable = parseBoolean(get("stackable")) ?? parseBoolean(get("pileable"));
  if (stackable !== null) {
    template.is_stackable = stackable;
  } else if (isKnownStackable(sourceName, desc)) {
    template.is_stackable = true;
  }

  const layer = mapLayer(block, get);
  if (layer) {
    template.layer = layer;
  }

  const buy = parseInteger(get("vendorsellsfor"));
  const sell = parseInteger(get("vendorbuysfor"));
  if (buy !== null) {
    template.value = { buy };
    if (sell !== null) {
      template.value.sell = sell;
    }
  }

  const unmapped = collectUnmapped(block.properties);
  if (options.includeSourceParams && Object.keys(unmapped).length > 0) {
    template.params = {};
    for (const [key, values] of Object.entries(unmapped)) {
      template.params[normalizeTemplateId(key)] = typedParam(values.join(" | "));
    }
  }

  template._reportDetails = {
    unmapped,
    lossy: lossyDetails(block, template)
  };

  return template;
}

function first(properties, key) {
  return properties.get(key.toLowerCase())?.values[0];
}

function setNumber(template, key, value, defaultValue) {
  if (value !== null && value !== defaultValue) {
    template[key] = value;
  }
}

function setWeight(template, value) {
  if (!value) {
    return;
  }

  const parsed = parseWeight(value);
  if (parsed === null) {
    return;
  }

  template.weight = Math.max(1, Math.round(parsed));
}

function parseWeight(value) {
  const text = String(value).trim();
  const fraction = text.match(/^(\d+)\/(\d+)$/);
  if (fraction) {
    const numerator = Number.parseInt(fraction[1], 10);
    const denominator = Number.parseInt(fraction[2], 10);
    return denominator === 0 ? null : numerator / denominator;
  }

  const parsed = Number.parseFloat(text);
  return Number.isFinite(parsed) ? parsed : null;
}

function mapLayer(block, get) {
  const explicit = get("layer");
  if (explicit) {
    const numeric = parseInteger(explicit);
    if (numeric !== null) {
      return numericLayer(numeric);
    }

    return normalizeLayerName(explicit);
  }

  if (parseBoolean(get("twohanded")) === true) {
    return "TwoHanded";
  }

  if (block.sourceKind === "Weapon") {
    return "OneHanded";
  }

  const coverage = get("coverage");
  if (coverage) {
    return COVERAGE_TO_LAYER[coverage.toLowerCase()] || null;
  }

  return null;
}

function normalizeLayerName(value) {
  const normalized = String(value).replace(/[^a-z0-9]+/gi, "").toLowerCase();
  const known = {
    onehanded: "OneHanded",
    twohanded: "TwoHanded",
    shoes: "Shoes",
    pants: "Pants",
    shirt: "Shirt",
    helm: "Helm",
    gloves: "Gloves",
    ring: "Ring",
    neck: "Neck",
    arms: "Arms",
    cloak: "Cloak",
    backpack: "Backpack",
    bank: "Bank"
  };
  return known[normalized] || null;
}

function numericLayer(value) {
  const layers = {
    1: "OneHanded",
    2: "TwoHanded",
    3: "Shoes",
    4: "Pants",
    5: "Shirt",
    6: "Helm",
    7: "Gloves",
    8: "Ring",
    10: "Neck",
    13: "InnerTorso",
    14: "Bracelet",
    17: "MiddleTorso",
    19: "Arms",
    20: "Cloak",
    21: "Backpack",
    22: "OuterTorso",
    23: "OuterLegs",
    24: "InnerLegs",
    29: "Bank"
  };
  return layers[value] || null;
}

function collectUnmapped(properties) {
  const result = {};
  for (const [key, entry] of properties.entries()) {
    if (!MAPPED_KEYS.has(key)) {
      result[entry.key] = [...entry.values];
    }
  }

  return result;
}

function lossyDetails(block, template) {
  const weight = first(block.properties, "weight");
  const parsedWeight = weight ? parseWeight(weight) : null;
  return parsedWeight !== null && parsedWeight !== template.weight
    ? { sourceWeight: weight, emittedWeight: template.weight }
    : {};
}

function pathTags(sourcePath) {
  return sourcePath.split("/").filter((part) => part && !["config", "itemdesc.cfg"].includes(part.toLowerCase()));
}

function isKnownStackable(name, desc) {
  const text = `${name || ""} ${desc || ""}`.toLowerCase();
  return /\b(gold|coins|arrows|bolts|reagent|reg|kindling)\b/.test(text);
}

module.exports = { mapPolItem, parseWeight };

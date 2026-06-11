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
  "get",
  "gett2a",
  "getlbr",
  "getaos",
  "gettol",
  "id",
  "name",
  "weight",
  "value",
  "pileable",
  "movable",
  "layer"
]);

function mapUox3Item(section, options, report) {
  const get = (key) => first(section.properties, key);
  const itemId = parseInteger(get("id")) ?? parseInteger(section.sourceId);
  if (itemId === null) {
    return null;
  }

  const sourceName = get("name");
  const template = {
    source: "uox3",
    sourcePath: section.sourcePath,
    sourceKind: section.sourceKind,
    sourceId: section.sourceId,
    idCandidate: sourceName || `item_${section.sourceId}`,
    name: cleanDisplayName(sourceName, section.sourceId),
    comment: `Converted from UOX3 ${section.sourcePath} ${section.sourceId} using ${options.ruleset} ruleset`,
    item_id: itemId,
    tags: uniqueTags([...pathTags(section.sourcePath), "uox3", ...options.tags])
  };

  const weight = parseInteger(get("weight"));
  if (weight !== null) {
    template.weight = Math.max(1, Math.round(weight / 100));
  }

  const movable = parseBoolean(get("movable"));
  if (movable !== null) {
    template.is_movable = movable;
  }

  const pileable = parseBoolean(get("pileable"));
  if (pileable !== null) {
    template.is_stackable = pileable;
  }

  const layer = parseInteger(get("layer"));
  const layerName = layer !== null ? numericLayer(layer) : null;
  if (layerName) {
    template.layer = layerName;
  }

  const value = get("value");
  if (value) {
    const [buy, sell] = value.split(/\s+/).map((part) => parseInteger(part));
    if (buy !== null) {
      template.value = { buy };
      if (sell !== null) {
        template.value.sell = sell;
      }
    }
  }

  const unmapped = collectUnmapped(section.properties);
  if (options.includeSourceParams && Object.keys(unmapped).length > 0) {
    template.params = {};
    for (const [key, values] of Object.entries(unmapped)) {
      template.params[normalizeTemplateId(key)] = typedParam(values.join(" | "));
    }
  }

  template._reportDetails = {
    unmapped,
    lossy: weight !== null && template.weight !== weight
      ? { sourceWeight: weight, emittedWeight: template.weight }
      : {}
  };

  return template;
}

function first(properties, key) {
  return properties.get(key.toLowerCase())?.values[0];
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
    9: "Talisman",
    10: "Neck",
    11: "Hair",
    12: "Waist",
    13: "InnerTorso",
    14: "Bracelet",
    16: "FacialHair",
    17: "MiddleTorso",
    18: "Earrings",
    19: "Arms",
    20: "Cloak",
    21: "Backpack",
    22: "OuterTorso",
    23: "OuterLegs",
    24: "InnerLegs",
    25: "Mount",
    26: "ShopBuy",
    27: "ShopResale",
    28: "ShopSell",
    29: "Bank"
  };
  return layers[value] || null;
}

function pathTags(sourcePath) {
  return sourcePath.split("/").filter((part) => part && !["items"].includes(part.toLowerCase()));
}

module.exports = { mapUox3Item };

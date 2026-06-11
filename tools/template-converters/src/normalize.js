"use strict";

function createReport(options, files) {
  return {
    source: options.source,
    ruleset: options.source === "uox3" ? options.ruleset : null,
    input: options.input,
    files,
    output: {
      templateCount: 0
    },
    collisions: [],
    mergedGraphicVariants: [],
    unresolvedReferences: [],
    cycles: [],
    items: []
  };
}

function normalizeTemplateId(value, fallback) {
  const source = String(value || fallback || "item").trim();
  const spaced = source
    .replace(/([a-z0-9])([A-Z])/g, "$1_$2")
    .replace(/%[^%]*%/g, " ")
    .replace(/%/g, " ");
  const id = spaced
    .toLowerCase()
    .replace(/0x([0-9a-f]+)/gi, "$1")
    .replace(/[^a-z0-9]+/g, "_")
    .replace(/^_+|_+$/g, "")
    .replace(/_+/g, "_");

  return id || "item";
}

function assignTemplateIds(templates, report) {
  const counts = new Map();

  for (const template of templates) {
    const base = normalizeTemplateId(template.idCandidate, `item_${template.item_id || template.itemId || template.sourceId}`);
    const count = counts.get(base) || 0;
    counts.set(base, count + 1);
    template.id = count === 0 ? base : `${base}_${count + 1}`;

    if (count > 0) {
      report.collisions.push({
        id: base,
        assigned: template.id,
        source: template.source,
        sourcePath: template.sourcePath,
        sourceId: template.sourceId
      });
    }

    delete template.idCandidate;
  }
}

function mergeGraphicVariants(templates, report) {
  const groups = new Map();
  const merged = [];

  for (const template of templates) {
    const base = normalizeTemplateId(template.idCandidate, `item_${template.item_id || template.itemId || template.sourceId}`);
    const key = `${base}\0${semanticTemplateSignature(template)}`;
    const primary = groups.get(key);

    if (!primary || primary.item_id === template.item_id || hasGraphicVariant(primary, template.item_id)) {
      groups.set(key, primary || template);
      merged.push(template);
      continue;
    }

    primary.graphic_variants ||= [];
    primary.graphic_variants.push({ item_id: template.item_id });
    report.mergedGraphicVariants.push({
      id: base,
      itemId: template.item_id,
      source: template.source,
      sourcePath: template.sourcePath,
      sourceId: template.sourceId,
      mergedIntoSourcePath: primary.sourcePath,
      mergedIntoSourceId: primary.sourceId
    });
  }

  return merged;
}

function hasGraphicVariant(template, itemId) {
  return (template.graphic_variants || []).some((variant) => variant.item_id === itemId);
}

function semanticTemplateSignature(template) {
  const semantic = {};
  for (const key of Object.keys(template).sort((left, right) => left.localeCompare(right, "en"))) {
    if (isGraphicVariantIgnoredKey(key)) {
      continue;
    }

    semantic[key] = template[key];
  }

  return stableStringify(semantic);
}

function isGraphicVariantIgnoredKey(key) {
  return [
    "_reportDetails",
    "comment",
    "graphic_variants",
    "id",
    "idCandidate",
    "item_id",
    "source",
    "sourceId",
    "sourceKind",
    "sourcePath"
  ].includes(key);
}

function stableStringify(value) {
  if (Array.isArray(value)) {
    return `[${value.map(stableStringify).join(",")}]`;
  }

  if (value && typeof value === "object") {
    return `{${Object.keys(value)
      .sort((left, right) => left.localeCompare(right, "en"))
      .map((key) => `${JSON.stringify(key)}:${stableStringify(value[key])}`)
      .join(",")}}`;
  }

  return JSON.stringify(value);
}

function sortTemplates(templates) {
  templates.sort((left, right) => {
    const leftPath = `${left.sourcePath}:${left.sourceId}`;
    const rightPath = `${right.sourcePath}:${right.sourceId}`;
    return leftPath.localeCompare(rightPath, "en");
  });
}

function cleanDisplayName(value, fallback) {
  const cleaned = String(value || fallback || "")
    .replace(/%s%/gi, "")
    .replace(/%es/gi, "")
    .replace(/%ves\/f%/gi, "f")
    .replace(/%/g, "")
    .replace(/\s+/g, " ")
    .trim();

  return cleaned || String(fallback || "").trim();
}

function parseInteger(value) {
  if (value === undefined || value === null || value === "") {
    return null;
  }

  const first = String(value).trim().split(/\s+/)[0];
  if (/^0x[0-9a-f]+$/i.test(first)) {
    return Number.parseInt(first.slice(2), 16);
  }

  if (!/^-?\d+$/.test(first)) {
    return null;
  }

  const parsed = Number.parseInt(first, 10);
  return Number.isFinite(parsed) ? parsed : null;
}

function parseBoolean(value) {
  if (value === undefined || value === null) {
    return null;
  }

  const normalized = String(value).trim().toLowerCase();
  if (["1", "true", "yes", "y", "on"].includes(normalized)) {
    return true;
  }

  if (["0", "false", "no", "n", "off"].includes(normalized)) {
    return false;
  }

  return null;
}

function addReportItem(report, template, details) {
  report.items.push({
    templateId: template.id,
    source: template.source,
    sourcePath: template.sourcePath,
    sourceKind: template.sourceKind,
    sourceId: template.sourceId,
    ...details
  });
}

function typedParam(value) {
  const text = String(value);
  if (/^-?\d+$/.test(text)) {
    return { type: "Integer", value: text };
  }

  return { type: "String", value: text };
}

function uniqueTags(values) {
  const tags = [];
  for (const value of values) {
    const tag = normalizeTemplateId(value);
    if (tag && !tags.includes(tag)) {
      tags.push(tag);
    }
  }

  return tags;
}

module.exports = {
  addReportItem,
  assignTemplateIds,
  cleanDisplayName,
  createReport,
  mergeGraphicVariants,
  normalizeTemplateId,
  parseBoolean,
  parseInteger,
  sortTemplates,
  typedParam,
  uniqueTags
};

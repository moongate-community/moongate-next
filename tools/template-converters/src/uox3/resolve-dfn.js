"use strict";

const RULESET_KEYS = {
  t2a: "gett2a",
  lbr: "getlbr",
  aos: "getaos",
  tol: "gettol"
};

function resolveDfnSections(sections, options, report) {
  const byId = new Map(sections.map((section) => [section.sourceId.toLowerCase(), section]));
  const resolved = new Map();
  const visiting = new Set();
  const results = [];

  for (const section of sections) {
    const item = resolve(section, byId, resolved, visiting, options, report);
    if (item && isConcrete(item)) {
      results.push(item);
    }
  }

  return results;
}

function resolve(section, byId, resolved, visiting, options, report) {
  const key = section.sourceId.toLowerCase();
  if (resolved.has(key)) {
    return resolved.get(key);
  }

  if (visiting.has(key)) {
    report.cycles.push({ sourcePath: section.sourcePath, sourceId: section.sourceId });
    return null;
  }

  visiting.add(key);
  let merged = cloneSection(section);
  const parents = [...values(section.properties, "get"), ...values(section.properties, RULESET_KEYS[options.ruleset])];

  for (const parentId of parents) {
    const parent = byId.get(parentId.toLowerCase());
    if (!parent) {
      report.unresolvedReferences.push({
        sourcePath: section.sourcePath,
        sourceId: section.sourceId,
        reference: parentId
      });
      continue;
    }

    const resolvedParent = resolve(parent, byId, resolved, visiting, options, report);
    if (resolvedParent) {
      merged = mergeSections(resolvedParent, merged);
    }
  }

  visiting.delete(key);
  resolved.set(key, merged);
  return merged;
}

function mergeSections(parent, child) {
  const merged = cloneSection(child);
  const properties = new Map();

  for (const [key, entry] of parent.properties.entries()) {
    properties.set(key, { key: entry.key, values: [...entry.values] });
  }

  for (const [key, entry] of child.properties.entries()) {
    properties.set(key, { key: entry.key, values: [...entry.values] });
  }

  merged.properties = properties;
  return merged;
}

function cloneSection(section) {
  return {
    ...section,
    properties: new Map([...section.properties.entries()].map(([key, entry]) => [key, { key: entry.key, values: [...entry.values] }]))
  };
}

function values(properties, key) {
  return properties.get(key)?.values || [];
}

function isConcrete(section) {
  return section.sourceId.toLowerCase() !== "base_item" && !section.sourceId.toLowerCase().startsWith("base_");
}

module.exports = { resolveDfnSections };

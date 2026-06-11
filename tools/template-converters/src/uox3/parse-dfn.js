"use strict";

const fs = require("node:fs");
const { relativeSourcePath } = require("../files");

function parseDfn(file, inputRoot) {
  const lines = fs.readFileSync(file, "utf8").split(/\r?\n/);
  const sections = [];
  let current = null;

  for (let index = 0; index < lines.length; index += 1) {
    const lineNumber = index + 1;
    const raw = stripComment(lines[index]).trim();
    if (!raw) {
      continue;
    }

    const header = raw.match(/^\[([^\]]+)]$/);
    if (header) {
      current = {
        source: "uox3",
        sourcePath: relativeSourcePath(file, inputRoot),
        sourceKind: "Item",
        sourceId: header[1],
        line: lineNumber,
        properties: new Map()
      };
      sections.push(current);
      continue;
    }

    if (raw === "{" || raw === "}") {
      continue;
    }

    if (!current) {
      continue;
    }

    const parsed = parseProperty(raw);
    if (!parsed) {
      continue;
    }

    const { key, value } = parsed;
    const normalizedKey = key.toLowerCase();
    const existing = current.properties.get(normalizedKey) || { key, values: [] };
    existing.values.push(value);
    current.properties.set(normalizedKey, existing);
  }

  return sections;
}

function stripComment(line) {
  return line.replace(/\s*(\/\/|#).*$/, "");
}

function parseProperty(raw) {
  const separator = raw.indexOf("=");
  if (separator >= 0) {
    return {
      key: raw.slice(0, separator).trim(),
      value: raw.slice(separator + 1).trim()
    };
  }

  const whitespace = raw.match(/^([A-Za-z][A-Za-z0-9_]*)\s+(.+)$/);
  if (!whitespace) {
    return null;
  }

  return {
    key: whitespace[1],
    value: whitespace[2].trim()
  };
}

module.exports = { parseDfn };

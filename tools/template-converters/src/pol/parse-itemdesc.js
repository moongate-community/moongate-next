"use strict";

const fs = require("node:fs");
const { relativeSourcePath } = require("../files");

const BLOCK_HEADER = /^(Item|Container|Weapon|Armor|Door|Map)\s+(\S+)\s*$/i;

function parsePolItemdesc(file, inputRoot) {
  const lines = fs.readFileSync(file, "utf8").split(/\r?\n/);
  const blocks = [];
  let current = null;
  let pending = null;

  for (let index = 0; index < lines.length; index += 1) {
    const lineNumber = index + 1;
    const raw = stripComment(lines[index]).trim();
    if (!raw) {
      continue;
    }

    const header = raw.match(BLOCK_HEADER);
    if (header) {
      pending = {
        source: "pol",
        sourcePath: relativeSourcePath(file, inputRoot),
        sourceKind: titleCase(header[1]),
        sourceId: header[2],
        line: lineNumber,
        properties: new Map()
      };
      continue;
    }

    if (raw === "{") {
      if (!pending) {
        throw new Error(`Unexpected POL block opening in ${file}:${lineNumber}`);
      }

      current = pending;
      pending = null;
      continue;
    }

    if (raw === "}") {
      if (!current) {
        throw new Error(`Unexpected POL block closing in ${file}:${lineNumber}`);
      }

      blocks.push(current);
      current = null;
      continue;
    }

    if (!current) {
      continue;
    }

    const [key, ...rest] = raw.split(/\s+/);
    if (!key || rest.length === 0) {
      continue;
    }

    const value = rest.join(" ").trim();
    const normalizedKey = key.toLowerCase();
    const existing = current.properties.get(normalizedKey) || { key, values: [] };
    existing.values.push(value);
    current.properties.set(normalizedKey, existing);
  }

  if (current || pending) {
    throw new Error(`Unclosed POL block in ${file}`);
  }

  return blocks;
}

function stripComment(line) {
  return line.replace(/\s*(\/\/|#).*$/, "");
}

function titleCase(value) {
  return value[0].toUpperCase() + value.slice(1).toLowerCase();
}

module.exports = { parsePolItemdesc };

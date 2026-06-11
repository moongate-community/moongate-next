"use strict";

const fs = require("node:fs");
const path = require("node:path");

function discoverInputFiles(options) {
  return filterInputFiles(collectInputFiles(options.input), options);
}

function detectInputSource(options) {
  const files = collectInputFiles(options.input);
  const polFiles = filterInputFiles(files, { ...options, source: "pol" });
  const uoxFiles = filterInputFiles(files, { ...options, source: "uox3" });

  if (polFiles.length > 0 && uoxFiles.length > 0) {
    throw new Error(
      `Scan input contains both POL itemdesc.cfg files and UOX3 .dfn files; use --pol or --uox explicitly: ${options.input}`
    );
  }

  if (polFiles.length > 0) {
    return { source: "pol", fileCount: polFiles.length };
  }

  if (uoxFiles.length > 0) {
    return { source: "uox3", fileCount: uoxFiles.length };
  }

  throw new Error(`Scan input does not contain POL itemdesc.cfg files or UOX3 .dfn files: ${options.input}`);
}

function collectInputFiles(input) {
  if (!fs.existsSync(input)) {
    throw new Error(`Input path does not exist: ${input}`);
  }

  const stat = fs.statSync(input);
  return stat.isDirectory()
    ? walk(input)
    : [input];
}

function filterInputFiles(files, options) {
  const extension = options.source === "pol" ? ".cfg" : ".dfn";
  return files
    .filter((file) => file.toLowerCase().endsWith(extension))
    .filter((file) => options.source !== "pol" || path.basename(file).toLowerCase() === "itemdesc.cfg")
    .filter((file) => matchesAny(file, options.include, true))
    .filter((file) => !matchesAny(file, options.exclude, false))
    .sort((left, right) => left.localeCompare(right, "en"));
}

function walk(directory) {
  const entries = fs.readdirSync(directory, { withFileTypes: true })
    .sort((left, right) => left.name.localeCompare(right.name, "en"));

  const files = [];
  for (const entry of entries) {
    const fullPath = path.join(directory, entry.name);
    if (entry.isDirectory()) {
      files.push(...walk(fullPath));
    } else if (entry.isFile()) {
      files.push(fullPath);
    }
  }

  return files;
}

function matchesAny(file, patterns, defaultValue) {
  if (patterns.length === 0) {
    return defaultValue;
  }

  const normalized = normalizePath(file);
  return patterns.some((pattern) => {
    const normalizedPattern = normalizePath(pattern);
    if (!normalizedPattern.includes("*")) {
      return normalized.includes(normalizedPattern);
    }

    return globFragmentToRegExp(normalizedPattern).test(normalized);
  });
}

function globFragmentToRegExp(pattern) {
  const escaped = pattern
    .split("*")
    .map((part) => part.replace(/[|\\{}()[\]^$+?.]/g, "\\$&"))
    .join(".*");

  return new RegExp(escaped, "i");
}

function normalizePath(value) {
  return value.replaceAll(path.sep, "/").toLowerCase();
}

function ensureParentDirectory(file) {
  fs.mkdirSync(path.dirname(file), { recursive: true });
}

function outputPathForSource(outputDirectory, sourcePath) {
  const normalized = normalizePath(sourcePath)
    .replace(/\.(cfg|dfn)$/i, ".yaml")
    .split("/")
    .filter(Boolean);

  return path.join(outputDirectory, ...normalized);
}

function relativeSourcePath(file, inputRoot) {
  const base = fs.existsSync(inputRoot) && fs.statSync(inputRoot).isDirectory()
    ? inputRoot
    : path.dirname(inputRoot);
  return normalizePath(path.relative(base, file));
}

module.exports = { detectInputSource, discoverInputFiles, ensureParentDirectory, outputPathForSource, relativeSourcePath };

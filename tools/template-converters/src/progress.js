"use strict";

const path = require("node:path");

function createProgressReporter(options, stream = process.stderr) {
  const enabled = shouldShowProgress(options, stream);
  if (!enabled) {
    return {
      update() {},
      finish() {}
    };
  }

  let lastLength = 0;

  function render(event) {
    const total = Math.max(event.total || 0, 1);
    const current = Math.min(event.current || 0, total);
    const width = 24;
    const filled = Math.round((current / total) * width);
    const bar = `${"#".repeat(filled)}${"-".repeat(width - filled)}`;
    const file = event.file ? relativeFile(event.file, options.input) : event.message || "finalizing";
    const line = `[${bar}] ${current}/${total} ${file}`;

    if (stream.isTTY) {
      const padded = line.padEnd(lastLength, " ");
      stream.write(`\r${padded}`);
      lastLength = line.length;
      return;
    }

    stream.write(`${line}\n`);
  }

  return {
    update: render,
    finish() {
      if (stream.isTTY) {
        stream.write("\n");
      }
    }
  };
}

function shouldShowProgress(options, stream) {
  if (options.noProgress) {
    return false;
  }

  if (options.progress) {
    return true;
  }

  return Boolean(stream.isTTY);
}

function relativeFile(file, inputRoot) {
  const root = inputRoot || process.cwd();
  if (root === file) {
    return path.basename(file);
  }

  const relative = path.relative(root, file);
  return relative && !relative.startsWith("..") ? relative : file;
}

module.exports = { createProgressReporter };

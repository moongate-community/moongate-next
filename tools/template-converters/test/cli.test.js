"use strict";

const path = require("node:path");
const fs = require("node:fs");
const os = require("node:os");
const test = require("node:test");
const assert = require("node:assert/strict");
const { parseArgs, run } = require("../src/cli");

test("parses POL shortcut input", () => {
  const options = parseArgs(["--pol", "legacy/pol", "--output", "items.yaml"]);

  assert.equal(options.source, "pol");
  assert.equal(options.input, path.resolve("legacy/pol"));
  assert.equal(options.output, path.resolve("items.yaml"));
});

test("parses output directory mode", () => {
  const options = parseArgs(["--pol", "legacy/pol", "--output-dir", "generated/items"]);

  assert.equal(options.source, "pol");
  assert.equal(options.input, path.resolve("legacy/pol"));
  assert.equal(options.outputDir, path.resolve("generated/items"));
});

test("parses scan input", () => {
  const options = parseArgs(["--scan", "legacy/items", "--output-dir", "generated/items"]);

  assert.equal(options.scan, true);
  assert.equal(options.input, path.resolve("legacy/items"));
  assert.equal(options.outputDir, path.resolve("generated/items"));
});

test("parses UOX shortcut input", () => {
  const options = parseArgs(["--uox", "legacy/uox3", "--ruleset", "tol", "--dry-run", "--progress"]);

  assert.equal(options.source, "uox3");
  assert.equal(options.input, path.resolve("legacy/uox3"));
  assert.equal(options.ruleset, "tol");
  assert.equal(options.dryRun, true);
  assert.equal(options.progress, true);
});

test("rejects mixed shortcut and explicit input forms", () => {
  assert.throws(
    () => parseArgs(["--pol", "legacy/pol", "--input", "other"]),
    /cannot be combined/
  );

  assert.throws(
    () => parseArgs(["--source", "pol", "--uox", "legacy/uox3"]),
    /cannot be combined/
  );

  assert.throws(
    () => parseArgs(["--scan", "legacy/items", "--pol", "legacy/pol"]),
    /cannot be combined/
  );
});

test("parses progress controls", () => {
  const options = parseArgs(["--pol", "legacy/pol", "--output", "items.yaml", "--no-progress"]);

  assert.equal(options.noProgress, true);
});

test("writes split output files preserving source structure", async () => {
  const outputDir = fs.mkdtempSync(path.join(os.tmpdir(), "moongate-template-converter-"));
  const fixturesRoot = path.join(__dirname, "fixtures");

  try {
    const result = await run([
      "--pol",
      fixturesRoot,
      "--output-dir",
      outputDir,
      "--report",
      path.join(outputDir, "report.json"),
      "--no-progress"
    ]);

    const outputFile = path.join(outputDir, "pol", "itemdesc.yaml");
    assert.equal(result.report.output.fileCount, 1);
    assert.equal(fs.existsSync(outputFile), true);
    assert.match(fs.readFileSync(outputFile, "utf8"), /^item_templates:/);
  } finally {
    fs.rmSync(outputDir, { recursive: true, force: true });
  }
});

test("scan mode auto-detects POL input", async () => {
  const outputDir = fs.mkdtempSync(path.join(os.tmpdir(), "moongate-template-converter-pol-scan-"));
  const polRoot = path.join(__dirname, "fixtures", "pol");

  try {
    const result = await run([
      "--scan",
      polRoot,
      "--output-dir",
      outputDir,
      "--no-progress"
    ]);

    assert.equal(result.report.source, "pol");
    assert.equal(result.scanFileCount, undefined);
    assert.equal(fs.existsSync(path.join(outputDir, "itemdesc.yaml")), true);
  } finally {
    fs.rmSync(outputDir, { recursive: true, force: true });
  }
});

test("scan mode auto-detects UOX3 input", async () => {
  const outputDir = fs.mkdtempSync(path.join(os.tmpdir(), "moongate-template-converter-uox-scan-"));
  const uoxRoot = path.join(__dirname, "fixtures", "uox3");

  try {
    const result = await run([
      "--scan",
      uoxRoot,
      "--output-dir",
      outputDir,
      "--no-progress"
    ]);

    assert.equal(result.report.source, "uox3");
    assert.equal(fs.existsSync(path.join(outputDir, "items.yaml")), true);
  } finally {
    fs.rmSync(outputDir, { recursive: true, force: true });
  }
});

test("scan mode rejects mixed POL and UOX3 input", async () => {
  const outputDir = fs.mkdtempSync(path.join(os.tmpdir(), "moongate-template-converter-mixed-scan-"));
  const fixturesRoot = path.join(__dirname, "fixtures");

  try {
    await assert.rejects(
      () => run([
        "--scan",
        fixturesRoot,
        "--output-dir",
        outputDir,
        "--no-progress"
      ]),
      /contains both POL/
    );
  } finally {
    fs.rmSync(outputDir, { recursive: true, force: true });
  }
});

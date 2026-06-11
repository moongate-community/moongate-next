"use strict";

const fs = require("node:fs");
const path = require("node:path");
const { detectInputSource, discoverInputFiles, ensureParentDirectory, outputPathForSource } = require("./files");
const { addReportItem, assignTemplateIds, createReport, mergeGraphicVariants, sortTemplates } = require("./normalize");
const { renderItemTemplatesYaml } = require("./render-yaml");
const { parsePolItemdesc } = require("./pol/parse-itemdesc");
const { mapPolItem } = require("./pol/map-pol-item");
const { parseDfn } = require("./uox3/parse-dfn");
const { resolveDfnSections } = require("./uox3/resolve-dfn");
const { mapUox3Item } = require("./uox3/map-uox3-item");
const { createProgressReporter } = require("./progress");

const VALID_SOURCES = new Set(["pol", "uox3"]);
const VALID_RULESETS = new Set(["t2a", "lbr", "aos", "tol"]);

async function run(argv) {
  const options = parseArgs(argv);
  resolveScanSource(options);

  if (!VALID_SOURCES.has(options.source)) {
    throw new Error("--source must be one of: pol, uox3; or use --scan <file-or-directory>");
  }

  if (!options.input) {
    throw new Error("--input is required");
  }

  if (options.output && options.outputDir) {
    throw new Error("--output cannot be combined with --output-dir");
  }

  if (!options.output && !options.outputDir && !options.dryRun) {
    throw new Error("--output or --output-dir is required unless --dry-run is used");
  }

  if (!VALID_RULESETS.has(options.ruleset)) {
    throw new Error("--ruleset must be one of: t2a, lbr, aos, tol");
  }

  const progress = createProgressReporter(options);
  let result;
  try {
    result = convert({ ...options, onProgress: progress.update });
  } finally {
    progress.finish();
  }

  const yaml = renderItemTemplatesYaml(result.templates);

  if (options.dryRun) {
    process.stdout.write(yaml);
  } else if (options.outputDir) {
    writeDirectoryOutputs(result, options.outputDir);
  } else {
    ensureParentDirectory(options.output);
    fs.writeFileSync(options.output, yaml, "utf8");
  }

  if (options.report) {
    ensureParentDirectory(options.report);
    fs.writeFileSync(options.report, `${JSON.stringify(result.report, null, 2)}\n`, "utf8");
  }

  return result;
}

function writeDirectoryOutputs(result, outputDirectory) {
  const groups = new Map();

  for (const template of result.templates) {
    const group = groups.get(template.sourcePath) || [];
    group.push(template);
    groups.set(template.sourcePath, group);
  }

  for (const [sourcePath, templates] of groups.entries()) {
    const outputPath = outputPathForSource(outputDirectory, sourcePath);
    ensureParentDirectory(outputPath);
    fs.writeFileSync(outputPath, renderItemTemplatesYaml(templates), "utf8");
  }

  result.report.output.outputDirectory = outputDirectory;
  result.report.output.fileCount = groups.size;
}

function convert(options) {
  const files = discoverInputFiles(options);
  const report = createReport(options, files);
  const templates = [];
  const onProgress = typeof options.onProgress === "function" ? options.onProgress : null;

  if (options.source === "pol") {
    for (const [index, file] of files.entries()) {
      onProgress?.({ current: index + 1, total: files.length, file });
      const blocks = parsePolItemdesc(file, options.input);
      for (const block of blocks) {
        templates.push(mapPolItem(block, options, report));
      }
    }
  } else {
    const sections = [];
    for (const [index, file] of files.entries()) {
      onProgress?.({ current: index + 1, total: files.length, file });
      sections.push(...parseDfn(file, options.input));
    }

    onProgress?.({ current: files.length, total: files.length, message: "resolving inheritance" });
    const resolved = resolveDfnSections(sections, options, report);
    for (const section of resolved) {
      const template = mapUox3Item(section, options, report);
      if (template) {
        templates.push(template);
      }
    }
  }

  const mergedTemplates = mergeGraphicVariants(templates, report);
  assignTemplateIds(mergedTemplates, report);
  sortTemplates(mergedTemplates);
  for (const template of mergedTemplates) {
    addReportItem(report, template, template._reportDetails || {});
    delete template._reportDetails;
  }
  report.output.templateCount = mergedTemplates.length;

  return { templates: mergedTemplates, report };
}

function parseArgs(argv) {
  const options = {
    include: [],
    exclude: [],
    tags: [],
    ruleset: "aos",
    includeSourceParams: false,
    dryRun: false,
    progress: false,
    noProgress: false
  };

  for (let index = 0; index < argv.length; index += 1) {
    const arg = argv[index];
    switch (arg) {
      case "--pol":
        setShortcutSource(options, "pol", takeValue(argv, ++index, arg), arg);
        break;
      case "--uox":
      case "--uox3":
        setShortcutSource(options, "uox3", takeValue(argv, ++index, arg), arg);
        break;
      case "--scan":
        setScanInput(options, takeValue(argv, ++index, arg), arg);
        break;
      case "--source":
        setExplicitSource(options, takeValue(argv, ++index, arg), arg);
        break;
      case "--input":
        setExplicitInput(options, takeValue(argv, ++index, arg), arg);
        break;
      case "--output":
        options.output = path.resolve(takeValue(argv, ++index, arg));
        break;
      case "--output-dir":
        options.outputDir = path.resolve(takeValue(argv, ++index, arg));
        break;
      case "--include":
        options.include.push(takeValue(argv, ++index, arg));
        break;
      case "--exclude":
        options.exclude.push(takeValue(argv, ++index, arg));
        break;
      case "--tag":
        options.tags.push(takeValue(argv, ++index, arg));
        break;
      case "--ruleset":
        options.ruleset = takeValue(argv, ++index, arg).toLowerCase();
        break;
      case "--include-source-params":
        options.includeSourceParams = true;
        break;
      case "--report":
        options.report = path.resolve(takeValue(argv, ++index, arg));
        break;
      case "--dry-run":
        options.dryRun = true;
        break;
      case "--progress":
        options.progress = true;
        break;
      case "--no-progress":
        options.noProgress = true;
        break;
      case "--help":
      case "-h":
        printHelp();
        process.exit(0);
        break;
      default:
        throw new Error(`Unknown argument: ${arg}`);
    }
  }

  delete options.shortcutSourceOption;
  delete options.scanSourceOption;
  return options;
}

function resolveScanSource(options) {
  if (!options.scan) {
    return;
  }

  const detected = detectInputSource(options);
  options.source = detected.source;
  options.scanFileCount = detected.fileCount;
}

function setShortcutSource(options, source, input, optionName) {
  if (options.shortcutSourceOption) {
    throw new Error(`${optionName} cannot be combined with ${options.shortcutSourceOption}`);
  }

  if (options.scanSourceOption) {
    throw new Error(`${optionName} cannot be combined with ${options.scanSourceOption}`);
  }

  if (options.source || options.input) {
    throw new Error(`${optionName} cannot be combined with --source or --input`);
  }

  options.shortcutSourceOption = optionName;
  options.source = source;
  options.input = path.resolve(input);
}

function setExplicitSource(options, source, optionName) {
  if (options.shortcutSourceOption) {
    throw new Error(`${optionName} cannot be combined with ${options.shortcutSourceOption}`);
  }

  if (options.scanSourceOption) {
    throw new Error(`${optionName} cannot be combined with ${options.scanSourceOption}`);
  }

  options.source = source;
}

function setExplicitInput(options, input, optionName) {
  if (options.shortcutSourceOption) {
    throw new Error(`${optionName} cannot be combined with ${options.shortcutSourceOption}`);
  }

  if (options.scanSourceOption) {
    throw new Error(`${optionName} cannot be combined with ${options.scanSourceOption}`);
  }

  options.input = path.resolve(input);
}

function setScanInput(options, input, optionName) {
  if (options.scanSourceOption) {
    throw new Error(`${optionName} cannot be combined with ${options.scanSourceOption}`);
  }

  if (options.shortcutSourceOption || options.source || options.input) {
    throw new Error(`${optionName} cannot be combined with --pol, --uox, --source, or --input`);
  }

  options.scanSourceOption = optionName;
  options.scan = true;
  options.input = path.resolve(input);
}

function takeValue(argv, index, optionName) {
  const value = argv[index];
  if (!value || value.startsWith("--")) {
    throw new Error(`${optionName} requires a value`);
  }

  return value;
}

function printHelp() {
  process.stdout.write(`Usage:
  node tools/template-converters/bin/moongate-template-converter.js --scan <file-or-directory> --output-dir <directory>
  node tools/template-converters/bin/moongate-template-converter.js --pol <file-or-directory> --output <yaml-file>
  node tools/template-converters/bin/moongate-template-converter.js --pol <file-or-directory> --output-dir <directory>
  node tools/template-converters/bin/moongate-template-converter.js --uox <file-or-directory> --ruleset aos --output <yaml-file>
  node tools/template-converters/bin/moongate-template-converter.js --uox <file-or-directory> --ruleset aos --output-dir <directory>

Options:
  --scan <file-or-directory>           Auto-detect POL itemdesc.cfg or UOX3 .dfn input
  --pol <file-or-directory>            POL itemdesc.cfg source
  --uox <file-or-directory>            UOX3 .dfn source
  --uox3 <file-or-directory>           Alias for --uox
  --source pol|uox3                    Explicit source form
  --input <file-or-directory>          Explicit input form
  --output <yaml-file>                 Write one bundled YAML file
  --output-dir <directory>             Write one YAML per source file preserving source paths
  --include <glob-or-path-fragment>    Repeatable
  --exclude <glob-or-path-fragment>    Repeatable
  --tag <tag>                          Repeatable
  --ruleset t2a|lbr|aos|tol            UOX3 only; default aos
  --include-source-params
  --progress                          Force progress output on stderr
  --no-progress                       Disable progress output
  --report <json-file>
  --dry-run
`);
}

module.exports = { convert, parseArgs, run };

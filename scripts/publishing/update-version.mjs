import fs from "node:fs";

const version = process.argv[2];

if (!version) {
  console.error("Usage: node scripts/publishing/update-version.mjs <version>");
  process.exit(1);
}

if (!/^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$/.test(version)) {
  console.error(`Invalid semantic version: ${version}`);
  process.exit(1);
}

const baseVersion = version.split("-")[0];
const [major, minor, patch] = baseVersion.split(".");
const assemblyVersion = `${major}.${minor}.${patch}.0`;

const path = "Directory.Build.props";
let text = fs.readFileSync(path, "utf8");

const replacements = new Map([
  ["Version", version],
  ["AssemblyVersion", assemblyVersion],
  ["FileVersion", assemblyVersion],
  ["InformationalVersion", version]
]);

for (const [propertyName, propertyValue] of replacements) {
  const pattern = new RegExp(`<${propertyName}>[^<]*</${propertyName}>`);
  if (!pattern.test(text)) {
    console.error(`Missing ${propertyName} in ${path}`);
    process.exit(1);
  }

  text = text.replace(pattern, `<${propertyName}>${propertyValue}</${propertyName}>`);
}

fs.writeFileSync(path, text);
console.log(`Updated ${path} to ${version}`);

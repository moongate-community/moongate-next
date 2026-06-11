"use strict";

const FIELD_ORDER = [
  "id",
  "name",
  "comment",
  "item_id",
  "hue",
  "weight",
  "amount",
  "is_stackable",
  "is_movable",
  "gump_id",
  "layer",
  "script_id",
  "rarity",
  "visibility"
];

function renderItemTemplatesYaml(templates) {
  const lines = ["item_templates:"];

  for (const template of templates) {
    lines.push(`    - id: ${scalar(template.id)}`);
    let renderedGraphicVariants = false;
    for (const field of FIELD_ORDER.slice(1)) {
      if (template[field] !== undefined && template[field] !== null && template[field] !== "") {
        lines.push(`      ${field}: ${scalar(template[field])}`);
      }

      if (field === "item_id") {
        renderedGraphicVariants = renderGraphicVariants(lines, template);
      }
    }

    if (!renderedGraphicVariants) {
      renderGraphicVariants(lines, template);
    }

    if (template.value) {
      lines.push("      value:");
      lines.push(`          buy: ${template.value.buy}`);
      if (template.value.sell !== undefined && template.value.sell !== null) {
        lines.push(`          sell: ${template.value.sell}`);
      }
    }

    if (template.tags && template.tags.length > 0) {
      lines.push(`      tags: [${template.tags.map(scalar).join(", ")}]`);
    }

    if (template.params && Object.keys(template.params).length > 0) {
      lines.push("      params:");
      for (const key of Object.keys(template.params).sort((left, right) => left.localeCompare(right, "en"))) {
        const param = template.params[key];
        lines.push(`          ${key}:`);
        lines.push(`              type: ${param.type}`);
        lines.push(`              value: ${scalar(param.value)}`);
      }
    }
  }

  return `${lines.join("\n")}\n`;
}

function renderGraphicVariants(lines, template) {
  if (!template.graphic_variants || template.graphic_variants.length === 0) {
    return false;
  }

  lines.push("      graphic_variants:");
  for (const variant of template.graphic_variants) {
    lines.push(`          - item_id: ${scalar(variant.item_id)}`);
  }

  return true;
}

function scalar(value) {
  if (typeof value === "number" || typeof value === "boolean") {
    return String(value);
  }

  const text = String(value);
  if (/^[A-Za-z0-9_.-]+$/.test(text)) {
    return text;
  }

  return JSON.stringify(text);
}

module.exports = { renderItemTemplatesYaml };

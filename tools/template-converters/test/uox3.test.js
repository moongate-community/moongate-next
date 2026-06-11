"use strict";

const path = require("node:path");
const test = require("node:test");
const assert = require("node:assert/strict");
const { convert } = require("../src/cli");

const fixture = path.join(__dirname, "fixtures/uox3/items.dfn");

test("converts UOX3 DFN sections with inheritance", () => {
  const result = convert({
    source: "uox3",
    input: fixture,
    include: [],
    exclude: [],
    tags: ["fixture"],
    ruleset: "aos",
    includeSourceParams: true
  });

  const bread = result.templates.find((template) => template.id === "bread_loaf");
  assert.equal(bread.item_id, 4155);
  assert.equal(bread.weight, 1);
  assert.equal(bread.is_movable, true);
  assert.equal(bread.is_stackable, true);
  assert.deepEqual(bread.value, { buy: 7, sell: 2 });

  const katana = result.templates.find((template) => template.id === "katana");
  assert.equal(katana.item_id, 5119);
  assert.equal(katana.weight, 5);
  assert.equal(katana.layer, "OneHanded");
  assert.deepEqual(katana.params.damage, { type: "String", value: "5 10" });
  assert.deepEqual(katana.params.magic, { type: "String", value: "true" });

  const bronzeGloves = result.templates.find((template) => template.id === "bronze_ringmail_gloves");
  assert.equal(bronzeGloves.item_id, 5099);
  assert.equal(bronzeGloves.layer, "Gloves");
});

test("selects requested UOX3 ruleset inheritance", () => {
  const result = convert({
    source: "uox3",
    input: fixture,
    include: [],
    exclude: [],
    tags: [],
    ruleset: "tol",
    includeSourceParams: true
  });

  const katana = result.templates.find((template) => template.id === "katana");
  assert.deepEqual(katana.params.damage, { type: "String", value: "10 20" });
});

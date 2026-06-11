"use strict";

const path = require("node:path");
const test = require("node:test");
const assert = require("node:assert/strict");
const { convert } = require("../src/cli");

const fixture = path.join(__dirname, "fixtures/pol/itemdesc.cfg");

test("converts POL itemdesc blocks into Moongate templates", () => {
  const result = convert({
    source: "pol",
    input: fixture,
    include: [],
    exclude: [],
    tags: ["fixture"],
    ruleset: "aos",
    includeSourceParams: true
  });

  assert.equal(result.templates.length, 6);
  assert.equal(result.report.collisions.length, 1);

  const bread = result.templates.find((template) => template.id === "bread");
  assert.equal(bread.name, "bread loaf");
  assert.equal(bread.item_id, 4155);
  assert.equal(bread.weight, 1);
  assert.equal(bread.is_movable, true);
  assert.deepEqual(bread.value, { buy: 7, sell: 2 });
  assert.deepEqual(bread.params.food_value, { type: "Integer", value: "3" });
  assert.deepEqual(bread.params.cursed, { type: "String", value: "true" });

  const gold = result.templates.find((template) => template.id === "gold");
  assert.equal(gold.is_stackable, true);

  const weapon = result.templates.find((template) => template.id === "scimitar");
  assert.equal(weapon.layer, "OneHanded");
  assert.deepEqual(weapon.params.mindam, { type: "Integer", value: "3" });

  const armor = result.templates.find((template) => template.id === "chainmail_coif");
  assert.equal(armor.layer, "Helm");
});

test("merges equivalent duplicate POL names into graphic variants", () => {
  const result = convert({
    source: "pol",
    input: fixture,
    include: [],
    exclude: [],
    tags: [],
    ruleset: "aos",
    includeSourceParams: false
  });

  const bread = result.templates.find((template) => template.id === "bread");
  assert.equal(result.templates.length, 5);
  assert.deepEqual(bread.graphic_variants, [{ item_id: 4156 }]);
  assert.equal(result.report.collisions.length, 0);
  assert.equal(result.report.mergedGraphicVariants.length, 1);
  assert.equal(result.report.mergedGraphicVariants[0].id, "bread");
});

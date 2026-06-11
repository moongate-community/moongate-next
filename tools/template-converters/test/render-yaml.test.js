"use strict";

const test = require("node:test");
const assert = require("node:assert/strict");
const { renderItemTemplatesYaml } = require("../src/render-yaml");

test("renders deterministic item template YAML", () => {
  const yaml = renderItemTemplatesYaml([
    {
      id: "bread",
      name: "bread loaf",
      comment: "Converted",
      item_id: 4155,
      graphic_variants: [{ item_id: 4156 }],
      weight: 1,
      is_stackable: true,
      is_movable: true,
      value: { buy: 7, sell: 2 },
      tags: ["food", "pol"],
      params: {
        food_value: { type: "Integer", value: "3" }
      }
    }
  ]);

  assert.equal(yaml, `item_templates:
    - id: bread
      name: "bread loaf"
      comment: Converted
      item_id: 4155
      graphic_variants:
          - item_id: 4156
      weight: 1
      is_stackable: true
      is_movable: true
      value:
          buy: 7
          sell: 2
      tags: [food, pol]
      params:
          food_value:
              type: Integer
              value: 3
`);
});

# Item Template Converters

Moongate includes a small Node.js converter for importing legacy Ultima Online item definitions into `item_templates` YAML. It is intended for reviewable migration output, not for automatically changing bundled runtime data.

The converter supports two source formats:

- POL `itemdesc.cfg` files, which are the preferred source when available because item values, names, graphics, weapons, armor, containers, and doors are usually close to Moongate template semantics.
- UOX3 `.dfn` files, which are useful for broader coverage but require inheritance resolution and a selected ruleset.

## CLI

Run the converter from the repository root:

```bash
npm run templates:convert -- \
  --scan /tmp/moongate-pol-modern-distro/pkg/items \
  --output-dir /tmp/moongate-pol-items \
  --report /tmp/moongate-pol-items.report.json
```

`--scan` auto-detects POL `itemdesc.cfg` files or UOX3 `.dfn` files. If a directory contains both formats, run with `--pol` or `--uox` explicitly.

For explicit POL conversion:

```bash
npm run templates:convert -- \
  --pol /tmp/moongate-pol-modern-distro/pkg/items \
  --output-dir /tmp/moongate-pol-items \
  --report /tmp/moongate-pol-items.report.json
```

For explicit UOX3 conversion, choose the ruleset used for `get<t2a|lbr|aos|tol>` inheritance:

```bash
npm run templates:convert -- \
  --uox /home/squid/projects/others/UOX3/data/dfndata/items \
  --ruleset aos \
  --output-dir /tmp/moongate-uox3-items \
  --report /tmp/moongate-uox3-items.report.json
```

Use `--output <file.yaml>` when you want one bundled YAML file. Use `--output-dir <directory>` when you want one YAML file per source file while preserving the source directory structure. For example, POL `food_drink/config/itemdesc.cfg` becomes `food_drink/config/itemdesc.yaml`, and UOX3 `food/foods.dfn` becomes `food/foods.yaml`.

Useful options:

- `--scan <path>` auto-detects POL `itemdesc.cfg` or UOX3 `.dfn` input.
- `--pol <path>` selects a POL `itemdesc.cfg` file or a directory containing them.
- `--uox <path>` selects a UOX3 `.dfn` file or directory. `--uox3` is accepted as an alias.
- `--output <file.yaml>` writes one bundled YAML file.
- `--output-dir <directory>` writes split YAML files preserving the source directory structure.
- `--include <text-or-glob>` filters input paths. It can be repeated.
- `--exclude <text-or-glob>` removes input paths. It can be repeated.
- `--tag <tag>` adds a tag to every emitted template. It can be repeated.
- `--include-source-params` emits unmapped source fields into typed template `params`.
- `--progress` forces the progress bar on `stderr`; interactive terminals show it automatically.
- `--no-progress` disables progress output for scripts.
- `--dry-run` writes YAML to stdout instead of a file.

## Mapping

Both parsers normalize source records before rendering deterministic YAML under the top-level `item_templates` list.

Common fields:

- `id` is normalized from the source name when possible. Duplicate ids get deterministic numeric suffixes and are listed in the JSON report.
- `name` uses POL `Desc` or UOX3 `name`, with common POL plural markers removed.
- `item_id` comes from POL `Graphic` or block object type, and from UOX3 `id`.
- `graphic_variants` is emitted when duplicate source records have the same normalized id and matching non-graphic fields. Only the alternate `item_id` values are merged.
- `weight` is rounded to Moongate's integer template field. Fractional or scaled source weights are noted in the report.
- `value.buy` and `value.sell` map from POL vendor fields or UOX3 `value=buy sell`.
- `tags` come from source path segments, source kind, and any `--tag` options.
- `comment` records source provenance.

POL-specific mapping:

- `Item`, `Container`, `Weapon`, `Armor`, `Door`, and `Map` blocks are supported.
- `Color` maps to `hue`.
- `Movable` maps to `is_movable`.
- `Layer`, `TwoHanded`, `Coverage`, and weapon block type map to `layer` when unambiguous.
- Weapon, armor, script, and package-specific fields stay in the report unless `--include-source-params` is used.

UOX3-specific mapping:

- `get` inheritance is resolved before mapping.
- The selected `--ruleset` resolves `gett2a`, `getlbr`, `getaos`, or `gettol`.
- `pileable=1` maps to `is_stackable`.
- `movable=1` maps to `is_movable`.
- Numeric `layer` values map to `ItemLayerType` names when known.
- Combat, armor, script, and emulator-specific fields stay in the report unless `--include-source-params` is used.

## Graphic Variant Merging

Legacy data often contains several records with the same name and behavior but
different graphics. The converter groups those records into one Moongate
template with `graphic_variants` when all emitted semantic fields match except
for `item_id`, source provenance, and generated comments.

Example output:

```yaml
item_templates:
    - id: bread
      name: "bread loaf"
      item_id: 4155
      graphic_variants:
          - item_id: 4156
      weight: 1
      is_movable: true
```

If two records share the same normalized id but differ in value, weight, layer,
tags, params, or other emitted semantic fields, they remain separate templates
with deterministic suffixes such as `bread` and `bread_2`.

Merged records are listed in the JSON report under `mergedGraphicVariants`.

## Review Workflow

Write generated YAML to `/tmp` or another scratch location first. Review the YAML and the report together, then copy only curated templates into `moongate_data/templates/items/` when they are ready to become bundled data.

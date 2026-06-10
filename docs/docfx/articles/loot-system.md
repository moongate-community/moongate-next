---
title: Loot system
---

# Loot System

Loot tables define reusable item drops that Moongate can validate at boot and
resolve into persisted item entities at runtime.

Use loot tables for creature drops, containers, scripted rewards, treasure
bundles, and any system that needs to produce items from the
[item template](item-templates.md) catalog.

## File locations

Runtime loot tables live under:

```text
moongate_data/templates/loot/
```

Bundled default loot tables live under:

```text
src/Moongate.Server/Assets/templates/loot/
```

During local development, edit the runtime directory. When changing the default
catalog shipped with Moongate, keep the bundled file in sync.

Moongate loads every `*.yaml` file under the loot directory, including nested
directories. Files are read in deterministic path order and all top-level
`loot_tables` entries are merged into one registry.

## Minimal loot table

```yaml
loot_tables:
    - id: poor
      content:
          - item: gold_coin
            amount: { min: 1, max: 25 }
          - category: food
            chance: 0.5
```

`id` is the loot table identifier used by runtime callers. `content` is a list
of loot nodes. When a loot table is generated, Moongate resolves every node in
`content` and returns the created item entities.

## Node types

Every loot node must declare exactly one node type.

| Node type | YAML field | Behavior |
| --- | --- | --- |
| Item | `item` | Creates the referenced item template. |
| Category | `category` | Picks one concrete item template with the matching tag, then creates it. |
| Pick one | `pick_one_of` | Chooses exactly one child node, using child `weight` values. |
| Group | `group` | Resolves every child node. |

### Item

Use `item` when the drop should produce one specific template id.

```yaml
content:
    - item: gold_coin
      amount: { min: 10, max: 100 }
```

The template must exist and must not be abstract.

### Category

Use `category` when the drop can be any concrete item template carrying a tag.

```yaml
content:
    - category: food
      chance: 0.5
```

Category matching is case-insensitive and uses item template `tags`. Abstract
templates are ignored. Boot validation fails if a category has no concrete
template match.

### Pick One

Use `pick_one_of` when exactly one child should be selected.

```yaml
content:
    - pick_one_of:
          - category: food
          - category: reagent
            weight: 2
```

`weight` is relative to sibling weights. In the example above, `reagent` has
twice the chance of `food`.

### Group

Use `group` when several nodes should behave as one nested bundle.

```yaml
content:
    - group:
          - item: gold_coin
            amount: 25
          - pick_one_of:
                - category: food
                - category: reagent
```

A `group` can contain any node type, including nested groups.

## Common node fields

| Field | Type | Default | Description |
| --- | --- | --- | --- |
| `chance` | number | `1.0` | Probability that the node resolves. `0.0` never resolves, `1.0` always resolves. |
| `amount` | integer or object | `1` | Amount to produce for `item` or `category` nodes. |
| `weight` | integer | `1` | Relative selection weight when the node is a direct child of `pick_one_of`. |

`chance` is applied before the node type resolves. If a `group` or
`pick_one_of` node misses its chance roll, the whole subtree is skipped.

`amount` is meaningful only on `item` and `category` nodes. Avoid setting it on
`group` or `pick_one_of`; it is not used for those node types.

## Amount

`amount` can be a fixed scalar:

```yaml
amount: 7
```

or a range:

```yaml
amount: { min: 1, max: 100 }
```

If `max` is omitted, it defaults to `min`. If `amount` is omitted entirely,
Moongate uses `1`.

Runtime behavior depends on the item template:

- Stackable templates create one entity with the rolled amount.
- Non-stackable templates create one entity per rolled count.
- Non-stackable counts are clamped to `100` entities and log a warning.
- A rolled amount of `0` produces no item.

## Full example

```yaml
loot_tables:
    - id: poor
      content:
          - item: gold_coin
            amount: { min: 1, max: 25 }
          - category: food
            chance: 0.5

    - id: common
      content:
          - item: gold_coin
            amount: { min: 10, max: 100 }
          - pick_one_of:
                - category: food
                - category: reagent
                  weight: 2
          - category: armor
            chance: 0.25
          - category: weapon
            chance: 0.25
```

The bundled sample currently defines `poor` and `common` tables.

## Boot validation

Loot tables are loaded and validated during server boot after item templates
and starter loadouts. Invalid loot data prevents the server from starting with
a broken registry.

Validation rules:

- `id` must be present and non-empty.
- `id` must be unique, case-insensitively, across all loaded loot files.
- Every node must declare exactly one of `item`, `category`, `pick_one_of`, or
  `group`.
- `item` must reference an existing concrete item template.
- `category` must match at least one concrete item template tag.
- `pick_one_of` and `group` must contain at least one child node.
- `chance` must be between `0.0` and `1.0`.
- `weight` must be at least `1`.
- `amount.min` and `amount.max` must be non-negative.
- `amount.min` cannot be greater than `amount.max`.
- Empty YAML node entries are rejected with file and node context.

Missing or empty loot directories do not fail boot. They log a warning and
produce an empty loot table registry.

## Runtime usage

The loot service exposes a small runtime surface:

```csharp
bool Has(string lootTableId);
ValueTask<IReadOnlyList<ItemEntity>> GenerateAsync(
    string lootTableId,
    CancellationToken cancellationToken = default
);
```

`GenerateAsync` throws when the loot table id is unknown. Generated items are
created through the item factory, so template fields such as name, item id,
hue, rarity, value, visibility, stackability, and custom params are copied in
the same way as any other item created from a template.

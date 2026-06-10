---
title: Item templates
---

# Item Templates

Item templates are YAML definitions for item types that Moongate can load,
inspect in the admin UI, and instantiate into persisted item entities.

Use them for reusable shard data such as weapons, armor, reagents, tools,
containers, currency, and items referenced by
[starter loadouts](starter-loadouts.md) and [loot tables](loot-system.md).

## File locations

Runtime item templates live under:

```text
moongate_data/templates/items/
```

Bundled default templates live under:

```text
src/Moongate.Server/Assets/templates/items/
```

During local development, edit `moongate_data/templates/items/` to affect the
current runtime data directory. When changing the default catalog shipped with
Moongate, keep the matching file under
`src/Moongate.Server/Assets/templates/items/` in sync.

Each YAML file contains a top-level `item_templates` list.

## Minimal item

```yaml
item_templates:
    - id: dagger
      name: Dagger
      item_id: 3922
      weight: 1
      is_movable: true
      layer: OneHanded
      tags: [weapon]
```

`id` is the only required author-provided identifier. It must be unique across
all item template files. `item_id` is the UO art/tile id used for the item.

If `name` is missing, null, or empty, Moongate falls back to the UO tiledata
name for `item_id`. Tiledata names are raw client names, so they can contain UO
pluralization tokens such as `bread loa%ves/f%`.

## Reusable base item

Use `is_abstract: true` for base templates that should provide shared defaults
but not be created directly.

```yaml
item_templates:
    - id: base_weapon
      is_abstract: true
      weight: 4
      is_movable: true
      value:
          buy: 50
          sell: 25
      tags: [weapon]

    - id: katana
      base_item: base_weapon
      item_id: 5119
      layer: OneHanded
      value:
          buy: 85
          sell: 42
```

A child template inherits fields from `base_item` when the child does not set
its own value. `params` are merged by key, and a child param with the same key
overrides the parent value.

## Fields

| Field | Type | Default | Description |
| --- | --- | --- | --- |
| `id` | string | required | Unique template id used by loadouts, factories, admin UI, and API lookups. |
| `base_item` | string | null | Parent template id to inherit from. |
| `is_abstract` | boolean | `false` | Prevents direct item creation when true. Useful for base templates. |
| `name` | string | tiledata fallback | Display name shown in admin UI and copied onto created items. |
| `comment` | string | empty | Author note shown in template detail. |
| `item_id` | integer | `0` | UO item art/tile id. |
| `hue` | integer | `0` | Hue palette index. `0` means no hue override. |
| `weight` | integer | `0` | Weight for one unit of the item. |
| `amount` | integer | `1` | Initial stack amount. |
| `is_stackable` | boolean | `false` | Whether the item can stack. |
| `is_movable` | boolean | `false` | Whether players can move the item. Stored as a reserved custom property. |
| `gump_id` | integer | null | Optional container gump id. |
| `layer` | enum | null | Equipment or container layer, such as `OneHanded`, `TwoHanded`, `Shirt`, `Pants`, `Backpack`, or `Bank`. |
| `script_id` | string | empty | Script identifier for item behavior. |
| `rarity` | enum | `Common` | Rarity tier: `None`, `Common`, `Uncommon`, `Rare`, `Epic`, `Legendary`. |
| `visibility` | enum | `Player` | Minimum user level: `Player`, `GameMaster`, or `Administrator`. |
| `tags` | string list | empty | Search/filter labels for grouping templates. |
| `value` | object | null | Vendor economy values. See [Value](#value). |
| `params` | map | empty | Typed custom values copied to created item entities. See [Params](#params). |

## Value

`value` models the vendor economy price for a template.

```yaml
value:
    buy: 120
    sell: 60
```

- `buy` is the base gold amount a player pays when buying the item.
- `sell` is the base gold amount a player receives when selling the item.
- If `sell` is omitted, Moongate uses `buy / 2`.

The item rarity applies a multiplier to compute the effective value copied to
created item entities and shown in the admin UI.

| Rarity | Multiplier |
| --- | ---: |
| `None` | `1.0` |
| `Common` | `1.0` |
| `Uncommon` | `1.25` |
| `Rare` | `1.5` |
| `Epic` | `2.0` |
| `Legendary` | `3.0` |

Example:

```yaml
item_templates:
    - id: halberd
      item_id: 5182
      weight: 16
      layer: TwoHanded
      rarity: Rare
      value:
          buy: 120
          sell: 60
```

The admin API exposes both the base values and the effective values:

```json
{
  "value": {
    "buy": 120,
    "sell": 60,
    "rarityMultiplier": 1.5,
    "effectiveBuy": 180,
    "effectiveSell": 90
  }
}
```

## Params

Use `params` for typed custom properties that are not first-class template
fields.

```yaml
params:
    charges:
        type: Integer
        value: "0x10"
    dyeable:
        type: String
        value: "true"
```

Supported param types:

| Type | Value format |
| --- | --- |
| `String` | Raw string. |
| `Integer` | Decimal or `0x`-prefixed hexadecimal integer. |
| `Hue` | Decimal or `0x`-prefixed hexadecimal hue index. |
| `Serial` | Decimal or `0x`-prefixed hexadecimal serial. |

Do not declare a param named `is_movable`. That key is reserved because the
factory writes the template's `is_movable` flag into item custom properties.

## Validation rules

The template loader fails fast when it finds invalid template data.

- `id` must be present and non-empty.
- `id` must be unique, case-insensitively, across all loaded template files.
- `base_item` must reference an existing template.
- Base item chains cannot contain cycles.
- Non-string params must parse as decimal or `0x`-prefixed hexadecimal numbers.
- `is_movable` cannot be used as a custom param key.

## Checking templates

The admin UI exposes item templates as a read-only catalog. Use it to verify
loaded names, tags, hue, rarity, art, and value calculations after editing YAML.

The REST endpoints are administrator-only:

```text
GET /api/admin/item-templates
GET /api/admin/item-templates/{id}
```

The list endpoint supports search across ids, names, comments, script ids,
item ids, tags, and value fields.

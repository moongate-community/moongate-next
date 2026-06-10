---
title: Starter loadouts
---

# Starter Loadouts

Starter loadouts define the items given to a newly created character. A loadout
does not define item properties directly; it references existing
[item templates](item-templates.md) by id.

Use starter loadouts to configure default backpack contents, race-specific
equipment, and profession-specific starting items.

## File locations

The runtime starter loadout file is:

```text
moongate_data/templates/loadouts/starter.yaml
```

The bundled default starter loadout is:

```text
src/Moongate.Server/Assets/templates/loadouts/starter.yaml
```

During local development, edit the runtime file. When changing the default
catalog shipped with Moongate, keep the bundled file in sync.

Moongate currently loads one starter loadout file named `starter.yaml`.

## Minimal loadout

```yaml
starter_loadout:
    backpack_template: backpack
    base:
        backpack_items:
            - template: gold_coin
              amount: 1000
            - template: dagger
```

`backpack_template` is the item template id for the backpack that will be
equipped on the new character. Any `backpack_items` require a valid
`backpack_template`.

Each item entry references an item template by `template`. If `amount` is not
set, Moongate uses the referenced item template's default `amount`.

## Composition order

Starter loadouts are additive overlays. Moongate composes them in this order:

1. `backpack_template`
2. `base`
3. matching race section
4. matching profession section

Example:

```yaml
starter_loadout:
    backpack_template: backpack
    base:
        backpack_items:
            - template: gold_coin
              amount: 1000
            - template: candle
    races:
        human:
            equip_items:
                - template: plain_shirt
                  packet_hue: Shirt
                - template: plain_pants
                  packet_hue: Pants
                - template: leather_shoes
    professions:
        warrior:
            backpack_items:
                - template: broadsword
```

A human warrior receives the backpack, the base backpack items, the human
equipment, and the warrior backpack item.

## Sections

### Base

`base` applies to every new character.

```yaml
base:
    backpack_items:
        - template: gold_coin
          amount: 1000
    equip_items:
        - template: dagger
```

Use `base` for universal items such as gold, candles, starter tools, or default
clothing shared by all characters.

### Races

`races` contains optional overlays for character race.

Valid race keys are:

- `human`
- `elf`
- `gargoyle`

```yaml
races:
    gargoyle:
        equip_items:
            - template: plain_robe
              packet_hue: Shirt
```

Race sections are selected from the character creation race index:

| Race index | Race key |
| ---: | --- |
| `0` | `human` |
| `1` | `elf` |
| `2` | `gargoyle` |

### Professions

`professions` contains optional overlays for the selected profession.

```yaml
professions:
    mage:
        backpack_items:
            - template: spellbook
    warrior:
        backpack_items:
            - template: broadsword
```

Profession keys are matched case-insensitively against the profession catalog.
The bundled catalog includes professions such as `Samurai`, `Ninja`, `Paladin`,
`Necromancer`, `Warrior`, `Mage`, and `Blacksmith`.

## Item entries

Both `backpack_items` and `equip_items` use the same item entry shape.

| Field | Type | Default | Description |
| --- | --- | --- | --- |
| `template` | string | required | Item template id to create. The template must exist and must not be abstract. |
| `amount` | integer | template amount | Optional amount override. Must be at least `1`. |
| `packet_hue` | enum | `None` | Character creation hue source. Only valid on `equip_items`. |

Supported `packet_hue` values:

| Value | Behavior |
| --- | --- |
| `None` | Keep the item template hue. |
| `Shirt` | Apply the shirt hue from the character creation packet. |
| `Pants` | Apply the pants hue from the character creation packet. |

## Equipped items

Items in `equip_items` are equipped on the layer declared by their item
template.

```yaml
equip_items:
    - template: plain_shirt
      packet_hue: Shirt
```

The referenced item template must have a `layer`, for example `Shirt`,
`Pants`, `Shoes`, `OneHanded`, or `TwoHanded`.

Avoid layer conflicts. Moongate validates every possible base + race +
profession combination and fails startup if two equipped items would occupy the
same layer. The backpack template also counts as an equipped layer.

## Backpack items

Items in `backpack_items` are created and placed inside the equipped backpack.

```yaml
backpack_items:
    - template: gold_coin
      amount: 1000
    - template: spellbook
```

Do not use `packet_hue` on backpack items. If a backpack item needs a hue, set
it on the item template itself.

## Validation rules

Starter loadouts are validated at boot. A broken loadout prevents the server
from starting with invalid starter equipment.

- `starter.yaml` is optional. If it is missing, no starter loadout is configured.
- `starter_loadout` may be empty. In that case, no starter loadout is configured.
- `backpack_template` must exist when any section declares `backpack_items`.
- `backpack_template` must reference a concrete item template with a `layer`.
- Every `template` entry must be non-empty, known, and concrete.
- `equip_items` templates must have a `layer`.
- `backpack_items` cannot declare `packet_hue`.
- `amount`, when present, must be at least `1`.
- Race keys must be `human`, `elf`, or `gargoyle`.
- Profession keys must exist in the profession catalog.
- Race and profession keys cannot be duplicated by case, such as `Mage` and
  `mage` in the same map.
- No composed loadout may equip two items on the same layer.

## Common edits

### Add a profession item

```yaml
professions:
    mage:
        backpack_items:
            - template: spellbook
            - template: ginseng
              amount: 10
```

Create or update the referenced item template first if the item does not
already exist.

### Add race-specific clothing

```yaml
races:
    elf:
        equip_items:
            - template: plain_shirt
              packet_hue: Shirt
            - template: plain_pants
              packet_hue: Pants
            - template: leather_shoes
```

Use `packet_hue` only when the created character's selected clothing color
should override the template hue.

### Change starting gold

```yaml
base:
    backpack_items:
        - template: gold_coin
          amount: 1500
```

`amount` overrides only this loadout entry. It does not change the
`gold_coin` item template.

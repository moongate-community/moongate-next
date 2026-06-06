---
title: Bundled data assets
---

# Bundled Data Assets

Moongate ships editable YAML reference data under `src/Moongate.Server/Assets`.

- `Assets/data/uo_files/` contains static UO reference data used by UO data stores.
- `Assets/data/` contains server world data such as locations, regions,
  teleporters, weather, containers, decorations, signs, doors, and spawns.

At startup, missing bundled YAML files are copied from embedded resources into
the runtime data directory. Existing runtime files are never overwritten, so
operators can customize shard data after first boot.

After the data seed runs, Moongate registers lazy world data services. Each
service loads its YAML data on first query and can be reloaded through the
common data service contract.

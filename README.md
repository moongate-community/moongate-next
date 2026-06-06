# Moongate

## Overview

Short description goes here.

## Build

```bash
dotnet build Moongate.slnx
```

## Test

```bash
dotnet test Moongate.slnx
```

## Configuration

Moongate uses YAML for runtime configuration. The main server config is
`moongate.yaml` in the runtime config directory. Trusted plugins use
`plugin.yaml` in their plugin directory.

## Bundled Data Assets

Moongate ships editable YAML reference data under
`src/Moongate.Server/Assets`.

- `Assets/data/uo_files/` contains static UO reference data used by the UO data stores.
- `Assets/data/` contains server world data such as locations, regions,
  teleporters, weather, containers, decorations, signs, doors, and spawns.

At startup, missing bundled YAML files are copied from embedded resources into
the runtime data directory. Existing runtime files are never overwritten, so
operators can customize shard data after first boot.

After the data seed runs, Moongate loads the runtime `data/` tree into world
data services for doors, spawns, teleporters, regions, weather, containers,
locations, names, professions, signs, decorations, and mount conversion data.

## License

MIT - see [LICENSE](LICENSE).

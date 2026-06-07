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

## Documentation

Moongate uses DocFX for documentation and API reference generation.

```bash
dotnet tool restore
dotnet build Moongate.slnx --configuration Release
dotnet tool run docfx docs/docfx/docfx.json
```

The generated site is written to `docs/docfx/_site`. GitHub Pages deployment is
handled by the `Docs` workflow.

## Configuration

Moongate uses YAML for runtime configuration. The main server config is
`moongate.yaml` in the runtime config directory. Trusted plugins use
`plugin.yaml` in their plugin directory.

The UO starting location uses a named map facet and compact world coordinates:

```yaml
uo:
  client_files_directory: ~/uo
  starting_map: Trammel
  starting: 1496,1628,10
  starting_city: Britain
```

## Commands

Moongate exposes one command registry for built-in server commands and trusted
plugins. Commands can be executed from the interactive server console or from
in-game speech with the `.` prefix, for example `.help`.

Plugins register commands during `Configure` through
`PluginContext.RegisterCommand`. No source generator is required; command
registration is explicit and can choose console-only, in-game-only, or shared
sources.

Lua scripts can register commands through the `commands` module:

```lua
commands.register("hello", "all", "Greets the caller.", function(ctx)
    return "hello " .. ctx.args[1]
end)
```

## Bundled Data Assets

Moongate ships editable YAML reference data under
`src/Moongate.Server/Assets`.

- `Assets/data/uo_files/` contains static UO reference data used by the UO data stores.
- `Assets/data/` contains server world data such as locations, regions,
  teleporters, weather, containers, decorations, signs, doors, and spawns.

At startup, missing bundled YAML files are copied from embedded resources into
the runtime data directory. Existing runtime files are never overwritten, so
operators can customize shard data after first boot.

After the data seed runs, Moongate registers lazy world data services for doors,
spawns, teleporters, regions, weather, containers, locations, names,
professions, signs, decorations, and mount conversion data. Each service loads
its YAML data on first query and can be reloaded through the common data service
contract.

## License

Apache-2.0 - see [LICENSE](LICENSE). Some source files carry separate license
notices that apply to those files.

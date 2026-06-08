---
title: Runtime configuration
---

# Runtime Configuration

Moongate loads one root YAML config file during boot. The runtime file name is
`moongate.yaml`.

The server does not currently look for `moongate.yml`; keep the `.yaml`
extension unless the runtime constant is changed.

## Location

The config path is:

```text
<root>/config/moongate.yaml
```

The root directory is resolved in this order:

1. `--root-directory` command line option.
2. `MOONGATE_ROOT` environment variable.
3. `NIGHTHEAVEN_ROOT` environment variable, kept as a legacy fallback.
4. `./moongate` under the current working directory.

If `<root>/config/nightheaven.yaml` exists and `moongate.yaml` does not, the
legacy file is still used. New installs should use `moongate.yaml`.

## Boot Behavior

Every module registers its config section before the file is loaded. On startup,
Moongate:

1. Creates the config directory when missing.
2. Creates `moongate.yaml` with default sections when missing.
3. Adds any newly registered missing section back to an existing file.
4. Binds known sections into DI as typed config objects.
5. Validates sections that implement validation, such as `uo`.

Unknown top-level sections are ignored with a warning. Malformed YAML or invalid
validated values fail startup.

## Example

```yaml
logger:
  level: Information
  log_packets: false
  write_to_file: false
  file_name: moongate.log

game_loop:
  idle_cpu_enabled: true
  idle_sleep_ms: 1

timing:
  tick_duration: "00:00:00.0080000"
  wheel_size: 512

metrics:
  refresh_interval: "00:00:05"
  log_enabled: true
  log_interval: "00:01:00"

persistence:
  autosave_interval: "00:05:00"
  snapshot_file_name: world.snapshot.bin
  journal_file_name: world.journal.bin
  enable_file_lock: true

web:
  jwt:
    issuer: Moongate
    audience: Moongate.Web
    signing_key: MOONGATE_DEVELOPMENT_ONLY_SIGNING_KEY_CHANGE_ME_2026
    access_token_minutes: 15
    refresh_token_days: 14
    rotate_refresh_tokens: true

network:
  port: 2593
  ping_server_enabled: true
  ping_server_port: 12000
  max_pending_buffer_bytes: 65536
  max_declared_packet_length: 16384
  max_packets_per_drain: 256
  max_outgoing_packets_per_drain: 256

uo:
  client_files_directory: ~/uo
  starting_map: Trammel
  starting: 1496,1628,10
  starting_city: Britain
```

Plugins may add extra top-level sections. Those sections are only valid after the
owning plugin is loaded and has registered them.

## Value Formats

YAML property names use `snake_case`, matching the generated default file.

`TimeSpan` values use the .NET constant format, for example:

```yaml
autosave_interval: "00:05:00"
tick_duration: "00:00:00.0080000"
```

`Point3D` values use compact `x,y,z` syntax:

```yaml
starting: 1496,1628,10
```

Enums use their names:

```yaml
logger:
  level: Debug

uo:
  starting_map: Felucca
```

Supported UO map facets are `Felucca`, `Trammel`, `Ilshenar`, `Malas`, `Tokuno`,
and `TerMur`.

## Sections

### `logger`

| Key | Default | Description |
|---|---:|---|
| `level` | `Information` | Minimum log level. Supported values are `None`, `Trace`, `Debug`, `Information`, `Warning`, `Error`, and `Critical`. |
| `log_packets` | `false` | Enables packet-level network logging. |
| `write_to_file` | `false` | Writes logs to a file under `<root>/logs`. |
| `file_name` | `moongate.log` | Log file name used when `write_to_file` is enabled. |

### `game_loop`

| Key | Default | Description |
|---|---:|---|
| `idle_cpu_enabled` | `true` | Sleeps briefly when a game-loop tick has no work. |
| `idle_sleep_ms` | `1` | Sleep duration in milliseconds when idle CPU mode is enabled. |

### `timing`

| Key | Default | Description |
|---|---:|---|
| `tick_duration` | `00:00:00.0080000` | Timer wheel granularity. Timers cannot be more precise than this value. |
| `wheel_size` | `512` | Number of timer wheel slots. A power of two is recommended. |

### `metrics`

| Key | Default | Description |
|---|---:|---|
| `refresh_interval` | `00:00:05` | How often metric providers are polled. |
| `log_enabled` | `true` | Enables periodic metrics logging. |
| `log_interval` | `00:01:00` | How often the latest metrics snapshot is logged. |

### `persistence`

| Key | Default | Description |
|---|---:|---|
| `autosave_interval` | `00:05:00` | How often the world snapshot is written and the journal is trimmed. |
| `snapshot_file_name` | `world.snapshot.bin` | Snapshot file name under `<root>/save`. |
| `journal_file_name` | `world.journal.bin` | Journal file name under `<root>/save`. |
| `enable_file_lock` | `true` | Opens the journal file with a per-process lock. |

### `web`

| Key | Default | Description |
|---|---:|---|
| `jwt.issuer` | `Moongate` | Expected JWT issuer. |
| `jwt.audience` | `Moongate.Web` | Expected JWT audience. |
| `jwt.signing_key` | development key | HMAC signing key for access tokens. It must be at least 32 UTF-8 bytes. Replace the development default before production use. |
| `jwt.access_token_minutes` | `15` | Access token lifetime in minutes. |
| `jwt.refresh_token_days` | `14` | Refresh token lifetime in days. |
| `jwt.rotate_refresh_tokens` | `true` | Revokes the used refresh token and returns a new one on every refresh. |

The default `jwt.signing_key` is intentionally present so development servers
can boot without manual setup. Production deployments should always configure a
unique secret value.

### `network`

| Key | Default | Description |
|---|---:|---|
| `port` | `2593` | TCP game listener port. |
| `ping_server_enabled` | `true` | Starts the UDP ping echo server used by UO launchers. |
| `ping_server_port` | `12000` | UDP ping echo server port. |
| `max_pending_buffer_bytes` | `65536` | Maximum buffered unparsed bytes per session before disconnect. |
| `max_declared_packet_length` | `16384` | Maximum variable packet length accepted from a client. |
| `max_packets_per_drain` | `256` | Maximum ingress queue items drained per loop wake-up. |
| `max_outgoing_packets_per_drain` | `256` | Maximum outbound packets drained per outbound loop wake-up. |

### `uo`

| Key | Default | Description |
|---|---:|---|
| `client_files_directory` | `~/uo` | Directory containing the UO client files. Supports `~` and environment variables. |
| `starting_map` | `Trammel` | Facet used for newly created characters. |
| `starting` | `1496,1628,10` | Starting world coordinates in `x,y,z` format. |
| `starting_city` | `Britain` | Display name for the starting city. |

The `uo` section validates at boot. The configured client files directory must
exist and must contain at least `tiledata.mul`.

---
title: Runtime configuration
---

# Runtime Configuration

Moongate uses YAML for runtime configuration. The main server config is
`moongate.yaml` in the runtime config directory. Trusted plugins use a
`plugin.yaml` file in their plugin directory.

The UO starting location uses a named map facet and compact world coordinates:

```yaml
uo:
  client_files_directory: ~/uo
  starting_map: Trammel
  starting: 1496,1628,10
  starting_city: Britain
```

Configuration sections are registered by each runtime module before the root
YAML file is loaded, so plugin-provided config sections can participate in boot.

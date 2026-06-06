---
title: Plugins
---

# Plugins

Moongate loads trusted .NET plugins from the runtime plugins directory. Each
plugin receives a `PluginContext` during startup.

The plugin context provides:

- plugin directory paths
- access to plugin YAML config
- command registration through `RegisterCommand`

Plugin command registration is explicit and does not require a source generator.

---
title: Lua scripting
---

# Lua Scripting

Moongate exposes core Lua modules during script engine startup:

- `events`
- `commands`
- `log`
- `random`
- `timers`

Lua callbacks receive stable snake_case payloads. For command callbacks, the
payload includes fields such as `command`, `args`, `source`, `session_id`, and
`is_in_game`. When the command is associated with a player session, a nested
`player` table is included.

Timer callbacks receive `name` and `repeat` fields.

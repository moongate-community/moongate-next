---
title: Commands
---

# Commands

Moongate exposes one command registry for built-in server code and trusted
plugins. Commands can run from the interactive server console or from in-game
speech with the `.` prefix.

Built-in commands are registered explicitly at boot. Plugins can register their
own commands during `Configure` through `PluginContext.RegisterCommand`.

Lua scripts can also register commands through the `commands` module:

```lua
commands.register("hello", "all", "Greets the caller.", function(ctx)
    return "hello " .. ctx.args[1]
end)
```

Command sources can be `console`, `ingame`, or `all`.

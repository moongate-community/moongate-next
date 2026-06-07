---
title: Plugins
---

# Plugins

Moongate loads trusted .NET plugins from the runtime plugins directory during
server startup. A plugin is a class library that contains exactly one concrete
type implementing `IMoongatePlugin`.

Plugins can:

- register services in the DryIoc container
- load typed `plugin.yaml` configuration
- register console and in-game commands
- declare config sections that the main server config loader can bind
- register Lua modules and other extension points exposed by Moongate packages

This tutorial builds a minimal command plugin and installs it into a Moongate
runtime root.

## Prerequisites

Use the same .NET SDK as Moongate and keep a local checkout of the repository:

```bash
export MOONGATE_REPO=/path/to/moongate-next
export MOONGATE_ROOT=$HOME/moongate
```

`MOONGATE_ROOT` is the server runtime directory. If it is not set and no
`--root-directory` argument is passed, Moongate uses `./moongate` under the
current working directory.

## Create The Project

Create a class library targeting `net10.0`:

```bash
mkdir -p ~/moongate-plugins
cd ~/moongate-plugins
dotnet new classlib --framework net10.0 --name Moongate.Tutorial.HelloPlugin
cd Moongate.Tutorial.HelloPlugin
```

Reference the plugin API from the Moongate checkout:

```bash
dotnet add reference "$MOONGATE_REPO/src/Moongate.Plugins/Moongate.Plugins.csproj"
```

Until Moongate packages are published to NuGet, project references are the
recommended local development path. After NuGet packages exist, replace the
project reference with the matching package reference.

## Add Plugin Configuration

Delete the generated `Class1.cs`, then create `HelloPluginConfig.cs`:

```csharp
namespace Moongate.Tutorial.HelloPlugin;

public sealed class HelloPluginConfig
{
    public string Greeting { get; set; } = "Hello";

    public string DefaultName { get; set; } = "Britannia";
}
```

Moongate stores this configuration in `plugin.yaml` inside the plugin runtime
directory. If `plugin.yaml` is missing, `PluginContext.LoadConfig` writes the
defaults.

## Implement The Plugin

Create `HelloPlugin.cs`:

```csharp
using DryIoc;
using Moongate.Abstractions.Types.Commands;
using Moongate.Plugins.Data;
using Moongate.Plugins.Interfaces.Plugins;

namespace Moongate.Tutorial.HelloPlugin;

public sealed class HelloPlugin : IMoongatePlugin
{
    public PluginMetadata Metadata { get; } = new()
    {
        Id = "moongate.tutorial.hello",
        Name = "Hello Tutorial Plugin",
        Version = new(1, 0, 0),
        Author = "Moongate Community",
        Description = "Minimal tutorial plugin that registers a command."
    };

    public void Configure(IContainer container, PluginContext context)
    {
        _ = container;

        var config = context.LoadConfig(() => new HelloPluginConfig());

        context.RegisterCommand(
            "hello_plugin|hplugin",
            commandContext =>
            {
                var target = commandContext.Arguments.Count == 0
                                 ? config.DefaultName
                                 : string.Join(" ", commandContext.Arguments);

                commandContext.Print("{0}, {1}!", config.Greeting, target);

                return Task.CompletedTask;
            },
            "Prints a greeting from the tutorial plugin.",
            CommandSourceType.All
        );
    }
}
```

`Metadata.Id` is the stable plugin identity. Use lowercase dotted names, and
make the runtime directory match the id. `CommandSourceType.All` makes the
command available from both the server console and in-game speech.

## Build And Publish

Publish the plugin:

```bash
dotnet publish --configuration Release -o ./publish
```

Install it under the Moongate runtime plugins directory:

```bash
PLUGIN_DIR="$MOONGATE_ROOT/plugins/moongate.tutorial.hello"
rm -rf "$PLUGIN_DIR"
mkdir -p "$PLUGIN_DIR"
cp -a ./publish/. "$PLUGIN_DIR/"
```

The loader scans each direct child directory under `plugins/`, loads the `.dll`
files in that directory, and requires exactly one concrete `IMoongatePlugin`
implementation.

## Run And Verify

Start Moongate with the same runtime root:

```bash
cd "$MOONGATE_REPO"
dotnet run --project src/Moongate.Server/Moongate.Server.csproj -- --root-directory "$MOONGATE_ROOT"
```

After the server is ready, run the command from the console:

```text
MG> hello_plugin
Hello, Britannia!
MG> hello_plugin Luna
Hello, Luna!
```

The short alias works too:

```text
MG> hplugin
Hello, Britannia!
```

Moongate also creates the plugin config file if it did not exist:

```bash
cat "$MOONGATE_ROOT/plugins/moongate.tutorial.hello/plugin.yaml"
```

Expected content:

```yaml
greeting: Hello
default_name: Britannia
```

Edit `plugin.yaml`, restart the server, and the command will use the new values.

## Common Failure Modes

If startup fails with a message that the plugin directory does not contain a
plugin assembly, confirm that the published `.dll` files were copied into the
plugin directory itself, not into a nested `publish/` folder.

If startup fails because multiple plugin implementations were found, keep only
one concrete `IMoongatePlugin` type in the plugin output.

If command registration fails, make sure the plugin is loaded after the command
system is registered. Built-in Moongate startup does this automatically; custom
test harnesses must register an `ICommandRegistry` before loading plugins.

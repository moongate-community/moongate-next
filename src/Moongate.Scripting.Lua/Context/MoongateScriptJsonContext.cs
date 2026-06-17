using System.Text.Json.Serialization;
using Moongate.Scripting.Lua.Data.Luarc;

namespace Moongate.Scripting.Lua.Context;

[JsonSerializable(typeof(LuarcConfig))]
[JsonSerializable(typeof(LuarcRuntimeConfig))]
[JsonSerializable(typeof(LuarcWorkspaceConfig))]
[JsonSerializable(typeof(LuarcDiagnosticsConfig))]
[JsonSerializable(typeof(LuarcCompletionConfig))]
[JsonSerializable(typeof(LuarcFormatConfig))]
/// <summary>
/// JSON serialization context for Lua scripting configuration types.
/// </summary>
public partial class MoongateScriptJsonContext : JsonSerializerContext
{
}

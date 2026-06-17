namespace Moongate.Scripting.Lua.Data.Config;

public sealed record LuaEngineConfig
{
    public LuaEngineConfig(string luarcDirectory, string scriptsDirectory, string engineVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(luarcDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptsDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(engineVersion);

        LuarcDirectory = luarcDirectory;
        ScriptsDirectory = scriptsDirectory;
        EngineVersion = engineVersion;
    }

    public string LuarcDirectory { get; }
    public string ScriptsDirectory { get; }
    public string EngineVersion { get; }
}

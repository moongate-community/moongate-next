namespace Moongate.Server.Bootstrap;

public sealed class MoongateBootstrapOptions
{
    public MoongateBootstrapOptions(string[] args, string? rootDirectory, bool debug, bool showHeader)
    {
        Args = args;
        RootDirectory = rootDirectory;
        Debug = debug;
        ShowHeader = showHeader;
    }

    public string[] Args { get; }

    public bool Debug { get; }

    public string? RootDirectory { get; }

    public bool ShowHeader { get; }
}

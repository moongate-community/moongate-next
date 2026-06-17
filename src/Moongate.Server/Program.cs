using ConsoleAppFramework;
using Moongate.Server.Bootstrap;

await ConsoleApp.RunAsync(
    args,
    async (CancellationToken cancellationToken, string? rootDirectory = null, bool debug = false, bool header = true) =>
    {
        await MoongateBootstrap.RunAsync(
            new MoongateBootstrapOptions(args, rootDirectory, debug, header),
            cancellationToken
        );
    }
);

using System.Reflection;
using Moongate.Core.Utils;

namespace Moongate.Server.Bootstrap.Internal;

internal static class HeaderPrinter
{
    public static void Print(MoongateBootstrapContext context, bool showHeader)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (showHeader)
        {
            var headerContent = ResourceUtils.GetEmbeddedResourceString(
                Assembly.GetExecutingAssembly(),
                "Assets/header.txt"
            );

            Console.WriteLine(headerContent);
        }

        Console.WriteLine($"Moongate UO Server v{VersionUtils.GetVersion()}");
        Console.WriteLine($"Root Directory: {context.Directories.Root}");
    }
}

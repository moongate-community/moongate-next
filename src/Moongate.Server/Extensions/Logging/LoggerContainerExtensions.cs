using DryIoc;
using Moongate.Abstractions.Data.Logging;
using Moongate.Abstractions.Extensions.DryIoc;

namespace Moongate.Server.Extensions.Logging;

/// <summary>
/// DryIoc-native registration helpers for logging configuration.
/// </summary>
public static class LoggerContainerExtensions
{
    extension(IContainer container)
    {
        /// <summary>
        /// Registers the logger TOML config section.
        /// </summary>
        public IContainer AddMoongateLogging()
        {
            container.RegisterConfigSection("logger", () => new LoggerConfig());

            return container;
        }
    }
}

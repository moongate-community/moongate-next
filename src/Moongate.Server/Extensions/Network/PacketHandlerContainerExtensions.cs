using DryIoc;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Server.Data.Events;
using Moongate.Server.Services.Network;

namespace Moongate.Server.Extensions.Network;

/// <summary>
/// DryIoc-native registration helpers for the server packet dispatcher.
/// </summary>
public static class PacketHandlerContainerExtensions
{
    extension(IContainer container)
    {
        /// <summary>
        /// Registers the packet dispatcher on the event-bus tick path.
        /// </summary>
        public IContainer AddMoongatePacketHandlers()
        {
            container.AddTickEventHandler<PacketDispatchHandler, PacketReceivedEvent>();

            return container;
        }
    }
}

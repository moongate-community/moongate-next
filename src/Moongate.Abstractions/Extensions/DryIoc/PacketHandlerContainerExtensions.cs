using System.Reflection;
using DryIoc;
using Moongate.Abstractions.Interfaces.Network;

namespace Moongate.Abstractions.Extensions.DryIoc;

/// <summary>
/// DryIoc-native registration helpers for typed packet handlers.
/// </summary>
public static class PacketHandlerContainerExtensions
{
    extension(IContainer container)
    {
        /// <summary>
        /// Registers a typed packet handler.
        /// </summary>
        public IContainer AddPacketHandler<THandler, TPacket>()
            where THandler : class, IPacketHandler<TPacket>
            where TPacket : IGameNetworkPacket
        {
            container.Register<THandler>(Reuse.Singleton);
            container.RegisterMapping<IPacketHandler<TPacket>, THandler>();

            return container;
        }

        /// <summary>
        /// Scans an assembly for typed packet handlers and registers each mapping.
        /// </summary>
        public IContainer AddPacketHandlersFromAssembly(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly);

            foreach (var handlerType in assembly.GetTypes())
            {
                if (handlerType.IsAbstract || !handlerType.IsClass)
                {
                    continue;
                }

                var handlerInterfaces = handlerType.GetInterfaces()
                                                   .Where(
                                                       static interfaceType =>
                                                           interfaceType.IsGenericType &&
                                                           interfaceType.GetGenericTypeDefinition() ==
                                                           typeof(IPacketHandler<>)
                                                   )
                                                   .ToArray();

                if (handlerInterfaces.Length == 0)
                {
                    continue;
                }

                container.Register(handlerType, Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Keep);

                foreach (var handlerInterface in handlerInterfaces)
                {
                    container.RegisterMapping(handlerInterface, handlerType);
                }
            }

            return container;
        }
    }
}

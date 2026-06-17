using System.Reflection;
using DryIoc;
using Moongate.Abstractions.Attributes;
using Moongate.Abstractions.Interfaces.Network;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Abstractions.Extensions.DryIoc;

/// <summary>
///     DryIoc-native registration helpers for typed packet handlers.
/// </summary>
public static class PacketHandlerContainerExtensions
{
    private static readonly ILogger _logger = Log.ForContext(typeof(PacketHandlerContainerExtensions));

    extension(IContainer container)
    {
        /// <summary>
        ///     Registers a typed packet handler.
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
        ///     Scans an assembly for classes marked with <see cref="RegisterPacketHandlerAttribute" /> and
        ///     registers each (mapping every <c>IPacketHandler&lt;&gt;</c> interface it implements).
        /// </summary>
        public IContainer AddPacketHandlersFromAssembly(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly);

            return container.AddPacketHandlers(assembly.GetTypes());
        }

        /// <summary>
        ///     Registers every class in <paramref name="handlerTypes" /> that is marked with
        ///     <see cref="RegisterPacketHandlerAttribute" /> as a singleton, mapping each
        ///     <c>IPacketHandler&lt;&gt;</c> interface it implements. Throws if a marked class implements none.
        /// </summary>
        internal IContainer AddPacketHandlers(IEnumerable<Type> handlerTypes)
        {
            ArgumentNullException.ThrowIfNull(handlerTypes);

            foreach (var handlerType in handlerTypes)
            {
                if (handlerType.IsAbstract ||
                    !handlerType.IsClass ||
                    !handlerType.IsDefined(typeof(RegisterPacketHandlerAttribute), false))
                {
                    continue;
                }

                var handlerInterfaces = handlerType.GetInterfaces()
                    .Where(static interfaceType =>
                        interfaceType.IsGenericType &&
                        interfaceType.GetGenericTypeDefinition() ==
                        typeof(IPacketHandler<>)
                    )
                    .ToArray();

                if (handlerInterfaces.Length == 0)
                {
                    throw new InvalidOperationException(
                        $"{handlerType.FullName} is marked [RegisterPacketHandler] but implements no IPacketHandler<>."
                    );
                }

                container.Register(handlerType, Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Keep);

                foreach (var handlerInterface in handlerInterfaces)
                {
                    container.RegisterMapping(handlerInterface, handlerType);
                }

                _logger.Information(
                    "Registered packet handler {Handler} for {Packets}",
                    handlerType.Name,
                    string.Join(", ", handlerInterfaces.Select(static i => i.GetGenericArguments()[0].Name))
                );
            }

            return container;
        }
    }
}

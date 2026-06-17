using DryIoc;
using Moongate.Abstractions.Data.Seed;
using Moongate.Core.Extensions.Container;

namespace Moongate.Abstractions.Extensions.DryIoc;

/// <summary>
///     DryIoc-native registration helpers for Moongate seed declarations.
/// </summary>
public static class SeedContainerExtensions
{
    extension(IContainer container)
    {
        /// <summary>
        ///     Adds a boot-time seed action.
        /// </summary>
        public IContainer AddSeed(SeedAction action)
        {
            container.AddToRegisterTypedList(action);

            return container;
        }
    }
}

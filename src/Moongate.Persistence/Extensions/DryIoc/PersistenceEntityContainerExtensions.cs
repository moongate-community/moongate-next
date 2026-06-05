using DryIoc;
using Moongate.Core.Extensions.Container;
using Moongate.Persistence.Data;

namespace Moongate.Persistence.Extensions.DryIoc;

/// <summary>
/// DryIoc-native registration helpers for Moongate persistence entity declarations.
/// </summary>
public static class PersistenceEntityContainerExtensions
{
    extension(IContainer container)
    {
        /// <summary>
        /// Registers a persisted entity type consumed by the persistence service at boot.
        /// </summary>
        /// <param name="typeId">Stable numeric identifier for the entity kind.</param>
        /// <param name="schemaVersion">Version of the persisted entity schema.</param>
        /// <param name="keySelector">Selects the entity key.</param>
        public IContainer RegisterPersistenceEntity<TEntity, TKey>(
            ushort typeId,
            int schemaVersion,
            Func<TEntity, TKey> keySelector
        )
            where TKey : notnull
        {
            var descriptor = new PersistenceEntityDescriptor<TEntity, TKey>(
                typeId,
                typeof(TEntity).Name,
                schemaVersion,
                keySelector
            );
            container.AddToRegisterTypedList(new PersistenceEntityRegistration(descriptor));

            return container;
        }
    }
}

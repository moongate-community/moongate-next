using Moongate.Persistence.Data;
using Moongate.Persistence.Interfaces.Persistence;

namespace Moongate.Persistence.Services.Persistence;

/// <summary>
/// Default in-memory <see cref="IPersistenceEntityRegistry" />.
/// </summary>
public sealed class PersistenceEntityRegistry : IPersistenceEntityRegistry
{
    private readonly Dictionary<ushort, IPersistenceEntityDescriptor> _byTypeId = [];
    private readonly Dictionary<(Type Entity, Type Key), IPersistenceEntityDescriptor> _byClrTypes = [];

    public bool IsFrozen { get; private set; }

    public void Freeze()
        => IsFrozen = true;

    public IPersistenceEntityDescriptor GetDescriptor(ushort typeId)
    {
        if (_byTypeId.TryGetValue(typeId, out var descriptor))
        {
            return descriptor;
        }

        throw new InvalidOperationException($"No persistence descriptor registered for type id {typeId}.");
    }

    public IPersistenceEntityDescriptor<TEntity, TKey> GetDescriptor<TEntity, TKey>()
    {
        if (_byClrTypes.TryGetValue((typeof(TEntity), typeof(TKey)), out var descriptor))
        {
            return (IPersistenceEntityDescriptor<TEntity, TKey>)descriptor;
        }

        throw new InvalidOperationException(
            $"No persistence descriptor registered for {typeof(TEntity).Name}/{typeof(TKey).Name}."
        );
    }

    public IReadOnlyCollection<IPersistenceEntityDescriptor> GetRegisteredDescriptors()
        => _byTypeId.Values.ToArray();

    public bool IsRegistered(ushort typeId)
        => _byTypeId.ContainsKey(typeId);

    public bool IsRegistered<TEntity, TKey>()
        => _byClrTypes.ContainsKey((typeof(TEntity), typeof(TKey)));

    public void Register<TEntity, TKey>(PersistenceEntityDescriptor<TEntity, TKey> descriptor)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        if (IsFrozen)
        {
            throw new InvalidOperationException("Cannot register entities after the registry is frozen.");
        }

        if (!_byTypeId.TryAdd(descriptor.TypeId, descriptor))
        {
            throw new InvalidOperationException($"Type id {descriptor.TypeId} is already registered.");
        }

        _byClrTypes[(typeof(TEntity), typeof(TKey))] = descriptor;
    }
}

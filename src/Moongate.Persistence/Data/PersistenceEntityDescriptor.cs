using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using Moongate.Core.Interfaces.Ids;
using Moongate.Persistence.Formatters;
using Moongate.Persistence.Interfaces.Internal;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.Persistence.Internal;

namespace Moongate.Persistence.Data;

/// <summary>
/// Default descriptor for a persisted entity kind. Serializes via a composite MessagePack resolver
/// (<see cref="SerialMessagePackFormatter" /> first, then contractless), so plain POCO entities —
/// including those with <c>Serial</c> fields/keys — need no attributes.
/// </summary>
public sealed class PersistenceEntityDescriptor<TEntity, TKey>
    : IPersistenceEntityDescriptor<TEntity, TKey>, IInternalEntityApplier
    where TKey : notnull
{
    internal static readonly MessagePackSerializerOptions SerializerOptions =
        MessagePackSerializerOptions.Standard.WithResolver(
            CompositeResolver.Create(
                new IMessagePackFormatter[]
                {
                    SerialMessagePackFormatter.Instance,
                    AutoInt32MessagePackFormatter.Instance,
                    AutoInt64MessagePackFormatter.Instance
                },
                new IFormatterResolver[] { ContractlessStandardResolver.Instance }
            )
        );

    private readonly Func<TEntity, TKey> _keySelector;

    public PersistenceEntityDescriptor(
        ushort typeId,
        string typeName,
        int schemaVersion,
        Func<TEntity, TKey> keySelector
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);
        ArgumentNullException.ThrowIfNull(keySelector);

        TypeId = typeId;
        TypeName = typeName;
        SchemaVersion = schemaVersion;
        _keySelector = keySelector;
    }

    public ushort TypeId { get; }
    public string TypeName { get; }
    public int SchemaVersion { get; }
    public Type EntityType => typeof(TEntity);
    public Type KeyType => typeof(TKey);

    public TEntity Clone(TEntity entity)
        => DeserializeEntity(SerializeEntity(entity));

    public IReadOnlyList<TEntity> DeserializeBucket(byte[] payload)
        => MessagePackSerializer.Deserialize<List<TEntity>>(payload, SerializerOptions) ?? [];

    public TEntity DeserializeEntity(byte[] payload)
        => MessagePackSerializer.Deserialize<TEntity>(payload, SerializerOptions)!;

    public TKey DeserializeKey(byte[] payload)
        => MessagePackSerializer.Deserialize<TKey>(payload, SerializerOptions)!;

    public TKey GetKey(TEntity entity)
        => _keySelector(entity);

    public byte[] SerializeBucket(IReadOnlyCollection<TEntity> entities)
        => MessagePackSerializer.Serialize(entities, SerializerOptions);

    public byte[] SerializeEntity(TEntity entity)
        => MessagePackSerializer.Serialize(entity, SerializerOptions);

    public byte[] SerializeKey(TKey key)
        => MessagePackSerializer.Serialize(key, SerializerOptions);

    void IInternalEntityApplier.ApplyRemove(PersistenceStateStore stateStore, byte[] payload)
        => stateStore.GetBucket<TEntity, TKey>(TypeId).Remove(DeserializeKey(payload));

    void IInternalEntityApplier.ApplyUpsert(PersistenceStateStore stateStore, byte[] payload)
    {
        var entity = DeserializeEntity(payload);
        var key = GetKey(entity);
        stateStore.GetBucket<TEntity, TKey>(TypeId)[key] = entity;

        if (key is IAutoIncrementKey autoKey)
        {
            stateStore.TrackKey(TypeId, autoKey);
        }
    }

    EntitySnapshotBucket? IInternalEntityApplier.CaptureBucket(PersistenceStateStore stateStore)
    {
        var entities = stateStore.GetBucket<TEntity, TKey>(TypeId).Values.ToArray();

        if (entities.Length == 0)
        {
            return null;
        }

        return new()
        {
            TypeId = TypeId,
            TypeName = TypeName,
            SchemaVersion = SchemaVersion,
            Payload = SerializeBucket(entities)
        };
    }

    int IInternalEntityApplier.Count(PersistenceStateStore stateStore)
        => stateStore.GetBucket<TEntity, TKey>(TypeId).Count;

    void IInternalEntityApplier.LoadBucket(PersistenceStateStore stateStore, EntitySnapshotBucket bucket)
    {
        var typed = stateStore.GetBucket<TEntity, TKey>(TypeId);
        typed.Clear();

        foreach (var entity in DeserializeBucket(bucket.Payload))
        {
            var key = GetKey(entity);
            typed[key] = entity;

            if (key is IAutoIncrementKey autoKey)
            {
                stateStore.TrackKey(TypeId, autoKey);
            }
        }
    }
}

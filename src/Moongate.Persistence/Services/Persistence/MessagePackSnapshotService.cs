using MessagePack;
using MessagePack.Resolvers;
using Moongate.Persistence.Data;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.Persistence.Internal;

namespace Moongate.Persistence.Services.Persistence;

/// <summary>
/// Stores each registered entity type as its own MessagePack snapshot file
/// (<c>&lt;TypeName&gt;&lt;suffix&gt;</c> under the save directory), written atomically via temp +
/// rename and verified by a payload checksum on load.
/// </summary>
public sealed class MessagePackSnapshotService : ISnapshotService, IDisposable
{
    private static readonly MessagePackSerializerOptions _options = ContractlessStandardResolver.Options;

    private static readonly char[] _invalidTypeNameChars =
        [.. Path.GetInvalidFileNameChars(), Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];

    private readonly SemaphoreSlim _ioLock = new(1, 1);
    private readonly string _directory;
    private readonly string _suffix;

    public MessagePackSnapshotService(string saveDirectory, string fileSuffix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(saveDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileSuffix);

        _directory = Path.GetFullPath(saveDirectory);
        _suffix = fileSuffix;

        Directory.CreateDirectory(_directory);
    }

    public void Dispose()
        => _ioLock.Dispose();

    public async ValueTask SaveBucketAsync(
        EntitySnapshotBucket bucket,
        long lastSequenceId,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(bucket);

        var envelope = new SnapshotFileEnvelope
        {
            Version = 1,
            LastSequenceId = lastSequenceId,
            Checksum = ChecksumUtils.Compute(bucket.Payload),
            Bucket = bucket
        };

        var path = PathFor(bucket.TypeName);
        var tempPath = path + ".tmp";
        var bytes = MessagePackSerializer.Serialize(envelope, _options, cancellationToken);

        await _ioLock.WaitAsync(cancellationToken);

        try
        {
            await using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.WriteAsync(bytes, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }

            File.Move(tempPath, path, true);
        }
        finally
        {
            _ioLock.Release();
        }
    }

    public async ValueTask<PersistedBucket?> LoadBucketAsync(string typeName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);

        var path = PathFor(typeName);

        await _ioLock.WaitAsync(cancellationToken);

        try
        {
            if (!File.Exists(path))
            {
                return null;
            }

            var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
            var envelope = MessagePackSerializer.Deserialize<SnapshotFileEnvelope>(bytes, _options, cancellationToken);

            if (envelope is null
                || ChecksumUtils.Compute(envelope.Bucket.Payload) != envelope.Checksum
                || !string.Equals(envelope.Bucket.TypeName, typeName, StringComparison.Ordinal))
            {
                return null;
            }

            return new PersistedBucket(envelope.Bucket, envelope.LastSequenceId);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            // A corrupt/unreadable snapshot file is treated as "no snapshot"; the journal replay
            // rebuilds from the last good state.
            return null;
        }
        finally
        {
            _ioLock.Release();
        }
    }

    public async ValueTask DeleteBucketAsync(string typeName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);

        var path = PathFor(typeName);

        await _ioLock.WaitAsync(cancellationToken);

        try
        {
            File.Delete(path);
            File.Delete(path + ".tmp");
        }
        finally
        {
            _ioLock.Release();
        }
    }

    private string PathFor(string typeName)
    {
        if (typeName.AsSpan().IndexOfAny(_invalidTypeNameChars) >= 0)
        {
            throw new InvalidOperationException(
                $"Persisted type name '{typeName}' cannot be used as a snapshot file name."
            );
        }

        return Path.Combine(_directory, typeName + _suffix);
    }
}

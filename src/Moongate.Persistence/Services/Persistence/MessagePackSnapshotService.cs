using MessagePack;
using MessagePack.Resolvers;
using Moongate.Persistence.Data;
using Moongate.Persistence.Interfaces.Persistence;

namespace Moongate.Persistence.Services.Persistence;

/// <summary>
/// Stores the world snapshot as a single MessagePack file, written atomically via temp + rename.
/// </summary>
public sealed class MessagePackSnapshotService : ISnapshotService, IDisposable
{
    private static readonly MessagePackSerializerOptions _options = ContractlessStandardResolver.Options;

    private readonly SemaphoreSlim _ioLock = new(1, 1);
    private readonly string _path;

    public MessagePackSnapshotService(string snapshotFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(snapshotFilePath);

        _path = Path.GetFullPath(snapshotFilePath);

        var directory = Path.GetDirectoryName(_path);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public async ValueTask<WorldSnapshot?> LoadAsync(CancellationToken cancellationToken = default)
    {
        await _ioLock.WaitAsync(cancellationToken);

        try
        {
            if (!File.Exists(_path))
            {
                return null;
            }

            var bytes = await File.ReadAllBytesAsync(_path, cancellationToken);

            return MessagePackSerializer.Deserialize<WorldSnapshot>(bytes, _options, cancellationToken);
        }
        finally
        {
            _ioLock.Release();
        }
    }

    public async ValueTask SaveAsync(WorldSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        await _ioLock.WaitAsync(cancellationToken);

        try
        {
            var tempPath = _path + ".tmp";
            var bytes = MessagePackSerializer.Serialize(snapshot, _options, cancellationToken);

            await using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.WriteAsync(bytes, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }

            File.Move(tempPath, _path, true);
        }
        finally
        {
            _ioLock.Release();
        }
    }

    public void Dispose()
        => _ioLock.Dispose();
}

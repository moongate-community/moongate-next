using System.Buffers.Binary;
using MessagePack;
using MessagePack.Resolvers;
using Moongate.Persistence.Data;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.Persistence.Internal;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Moongate.Persistence.Services.Persistence;

/// <summary>
/// Append-only journal stored as length+checksum framed MessagePack records. A corrupt trailing
/// record (truncated write) is detected on read and the tail is discarded.
/// </summary>
public sealed class BinaryJournalService : IJournalService, IAsyncDisposable
{
    private const int HeaderSize = 8; // int length + uint checksum

    private static readonly MessagePackSerializerOptions _options = ContractlessStandardResolver.Options;

    private readonly ILogger _logger = Log.ForContext<BinaryJournalService>();
    private readonly SemaphoreSlim _ioLock = new(1, 1);
    private readonly string _path;

    public BinaryJournalService(string journalFilePath, bool enableFileLock = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(journalFilePath);

        _path = Path.GetFullPath(journalFilePath);
        _ = enableFileLock;

        var directory = Path.GetDirectoryName(_path);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public async ValueTask AppendAsync(JournalEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        await _ioLock.WaitAsync(cancellationToken);

        try
        {
            await using var stream = new FileStream(_path, FileMode.Append, FileAccess.Write, FileShare.Read);
            await WriteRecordAsync(stream, entry, cancellationToken);
        }
        finally
        {
            _ioLock.Release();
        }
    }

    public async ValueTask AppendBatchAsync(
        IReadOnlyList<JournalEntry> entries,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entries);

        await _ioLock.WaitAsync(cancellationToken);

        try
        {
            await using var stream = new FileStream(_path, FileMode.Append, FileAccess.Write, FileShare.Read);

            for (var i = 0; i < entries.Count; i++)
            {
                await WriteRecordAsync(stream, entries[i], cancellationToken);
            }
        }
        finally
        {
            _ioLock.Release();
        }
    }

    public ValueTask DisposeAsync()
    {
        _ioLock.Dispose();

        return ValueTask.CompletedTask;
    }

    public async ValueTask<IReadOnlyCollection<JournalEntry>> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        await _ioLock.WaitAsync(cancellationToken);

        try
        {
            if (!File.Exists(_path))
            {
                return [];
            }

            var bytes = await File.ReadAllBytesAsync(_path, cancellationToken);

            return ParseAll(bytes);
        }
        finally
        {
            _ioLock.Release();
        }
    }

    public async ValueTask ResetAsync(CancellationToken cancellationToken = default)
    {
        await _ioLock.WaitAsync(cancellationToken);

        try
        {
            await RewriteAsync([], cancellationToken);
        }
        finally
        {
            _ioLock.Release();
        }
    }

    public async ValueTask TrimThroughSequenceAsync(
        long inclusiveSequenceId,
        CancellationToken cancellationToken = default
    )
    {
        await _ioLock.WaitAsync(cancellationToken);

        try
        {
            if (!File.Exists(_path))
            {
                return;
            }

            var kept = ParseAll(await File.ReadAllBytesAsync(_path, cancellationToken))
                       .Where(e => e.SequenceId > inclusiveSequenceId)
                       .ToArray();

            await RewriteAsync(kept, cancellationToken);
        }
        finally
        {
            _ioLock.Release();
        }
    }

    private List<JournalEntry> ParseAll(byte[] bytes)
    {
        var entries = new List<JournalEntry>();
        var offset = 0;

        while (offset + HeaderSize <= bytes.Length)
        {
            var length = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(offset));
            var checksum = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset + 4));

            if (length <= 0 || offset + HeaderSize + length > bytes.Length)
            {
                _logger.Warning("Journal {Path}: truncated record at offset {Offset}; discarding tail", _path, offset);

                break;
            }

            var payload = bytes.AsSpan(offset + HeaderSize, length);

            if (ChecksumUtils.Compute(payload) != checksum)
            {
                _logger.Warning("Journal {Path}: checksum mismatch at offset {Offset}; discarding tail", _path, offset);

                break;
            }

            entries.Add(MessagePackSerializer.Deserialize<JournalEntry>(payload.ToArray(), _options));
            offset += HeaderSize + length;
        }

        return entries;
    }

    private async ValueTask RewriteAsync(IReadOnlyList<JournalEntry> entries, CancellationToken cancellationToken)
    {
        var tempPath = _path + ".tmp";

        await using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            for (var i = 0; i < entries.Count; i++)
            {
                await WriteRecordAsync(stream, entries[i], cancellationToken);
            }

            await stream.FlushAsync(cancellationToken);
        }

        File.Move(tempPath, _path, true);
    }

    private static async ValueTask WriteRecordAsync(
        FileStream stream,
        JournalEntry entry,
        CancellationToken cancellationToken
    )
    {
        var payload = MessagePackSerializer.Serialize(entry, _options, cancellationToken);
        var header = new byte[HeaderSize];
        BinaryPrimitives.WriteInt32LittleEndian(header, payload.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(4), ChecksumUtils.Compute(payload));

        await stream.WriteAsync(header, cancellationToken);
        await stream.WriteAsync(payload, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }
}

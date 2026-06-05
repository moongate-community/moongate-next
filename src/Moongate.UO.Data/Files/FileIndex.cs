using System.Runtime.InteropServices;
using Moongate.UO.Data.Interfaces.Files;

namespace Moongate.UO.Data.Files;

/// <summary>
/// Reads UO indexed resources from an <c>.idx</c>/<c>.mul</c> pair or a legacy <c>.uop</c> file,
/// applying any verdata patches supplied by an <see cref="IVerdataPatchSource" />.
/// </summary>
public sealed class FileIndex : IDisposable
{
    private readonly IVerdataPatchSource _verdata;
    private readonly string? _mulPath;

    public Entry3D[] Index { get; }

    public Stream? Stream { get; private set; }

    public long IdxLength { get; }

    public FileIndex(string? idxPath, string? mulPath, int length, int file, IVerdataPatchSource verdata)
        : this(idxPath, mulPath, null, length, file, ".dat", -1, false, verdata)
    {
    }

    public FileIndex(
        string? idxPath,
        string? mulPath,
        string? uopPath,
        int length,
        int file,
        string uopEntryExtension,
        int idxLength,
        bool hasExtra,
        IVerdataPatchSource verdata
    )
    {
        ArgumentNullException.ThrowIfNull(verdata);

        _verdata = verdata;
        Index = new Entry3D[length];
        _mulPath = mulPath;

        var resolvedUop = !string.IsNullOrEmpty(uopPath) && File.Exists(uopPath) ? uopPath : null;

        if (resolvedUop != null)
        {
            _mulPath = resolvedUop;
        }

        if (_mulPath != null && _mulPath.EndsWith(".uop", StringComparison.OrdinalIgnoreCase))
        {
            Stream = new FileStream(_mulPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

            var fi = new FileInfo(_mulPath);
            var uopPattern = fi.Name.Replace(fi.Extension, "").ToLowerInvariant();

            using var br = new BinaryReader(Stream, System.Text.Encoding.UTF8, leaveOpen: true);
            br.BaseStream.Seek(0, SeekOrigin.Begin);

            if (br.ReadInt32() != 0x50594D)
            {
                throw new ArgumentException("Bad UOP file.");
            }

            br.ReadInt64(); // version + signature
            var nextBlock = br.ReadInt64();
            br.ReadInt32(); // block capacity
            br.ReadInt32(); // count

            if (idxLength > 0)
            {
                IdxLength = idxLength * 12;
            }

            var hashes = new Dictionary<ulong, int>();

            for (var i = 0; i < length; i++)
            {
                var entryName = string.Format("build/{0}/{1:D8}{2}", uopPattern, i, uopEntryExtension);
                var hash = HashFileName(entryName);

                hashes.TryAdd(hash, i);
            }

            br.BaseStream.Seek(nextBlock, SeekOrigin.Begin);

            do
            {
                var filesCount = br.ReadInt32();
                nextBlock = br.ReadInt64();

                for (var i = 0; i < filesCount; i++)
                {
                    var offset = br.ReadInt64();
                    var headerLength = br.ReadInt32();
                    var compressedLength = br.ReadInt32();
                    var decompressedLength = br.ReadInt32();
                    var hash = br.ReadUInt64();
                    br.ReadUInt32(); // Adler32
                    var flag = br.ReadInt16();

                    var entryLength = flag == 1 ? compressedLength : decompressedLength;

                    if (offset == 0)
                    {
                        continue;
                    }

                    if (hashes.TryGetValue(hash, out var idx))
                    {
                        if (idx < 0 || idx > Index.Length)
                        {
                            throw new IndexOutOfRangeException(
                                "hashes dictionary and files collection have different count of entries!"
                            );
                        }

                        Index[idx].lookup = (int)(offset + headerLength);
                        Index[idx].length = entryLength;

                        if (hasExtra)
                        {
                            var curPos = br.BaseStream.Position;

                            br.BaseStream.Seek(offset + headerLength, SeekOrigin.Begin);

                            var extra = br.ReadBytes(8);

                            var extra1 = (ushort)((extra[3] << 24) | (extra[2] << 16) | (extra[1] << 8) | extra[0]);
                            var extra2 = (ushort)((extra[7] << 24) | (extra[6] << 16) | (extra[5] << 8) | extra[4]);

                            Index[idx].lookup += 8;
                            Index[idx].extra = (extra1 << 16) | extra2;

                            br.BaseStream.Seek(curPos, SeekOrigin.Begin);
                        }
                    }
                }
            } while (br.BaseStream.Seek(nextBlock, SeekOrigin.Begin) != 0);
        }
        else if (idxPath != null && File.Exists(idxPath) && _mulPath != null && File.Exists(_mulPath))
        {
            using (var index = new FileStream(idxPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                Stream = new FileStream(_mulPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                var count = (int)(index.Length / 12);
                IdxLength = index.Length;
                var gc = GCHandle.Alloc(Index, GCHandleType.Pinned);
                var buffer = new byte[index.Length];
                index.ReadExactly(buffer, 0, (int)index.Length);
                Marshal.Copy(buffer, 0, gc.AddrOfPinnedObject(), (int)Math.Min(IdxLength, length * 12));
                gc.Free();

                for (var i = count; i < length; ++i)
                {
                    Index[i].lookup = -1;
                    Index[i].length = -1;
                    Index[i].extra = -1;
                }
            }
        }
        else
        {
            Stream = null;

            return;
        }

        ApplyPatches(file, length);
    }

    private void ApplyPatches(int file, int length)
    {
        var patches = _verdata.Patches;

        if (file <= -1)
        {
            return;
        }

        for (var i = 0; i < patches.Count; ++i)
        {
            var patch = patches[i];

            if (patch.file == file && patch.index >= 0 && patch.index < length)
            {
                Index[patch.index].lookup = patch.lookup;
                Index[patch.index].length = patch.length | (1 << 31);
                Index[patch.index].extra = patch.extra;
            }
        }
    }

    /// <summary>
    /// Calculates a Mythic.Package entry hash from its build path. Taken from Mythic.Package.dll.
    /// </summary>
    /// <param name="s">The entry path, e.g. <c>build/artlegacymul/000000000.tga</c>.</param>
    public static ulong HashFileName(string s)
    {
        uint eax,
             ecx,
             edx,
             ebx,
             esi,
             edi;

        eax = ecx = edx = 0;
        ebx = edi = esi = (uint)s.Length + 0xDEADBEEF;

        var i = 0;

        for (i = 0; i + 12 < s.Length; i += 12)
        {
            edi = (uint)((s[i + 7] << 24) | (s[i + 6] << 16) | (s[i + 5] << 8) | s[i + 4]) + edi;
            esi = (uint)((s[i + 11] << 24) | (s[i + 10] << 16) | (s[i + 9] << 8) | s[i + 8]) + esi;
            edx = (uint)((s[i + 3] << 24) | (s[i + 2] << 16) | (s[i + 1] << 8) | s[i]) - esi;

            edx = (edx + ebx) ^ (esi >> 28) ^ (esi << 4);
            esi += edi;
            edi = (edi - edx) ^ (edx >> 26) ^ (edx << 6);
            edx += esi;
            esi = (esi - edi) ^ (edi >> 24) ^ (edi << 8);
            edi += edx;
            ebx = (edx - esi) ^ (esi >> 16) ^ (esi << 16);
            esi += edi;
            edi = (edi - ebx) ^ (ebx >> 13) ^ (ebx << 19);
            ebx += esi;
            esi = (esi - edi) ^ (edi >> 28) ^ (edi << 4);
            edi += ebx;
        }

        if (s.Length - i > 0)
        {
            switch (s.Length - i)
            {
                case 12:
                    esi += (uint)s[i + 11] << 24;
                    goto case 11;
                case 11:
                    esi += (uint)s[i + 10] << 16;
                    goto case 10;
                case 10:
                    esi += (uint)s[i + 9] << 8;
                    goto case 9;
                case 9:
                    esi += s[i + 8];
                    goto case 8;
                case 8:
                    edi += (uint)s[i + 7] << 24;
                    goto case 7;
                case 7:
                    edi += (uint)s[i + 6] << 16;
                    goto case 6;
                case 6:
                    edi += (uint)s[i + 5] << 8;
                    goto case 5;
                case 5:
                    edi += s[i + 4];
                    goto case 4;
                case 4:
                    ebx += (uint)s[i + 3] << 24;
                    goto case 3;
                case 3:
                    ebx += (uint)s[i + 2] << 16;
                    goto case 2;
                case 2:
                    ebx += (uint)s[i + 1] << 8;
                    goto case 1;
                case 1:
                    ebx += s[i];

                    break;
            }

            esi = (esi ^ edi) - ((edi >> 18) ^ (edi << 14));
            ecx = (esi ^ ebx) - ((esi >> 21) ^ (esi << 11));
            edi = (edi ^ ecx) - ((ecx >> 7) ^ (ecx << 25));
            esi = (esi ^ edi) - ((edi >> 16) ^ (edi << 16));
            edx = (esi ^ ecx) - ((esi >> 28) ^ (esi << 4));
            edi = (edi ^ edx) - ((edx >> 18) ^ (edx << 14));
            eax = (esi ^ edi) - ((edi >> 8) ^ (edi << 24));

            return ((ulong)edi << 32) | eax;
        }

        return ((ulong)esi << 32) | eax;
    }

    public Stream? Seek(int index, out int length, out int extra, out bool patched)
    {
        if (index < 0 || index >= Index.Length)
        {
            length = extra = 0;
            patched = false;

            return null;
        }

        var e = Index[index];

        if (e.lookup < 0)
        {
            length = extra = 0;
            patched = false;

            return null;
        }

        length = e.length & 0x7FFFFFFF;
        extra = e.extra;

        if ((e.length & (1 << 31)) != 0)
        {
            patched = true;

            return _verdata.Seek(e.lookup);
        }

        if (e.length < 0)
        {
            length = extra = 0;
            patched = false;

            return null;
        }

        if (Stream == null || !Stream.CanRead || !Stream.CanSeek)
        {
            Stream = _mulPath == null
                ? null
                : new FileStream(_mulPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        if (Stream == null)
        {
            length = extra = 0;
            patched = false;

            return null;
        }

        if (Stream.Length < e.lookup)
        {
            length = extra = 0;
            patched = false;

            return null;
        }

        patched = false;

        Stream.Seek(e.lookup, SeekOrigin.Begin);

        return Stream;
    }

    public bool Valid(int index, out int length, out int extra, out bool patched)
    {
        if (index < 0 || index >= Index.Length)
        {
            length = extra = 0;
            patched = false;

            return false;
        }

        var e = Index[index];

        if (e.lookup < 0)
        {
            length = extra = 0;
            patched = false;

            return false;
        }

        length = e.length & 0x7FFFFFFF;
        extra = e.extra;

        if ((e.length & (1 << 31)) != 0)
        {
            patched = true;

            return true;
        }

        if (e.length < 0)
        {
            length = extra = 0;
            patched = false;

            return false;
        }

        if (_mulPath == null || !File.Exists(_mulPath))
        {
            length = extra = 0;
            patched = false;

            return false;
        }

        if (Stream == null || !Stream.CanRead || !Stream.CanSeek)
        {
            Stream = new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        if (Stream.Length < e.lookup)
        {
            length = extra = 0;
            patched = false;

            return false;
        }

        patched = false;

        return true;
    }

    public void Dispose()
    {
        Stream?.Dispose();
        Stream = null;
    }
}

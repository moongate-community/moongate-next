using System.Text;
using Moongate.UO.Data.Data.Localization;
using Moongate.UO.Data.Internal.Compression;

namespace Moongate.UO.Data.Internal.Localization;

/// <summary>
/// Reads and parses a <c>cliloc</c> file, transparently decompressing Mythic-packed data.
/// </summary>
public static class ClilocReader
{
    private const uint MythicHeaderXor = 0x8E2C9A3D;
    private const int MaxExpectedMythicOutputLength = 64 * 1024 * 1024;
    private const int MaxEntryLength = 64 * 1024;

    public static List<StringEntry> Read(string filePath)
    {
        var entries = new List<StringEntry>();

        var buffer = File.ReadAllBytes(filePath);
        var clilocData = buffer;

        if (IsLikelyMythicCompressed(buffer))
        {
            try
            {
                var decompressed = MythicDecompress.Decompress(buffer);

                if (decompressed.Length > 0)
                {
                    clilocData = decompressed;
                }
            }
            catch
            {
                // Some distributions ship cliloc as plain data; fall back to raw parsing.
                clilocData = buffer;
            }
        }

        using var reader = new BinaryReader(new MemoryStream(clilocData));

        reader.ReadInt32();
        reader.ReadInt16();

        while (reader.BaseStream.Length != reader.BaseStream.Position)
        {
            var remaining = reader.BaseStream.Length - reader.BaseStream.Position;

            if (remaining < 7)
            {
                break;
            }

            var number = reader.ReadInt32();
            var flag = reader.ReadByte();
            var length = (int)reader.ReadUInt16();

            // A zero-length entry is a valid empty string; only a negative or oversized length is corrupt.
            if (length < 0 || length > MaxEntryLength)
            {
                break;
            }

            if (reader.BaseStream.Length - reader.BaseStream.Position < length)
            {
                break;
            }

            var textBuffer = reader.ReadBytes(length);

            if (textBuffer.Length != length)
            {
                break;
            }

            var text = Encoding.UTF8.GetString(textBuffer, 0, length);
            entries.Add(new StringEntry(number, text, flag));
        }

        return entries;
    }

    private static bool IsLikelyMythicCompressed(byte[] buffer)
    {
        if (buffer.Length <= 1028)
        {
            return false;
        }

        var header = BitConverter.ToUInt32(buffer, 0);
        var expectedOutputLength = (int)(header ^ MythicHeaderXor);

        return expectedOutputLength > 0 && expectedOutputLength <= MaxExpectedMythicOutputLength;
    }
}

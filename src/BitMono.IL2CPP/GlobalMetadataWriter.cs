using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BitMono.IL2CPP;

/// <summary>
/// Writes modified <c>global-metadata.dat</c> bytes. For now it only does same-length, in-place edits of the
/// identifier <c>string</c> region - the one rewrite that needs no offset fix-up, since every <c>nameIndex</c>
/// keeps pointing at the same place (Tulach's approach). Arbitrary-length renames need a full rebuild that
/// re-points every referent, which waits on the per-version table parser. See #276.
/// </summary>
/// <remarks>
/// This is only the offline half: a renamed metadata file loads correctly at runtime <i>only</i> if a matching
/// decryptor/forwarder is present in <c>GameAssembly.dll</c> to map name lookups back. That native piece is
/// still open #276 work - this primitive on its own will break a build.
/// </remarks>
public static class GlobalMetadataWriter
{
    /// <summary>
    /// Returns a copy of <paramref name="metadata"/> with identifier names overwritten in place. Keys are
    /// <c>nameIndex</c> values (byte offsets relative to the start of the string region, as IL2CPP uses them);
    /// each replacement must encode to exactly the same number of UTF-8 bytes as the name currently at that
    /// index, so nothing moves and the file stays valid.
    /// </summary>
    public static byte[] ReplaceNames(byte[] metadata, IReadOnlyDictionary<int, string> replacementsByIndex)
    {
        if (metadata == null)
        {
            throw new ArgumentNullException(nameof(metadata));
        }
        if (replacementsByIndex == null)
        {
            throw new ArgumentNullException(nameof(replacementsByIndex));
        }

        GlobalMetadataFile.ValidateHeader(metadata);
        var stringOffset = GlobalMetadataFile.ReadInt32(metadata, 24);
        var stringSize = GlobalMetadataFile.ReadInt32(metadata, 28);
        GlobalMetadataFile.CheckRegion(metadata, stringOffset, stringSize, "string");

        var result = (byte[])metadata.Clone();
        if (replacementsByIndex.Count == 0)
        {
            return result;
        }

        var regionEnd = stringOffset + stringSize;
        foreach (var pair in replacementsByIndex)
        {
            var index = pair.Key;
            // Bound on index (not stringOffset + index, which can overflow for a huge index).
            if (index < 0 || index >= stringSize)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(replacementsByIndex), $"Name index {index} is outside the string region.");
            }
            var start = stringOffset + index;

            var nul = Array.IndexOf(result, (byte)0, start, regionEnd - start);
            if (nul < 0)
            {
                throw new InvalidDataException($"Name at index {index} is not NUL-terminated.");
            }

            var existingLength = nul - start;
            var replacement = Encoding.UTF8.GetBytes(pair.Value ?? string.Empty);
            if (replacement.Length != existingLength)
            {
                throw new ArgumentException(
                    $"Replacement for name index {index} must be {existingLength} UTF-8 byte(s), got " +
                    $"{replacement.Length}. In-place rename can't change a name's length.");
            }
            Buffer.BlockCopy(replacement, 0, result, start, replacement.Length);
        }
        return result;
    }
}

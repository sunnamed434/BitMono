using System;
using System.IO;

namespace BitMono.IL2CPP;

/// <summary>
/// Offline half of the #276 metadata-encryption protection: turns a plain <c>global-metadata.dat</c> into an
/// encrypted blob that a native decryptor (compiled into <c>GameAssembly.dll</c>) restores in memory at
/// startup, so static dumpers can't parse it. The format is a small plaintext front header followed by the
/// XXTEA-encrypted metadata. <see cref="Decrypt"/> is the reference the native decryptor mirrors. The key
/// ships in the binary, so this is obfuscation strength, not secrecy.
/// </summary>
public static class GlobalMetadataEncryptor
{
    // Marker so the decryptor knows it's looking at a BitMono-encrypted file: ASCII "BMIL2CPP", little-endian.
    public const ulong Sanity = 0x50504332_4C494D42;

    // Front header: sanity(8) + bodyOffset(4) + bodyLength(4) + crc32(4). Plaintext - none of it is secret.
    private const int FrontHeaderSize = 20;

    public static byte[] Encrypt(byte[] metadata, byte[] key)
    {
        if (metadata == null)
        {
            throw new ArgumentNullException(nameof(metadata));
        }
        // Refuse anything that isn't a real global-metadata.dat (also stops accidental double-encryption).
        GlobalMetadataFile.ValidateHeader(metadata);

        var crc = Crc32.Compute(metadata);
        var body = Xxtea.Encrypt(metadata, key); // key length is validated inside Xxtea

        using var output = new MemoryStream(FrontHeaderSize + body.Length);
        var writer = new BinaryWriter(output);
        writer.Write(Sanity);          // u64 sanity
        writer.Write(FrontHeaderSize); // u32 bodyOffset
        writer.Write(metadata.Length); // u32 bodyLength (decrypted length)
        writer.Write(crc);             // u32 crc32 of the decrypted metadata
        writer.Write(body);
        return output.ToArray();
    }

    public static byte[] Decrypt(byte[] encrypted, byte[] key)
    {
        if (encrypted == null)
        {
            throw new ArgumentNullException(nameof(encrypted));
        }
        if (encrypted.Length < FrontHeaderSize)
        {
            throw new InvalidDataException("Encrypted metadata is too small.");
        }

        using var reader = new BinaryReader(new MemoryStream(encrypted));
        if (reader.ReadUInt64() != Sanity)
        {
            throw new InvalidDataException("Not BitMono-encrypted metadata (bad sanity).");
        }
        var bodyOffset = reader.ReadInt32();
        var bodyLength = reader.ReadInt32();
        var crc = reader.ReadUInt32();
        if (bodyOffset < FrontHeaderSize || bodyOffset > encrypted.Length || bodyLength < 0)
        {
            throw new InvalidDataException("Encrypted metadata header is out of range.");
        }

        var body = new byte[encrypted.Length - bodyOffset];
        Buffer.BlockCopy(encrypted, bodyOffset, body, 0, body.Length);
        var decrypted = Xxtea.Decrypt(body, key);
        if (bodyLength > decrypted.Length)
        {
            throw new InvalidDataException("Encrypted metadata body length is invalid.");
        }
        if (bodyLength != decrypted.Length)
        {
            Array.Resize(ref decrypted, bodyLength); // drop XXTEA's word padding
        }
        if (Crc32.Compute(decrypted) != crc)
        {
            throw new InvalidDataException("Metadata failed its integrity check (wrong key or corrupt file).");
        }
        return decrypted;
    }
}

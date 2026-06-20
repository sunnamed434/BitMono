using System;

namespace BitMono.IL2CPP;

// XXTEA (Corrected Block TEA) - the cipher Ether uses for global-metadata.dat. Works on the data as
// little-endian uint32 words with a 128-bit key. We encrypt the whole metadata file so static dumpers
// (Il2CppDumper/Cpp2IL) can't parse it; the key ships in the decryptor, so this is obfuscation strength,
// not secrecy. The native runtime decryptor must mirror this exactly. See il2cpp-output-protection.md (#276).
internal static class Xxtea
{
    private const uint Delta = 0x9E3779B9;

    // Encrypts a copy and returns it. Input is zero-padded up to a whole number of 32-bit words; the caller
    // records the original length separately (XXTEA can't shrink, and always rounds up to a word boundary).
    public static byte[] Encrypt(byte[] data, byte[] key)
    {
        var v = ToWords(data);
        var k = KeyWords(key);
        var n = v.Length;
        if (n >= 2)
        {
            var rounds = 6 + 52 / n;
            unchecked
            {
                uint sum = 0;
                var z = v[n - 1];
                while (rounds-- > 0)
                {
                    sum += Delta;
                    var e = (int)((sum >> 2) & 3);
                    uint y;
                    for (var p = 0; p < n - 1; p++)
                    {
                        y = v[p + 1];
                        z = v[p] += Mx(sum, y, z, p, e, k);
                    }
                    y = v[0];
                    z = v[n - 1] += Mx(sum, y, z, n - 1, e, k);
                }
            }
        }
        return ToBytes(v);
    }

    public static byte[] Decrypt(byte[] data, byte[] key)
    {
        var v = ToWords(data);
        var k = KeyWords(key);
        var n = v.Length;
        if (n >= 2)
        {
            var rounds = 6 + 52 / n;
            unchecked
            {
                var sum = (uint)rounds * Delta;
                var y = v[0];
                while (rounds-- > 0)
                {
                    var e = (int)((sum >> 2) & 3);
                    uint z;
                    for (var p = n - 1; p > 0; p--)
                    {
                        z = v[p - 1];
                        y = v[p] -= Mx(sum, y, z, p, e, k);
                    }
                    z = v[n - 1];
                    y = v[0] -= Mx(sum, y, z, 0, e, k);
                    sum -= Delta;
                }
            }
        }
        return ToBytes(v);
    }

    private static uint Mx(uint sum, uint y, uint z, int p, int e, uint[] key) =>
        unchecked((((z >> 5) ^ (y << 2)) + ((y >> 3) ^ (z << 4))) ^ ((sum ^ y) + (key[(p & 3) ^ e] ^ z)));

    private static uint[] ToWords(byte[] data)
    {
        var v = new uint[(data.Length + 3) / 4];
        for (var i = 0; i < data.Length; i++)
        {
            v[i / 4] |= (uint)data[i] << (8 * (i % 4));
        }
        return v;
    }

    private static byte[] ToBytes(uint[] v)
    {
        var result = new byte[v.Length * 4];
        for (var i = 0; i < v.Length; i++)
        {
            result[i * 4] = (byte)v[i];
            result[i * 4 + 1] = (byte)(v[i] >> 8);
            result[i * 4 + 2] = (byte)(v[i] >> 16);
            result[i * 4 + 3] = (byte)(v[i] >> 24);
        }
        return result;
    }

    private static uint[] KeyWords(byte[] key)
    {
        if (key == null || key.Length != 16)
        {
            throw new ArgumentException("XXTEA key must be 16 bytes (128-bit).", nameof(key));
        }
        return ToWords(key);
    }
}

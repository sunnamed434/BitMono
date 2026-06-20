namespace BitMono.IL2CPP;

// Standard IEEE CRC-32 (reflected, poly 0xEDB88320) - the FrontHeader integrity check, so the runtime
// decryptor can confirm it decrypted with the right key. Must stay bit-identical to the native stub's CRC.
internal static class Crc32
{
    private static readonly uint[] Table = BuildTable();

    public static uint Compute(byte[] data)
    {
        var crc = 0xFFFFFFFFu;
        foreach (var b in data)
        {
            crc = (crc >> 8) ^ Table[(crc ^ b) & 0xFF];
        }
        return crc ^ 0xFFFFFFFFu;
    }

    private static uint[] BuildTable()
    {
        var table = new uint[256];
        for (var i = 0u; i < 256; i++)
        {
            var c = i;
            for (var k = 0; k < 8; k++)
            {
                c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
            }
            table[i] = c;
        }
        return table;
    }
}

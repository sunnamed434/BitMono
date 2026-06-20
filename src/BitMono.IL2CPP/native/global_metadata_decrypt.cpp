// Native IL2CPP global-metadata.dat decryptor for #276.
//
// BitMonoDecryptMetadata is the function injected into libil2cpp's GlobalMetadata::Initialize, right after
//   s_GlobalMetadata = vm::MetadataLoader::LoadMetadataFile("global-metadata.dat");
// as:
//   s_GlobalMetadata = BitMonoDecryptMetadata(s_GlobalMetadata);
//
// It mirrors C# GlobalMetadataEncryptor.Decrypt byte-for-byte: parse the FrontHeader, XXTEA-decrypt the body
// with the build key, verify the CRC32. If the file isn't BitMono-encrypted (sanity mismatch) it's returned
// untouched, so a plain build still works. Compile with -DBITMONO_STANDALONE_TEST for the offline validator.

#include <cstdint>
#include <cstdlib>
#include <cstring>

namespace {

#pragma pack(push, 1)
struct BitMonoFrontHeader
{
    uint64_t sanity;     // 0x505043324C494D42 == "BMIL2CPP" little-endian
    uint32_t bodyOffset; // start of the encrypted body (== sizeof(FrontHeader) == 20)
    uint32_t bodyLength; // decrypted length
    uint32_t crc32;      // CRC32 of the decrypted metadata
};
#pragma pack(pop)

const uint64_t kBitMonoSanity = 0x505043324C494D42ULL;

// XXTEA decrypt over little-endian uint32 words (matches C# Xxtea.Decrypt).
void XxteaDecrypt(uint32_t* v, int n, const uint32_t key[4])
{
    if (n < 2)
        return;
    const uint32_t delta = 0x9E3779B9;
    unsigned rounds = 6 + 52 / n;
    uint32_t sum = (uint32_t)(rounds * delta);
    uint32_t y = v[0];
    do
    {
        unsigned e = (sum >> 2) & 3;
        uint32_t z;
        for (int p = n - 1; p > 0; p--)
        {
            z = v[p - 1];
            y = v[p] -= (((z >> 5) ^ (y << 2)) + ((y >> 3) ^ (z << 4))) ^ ((sum ^ y) + (key[(p & 3) ^ e] ^ z));
        }
        z = v[n - 1];
        y = v[0] -= (((z >> 5) ^ (y << 2)) + ((y >> 3) ^ (z << 4))) ^ ((sum ^ y) + (key[e] ^ z));
        sum -= delta;
    } while (--rounds);
}

// IEEE CRC-32 (matches C# Crc32.Compute).
uint32_t Crc32(const uint8_t* data, uint32_t length)
{
    uint32_t crc = 0xFFFFFFFF;
    for (uint32_t i = 0; i < length; i++)
    {
        crc ^= data[i];
        for (int k = 0; k < 8; k++)
            crc = (crc & 1) ? (0xEDB88320 ^ (crc >> 1)) : (crc >> 1);
    }
    return crc ^ 0xFFFFFFFF;
}

} // namespace

// Returns a decrypted copy when the input is BitMono-encrypted; otherwise returns the input unchanged.
extern "C" const void* BitMonoDecryptMetadata(const void* data)
{
    const BitMonoFrontHeader* header = (const BitMonoFrontHeader*)data;
    if (header->sanity != kBitMonoSanity)
        return data; // plain metadata - leave it alone

    uint32_t key[4];
    memcpy(key, "BitMono-IL2CPP!!", 16); // must match the CLI's --encrypt-metadata key

    uint32_t bodyLength = header->bodyLength;
    uint32_t paddedLength = (bodyLength + 3) & ~3u;
    uint8_t* buffer = (uint8_t*)malloc(paddedLength);
    if (!buffer)
        return data;
    memcpy(buffer, (const uint8_t*)data + header->bodyOffset, paddedLength);
    XxteaDecrypt((uint32_t*)buffer, (int)(paddedLength / 4), key);

    if (Crc32(buffer, bodyLength) != header->crc32)
    {
        free(buffer); // wrong key / tampered - let IL2CPP fail its own sanity check
        return data;
    }
    return buffer;
}

#ifdef BITMONO_STANDALONE_TEST
#include <cstdio>

static uint8_t* ReadAll(const char* path, long* size)
{
    FILE* f = fopen(path, "rb");
    if (!f) return nullptr;
    fseek(f, 0, SEEK_END); *size = ftell(f); fseek(f, 0, SEEK_SET);
    uint8_t* buf = (uint8_t*)malloc(*size);
    if (fread(buf, 1, (size_t)*size, f) != (size_t)*size) { fclose(f); free(buf); return nullptr; }
    fclose(f);
    return buf;
}

int main(int argc, char** argv)
{
    if (argc < 3) { printf("usage: %s <encrypted.enc> <original.dat>\n", argv[0]); return 2; }
    long encSize = 0, origSize = 0;
    uint8_t* enc = ReadAll(argv[1], &encSize);
    uint8_t* orig = ReadAll(argv[2], &origSize);
    if (!enc || !orig) { printf("read failed\n"); return 2; }

    const BitMonoFrontHeader* h = (const BitMonoFrontHeader*)enc;
    printf("sanity      : 0x%016llX (want 0x%016llX) -> %s\n",
        (unsigned long long)h->sanity, (unsigned long long)kBitMonoSanity, h->sanity == kBitMonoSanity ? "OK" : "BAD");

    const uint8_t* dec = (const uint8_t*)BitMonoDecryptMetadata(enc);
    bool decrypted = dec != enc;
    printf("decrypted   : %s\n", decrypted ? "yes (sanity + CRC passed)" : "no (sanity/CRC failed)");
    if (!decrypted) return 1;

    uint32_t magic = *(const uint32_t*)dec;
    int32_t version = *(const int32_t*)(dec + 4);
    printf("magic       : 0x%08X (want 0xFAB11BAF), version: %d\n", magic, version);

    bool match = (h->bodyLength == (uint32_t)origSize) && memcmp(dec, orig, h->bodyLength) == 0;
    printf("matches original global-metadata.dat byte-for-byte: %s (%u vs %ld bytes)\n",
        match ? "YES" : "NO", h->bodyLength, origSize);
    return match ? 0 : 1;
}
#endif

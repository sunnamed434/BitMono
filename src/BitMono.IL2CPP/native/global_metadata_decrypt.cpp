// Native IL2CPP global-metadata.dat decryptor (BitMono #276).
//
// Ships as a Unity source plugin: Unity compiles this loose .cpp into GameAssembly.dll for IL2CPP, and a
// file-scope initializer IAT-hooks CreateFileW at load - before il2cpp_init - so when IL2CPP opens an
// encrypted global-metadata.dat it transparently gets the decrypted bytes. No editing of the user's Unity
// install, no per-version libil2cpp patching: the Win32 file API is stable across Unity versions.
//
// BitMonoDecryptMetadata mirrors C# GlobalMetadataEncryptor.Decrypt byte-for-byte and returns the input
// untouched when it isn't BitMono-encrypted, so a plain build still works. (It can also be injected straight
// into GlobalMetadata::Initialize: s_GlobalMetadata = BitMonoDecryptMetadata(s_GlobalMetadata).)
//
// Build modes: no define = the shipped plugin; BITMONO_STANDALONE_TEST = offline validator (decrypt an .enc,
// compare to the .dat); BITMONO_HOOK_TEST = exercise the CreateFileW hook path.
//
// The plugin only compiles its machinery when enabled. The shipped build keys that off the per-build key
// header: BitMono's build hook writes bitmono_il2cpp_key.h next to this file only when EncryptIl2CppMetadata
// is on, so a build that doesn't use the feature leaves this file empty and ships no CreateFileW hook at all.

#include <cstdint>
#include <cstdlib>
#include <cstring>

// Enabled (and keyed) by the generated header, or forced on for the self-tests.
#if defined(__has_include)
#  if __has_include("bitmono_il2cpp_key.h")
#    include "bitmono_il2cpp_key.h"
#    define BITMONO_IL2CPP_ENABLED 1
#  endif
#endif
#if defined(BITMONO_STANDALONE_TEST) || defined(BITMONO_HOOK_TEST)
#  define BITMONO_IL2CPP_ENABLED 1
#endif

#if defined(BITMONO_IL2CPP_ENABLED)

// The 16-byte XXTEA key (must match the CLI's --encrypt-metadata key). The dev default is overridden per build
// by the generated header above, so each shipped game gets a different key - obfuscation strength (it still
// rides in GameAssembly.dll), not a secret, but no longer shared across every BitMono game.
#ifndef BITMONO_IL2CPP_KEY_BYTES
#  define BITMONO_IL2CPP_KEY_BYTES 'B','i','t','M','o','n','o','-','I','L','2','C','P','P','!','!'
#endif

// The metadata file IL2CPP loads; the hook intercepts an open of any path ending in this name.
#define BITMONO_METADATA_NAME "global-metadata.dat"

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
// global-metadata.dat's own magic; a successful decrypt restores it at the head of the file.
const uint32_t kIl2cppMetadataMagic = 0xFAB11BAF;

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

// Returns a decrypted copy (malloc'd, bodyLength bytes valid) when the input is BitMono-encrypted; otherwise
// returns the input pointer unchanged. Caller owns the returned buffer iff it differs from the input.
extern "C" const void* BitMonoDecryptMetadata(const void* data)
{
    const BitMonoFrontHeader* header = (const BitMonoFrontHeader*)data;
    if (header->sanity != kBitMonoSanity)
        return data; // plain metadata - leave it alone

    uint32_t key[4];
    static const unsigned char kKey[16] = { BITMONO_IL2CPP_KEY_BYTES };
    memcpy(key, kKey, 16); // must match the CLI's --encrypt-metadata key

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

// ---------------------------------------------------------------------------------------------------------
// CreateFileW hook: the runtime delivery used by the Unity source plugin (and exercised by BITMONO_HOOK_TEST).
// ---------------------------------------------------------------------------------------------------------
#if defined(_WIN32) && !defined(BITMONO_STANDALONE_TEST)
#define WIN32_LEAN_AND_MEAN
#include <windows.h>

namespace {

typedef HANDLE(WINAPI* CreateFileW_t)(LPCWSTR, DWORD, DWORD, LPSECURITY_ATTRIBUTES, DWORD, DWORD, HANDLE);
CreateFileW_t g_origCreateFileW = nullptr;

bool EndsWithMetadataName(LPCWSTR path)
{
    if (!path)
        return false;
    static const wchar_t kName[] = L"" BITMONO_METADATA_NAME;
    const size_t nameLen = (sizeof(kName) / sizeof(wchar_t)) - 1;
    size_t len = 0;
    while (path[len])
        len++;
    if (len < nameLen)
        return false;
    LPCWSTR tail = path + (len - nameLen);
    for (size_t i = 0; i < nameLen; i++)
    {
        wchar_t a = tail[i], b = kName[i];
        if (a >= L'A' && a <= L'Z') a = (wchar_t)(a - L'A' + L'a');
        if (b >= L'A' && b <= L'Z') b = (wchar_t)(b - L'A' + L'a');
        if (a != b)
            return false;
    }
    return true;
}

// Read the whole file behind an already-open handle. Returns malloc'd bytes + size, or null.
uint8_t* ReadAllFromHandle(HANDLE h, uint32_t* outSize)
{
    LARGE_INTEGER size;
    if (!GetFileSizeEx(h, &size) || size.QuadPart <= 0 || size.QuadPart > 0x7FFFFFFF)
        return nullptr;
    uint32_t total = (uint32_t)size.QuadPart;
    uint8_t* buf = (uint8_t*)malloc(total);
    if (!buf)
        return nullptr;
    uint32_t done = 0;
    while (done < total)
    {
        DWORD read = 0;
        if (!ReadFile(h, buf + done, total - done, &read, nullptr) || read == 0)
        {
            free(buf);
            return nullptr;
        }
        done += read;
    }
    *outSize = total;
    return buf;
}

// Decrypt `enc` and stash it in a DELETE_ON_CLOSE temp file, returning a read handle to it. IL2CPP then
// CreateFileMapping/MapViewOfFile's that handle and gets plaintext. The temp is TEMPORARY|DELETE_ON_CLOSE so
// Windows keeps it in cache and removes it on close.
// ponytail: decrypts to a delete-on-close temp file; swap to an in-memory section (hook MapViewOfFile too)
// if transient on-disk plaintext ever matters - but a memory dumper beats either, so this matches the threat.
HANDLE ServeDecrypted(const uint8_t* enc, uint32_t encSize)
{
    if (encSize < sizeof(BitMonoFrontHeader))
        return INVALID_HANDLE_VALUE;
    const void* dec = BitMonoDecryptMetadata(enc);
    if (dec == enc)
        return INVALID_HANDLE_VALUE; // not encrypted / wrong key
    uint32_t len = ((const BitMonoFrontHeader*)enc)->bodyLength;

    wchar_t dir[MAX_PATH], path[MAX_PATH];
    HANDLE temp = INVALID_HANDLE_VALUE;
    if (GetTempPathW(MAX_PATH, dir) && GetTempFileNameW(dir, L"bmm", 0, path))
    {
        temp = g_origCreateFileW(path, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ, nullptr,
            CREATE_ALWAYS, FILE_ATTRIBUTE_TEMPORARY | FILE_FLAG_DELETE_ON_CLOSE, nullptr);
        if (temp != INVALID_HANDLE_VALUE)
        {
            uint32_t done = 0;
            bool ok = true;
            while (done < len)
            {
                DWORD wrote = 0;
                if (!WriteFile(temp, (const uint8_t*)dec + done, len - done, &wrote, nullptr) || wrote == 0)
                {
                    ok = false;
                    break;
                }
                done += wrote;
            }
            if (ok)
                SetFilePointer(temp, 0, nullptr, FILE_BEGIN);
            else
            {
                CloseHandle(temp);
                temp = INVALID_HANDLE_VALUE;
            }
        }
    }
    free((void*)dec);
    return temp;
}

HANDLE WINAPI HookedCreateFileW(LPCWSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode,
    LPSECURITY_ATTRIBUTES lpsa, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplate)
{
    HANDLE real = g_origCreateFileW(
        lpFileName, dwDesiredAccess, dwShareMode, lpsa, dwCreationDisposition, dwFlagsAndAttributes, hTemplate);
    if (real == INVALID_HANDLE_VALUE || !EndsWithMetadataName(lpFileName))
        return real; // not our file (or the open failed) - leave it exactly as it was

    uint32_t size = 0;
    uint8_t* enc = ReadAllFromHandle(real, &size);
    if (!enc || size < sizeof(BitMonoFrontHeader) ||
        ((const BitMonoFrontHeader*)enc)->sanity != kBitMonoSanity)
    {
        // Plain (unencrypted) metadata: rewind the real handle and hand it back untouched.
        free(enc);
        SetFilePointer(real, 0, nullptr, FILE_BEGIN);
        return real;
    }

    HANDLE served = ServeDecrypted(enc, size);
    free(enc);
    if (served == INVALID_HANDLE_VALUE)
    {
        SetFilePointer(real, 0, nullptr, FILE_BEGIN); // decrypt failed - fail open so the game still boots
        return real;
    }
    CloseHandle(real);
    return served;
}

// IAT-hook CreateFileW in `mod` by NAME (works whether it's imported from kernel32, kernelbase or an API set,
// unlike address matching). Hooks every matching import slot. Returns true if at least one was patched.
bool InstallCreateFileWHook(HMODULE mod)
{
    static const char kCreateFileW[] = "CreateFileW";
    if (!mod)
        return false;
    BYTE* base = (BYTE*)mod;
    IMAGE_DOS_HEADER* dos = (IMAGE_DOS_HEADER*)base;
    if (dos->e_magic != IMAGE_DOS_SIGNATURE)
        return false;
    IMAGE_NT_HEADERS* nt = (IMAGE_NT_HEADERS*)(base + dos->e_lfanew);
    if (nt->Signature != IMAGE_NT_SIGNATURE)
        return false;
    IMAGE_DATA_DIRECTORY dir = nt->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT];
    if (!dir.VirtualAddress)
        return false;

    bool hooked = false;
    for (IMAGE_IMPORT_DESCRIPTOR* desc = (IMAGE_IMPORT_DESCRIPTOR*)(base + dir.VirtualAddress);
         desc->Name; desc++)
    {
        DWORD intRva = desc->OriginalFirstThunk ? desc->OriginalFirstThunk : desc->FirstThunk;
        IMAGE_THUNK_DATA* names = (IMAGE_THUNK_DATA*)(base + intRva);
        IMAGE_THUNK_DATA* iat = (IMAGE_THUNK_DATA*)(base + desc->FirstThunk);
        for (; names->u1.AddressOfData; names++, iat++)
        {
            if (names->u1.Ordinal & IMAGE_ORDINAL_FLAG)
                continue; // imported by ordinal - no name to match
            IMAGE_IMPORT_BY_NAME* byName = (IMAGE_IMPORT_BY_NAME*)(base + names->u1.AddressOfData);
            if (strcmp((const char*)byName->Name, kCreateFileW) != 0)
                continue;
            if (!g_origCreateFileW)
                g_origCreateFileW = (CreateFileW_t)iat->u1.Function;
            DWORD old;
            if (VirtualProtect(&iat->u1.Function, sizeof(void*), PAGE_READWRITE, &old))
            {
                iat->u1.Function = (ULONGLONG)(uintptr_t)&HookedCreateFileW;
                VirtualProtect(&iat->u1.Function, sizeof(void*), old, &old);
                hooked = true;
            }
        }
    }
    // Fall back to the real export if we never captured an original (e.g. nothing imported it yet).
    if (hooked && !g_origCreateFileW)
        g_origCreateFileW = (CreateFileW_t)GetProcAddress(GetModuleHandleW(L"kernel32.dll"), kCreateFileW);
    return hooked && g_origCreateFileW != nullptr;
}

} // namespace
#endif // _WIN32 && !BITMONO_STANDALONE_TEST

// ---------------------------------------------------------------------------------------------------------
// Unity plugin auto-install: runs from a C++ static initializer at GameAssembly.dll load, before il2cpp_init.
// On by default (no define) so a loose Unity source plugin self-installs; the test builds opt out.
// ---------------------------------------------------------------------------------------------------------
#if defined(_WIN32) && !defined(BITMONO_STANDALONE_TEST) && !defined(BITMONO_HOOK_TEST)

// Exported so the linker treats this translation unit as reachable and never strips the initializer below.
// (If a future Unity/toolchain ever strips it anyway, also reference it from C# via [DllImport("__Internal")].)
extern "C" __declspec(dllexport) int BitMonoIl2cppKeepAlive() { return 0xB117C0DE; }

namespace {
struct BitMonoAutoInstall
{
    BitMonoAutoInstall()
    {
        HMODULE self = nullptr;
        // Resolve the module we live in (GameAssembly.dll) so we hook its imports, not the host exe's.
        GetModuleHandleExW(
            GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
            (LPCWSTR)&HookedCreateFileW, &self);
        InstallCreateFileWHook(self);
        (void)&BitMonoIl2cppKeepAlive;
    }
};
BitMonoAutoInstall g_bitMonoAutoInstall;
} // namespace
#endif // _WIN32 && !BITMONO_STANDALONE_TEST && !BITMONO_HOOK_TEST

// ---------------------------------------------------------------------------------------------------------
// Android: the POSIX analog of the Windows hook. IL2CPP open()s the (encrypted) global-metadata.dat and
// mmap()s it; we GOT-hook open() in our own libil2cpp.so so the read returns plaintext. 64-bit only
// (arm64 + x86_64 are both ELF64 and share this code); 32-bit ARMv7 would need ELF32/REL handling.
// ---------------------------------------------------------------------------------------------------------
#if defined(__ANDROID__) && defined(__LP64__) && !defined(BITMONO_STANDALONE_TEST)
#include <link.h>
#include <dlfcn.h>
#include <sys/mman.h>
#include <unistd.h>
#include <fcntl.h>
#include <stdarg.h>

namespace {

typedef int (*open_t)(const char*, int, ...);
open_t g_origOpen = nullptr;

bool EndsWithMetadataNameA(const char* path)
{
    static const char kName[] = BITMONO_METADATA_NAME;
    const size_t nameLen = sizeof(kName) - 1;
    if (!path)
        return false;
    size_t len = strlen(path);
    return len >= nameLen && strcmp(path + (len - nameLen), kName) == 0;
}

bool ReadAllFd(int fd, uint8_t* buf, size_t len)
{
    size_t done = 0;
    while (done < len)
    {
        ssize_t n = read(fd, buf + done, len - done);
        if (n <= 0)
            return false;
        done += (size_t)n;
    }
    return true;
}

// Decrypt and stash in a temp file in the metadata's own (app-writable) dir, unlinked immediately so it's
// gone on close. Returns an fd at offset 0; IL2CPP then mmaps it and sees plaintext.
int ServeDecryptedFd(const char* origPath, const uint8_t* enc)
{
    const void* dec = BitMonoDecryptMetadata(enc);
    if (dec == enc)
        return -1;
    uint32_t len = ((const BitMonoFrontHeader*)enc)->bodyLength;

    static const char kTempName[] = "bmmXXXXXX"; // mkstemp template, placed in the metadata's own writable dir
    char tmpl[4096];
    const char* slash = strrchr(origPath, '/');
    size_t dirLen = slash ? (size_t)(slash - origPath) + 1 : 0;
    if (dirLen + sizeof(kTempName) >= sizeof(tmpl))
    {
        free((void*)dec);
        return -1;
    }
    memcpy(tmpl, origPath, dirLen);
    memcpy(tmpl + dirLen, kTempName, sizeof(kTempName));

    int fd = mkstemp(tmpl);
    if (fd >= 0)
    {
        unlink(tmpl); // delete-on-close
        size_t done = 0;
        bool ok = true;
        while (done < len)
        {
            ssize_t w = write(fd, (const uint8_t*)dec + done, len - done);
            if (w <= 0) { ok = false; break; }
            done += (size_t)w;
        }
        if (ok)
            lseek(fd, 0, SEEK_SET);
        else { close(fd); fd = -1; }
    }
    free((void*)dec);
    return fd;
}

int HookedOpen(const char* path, int oflag, ...)
{
    mode_t mode = 0;
    if (oflag & O_CREAT) { va_list ap; va_start(ap, oflag); mode = (mode_t)va_arg(ap, int); va_end(ap); }
    int real = g_origOpen(path, oflag, mode);
    if (real < 0 || !EndsWithMetadataNameA(path))
        return real;

    off_t size = lseek(real, 0, SEEK_END);
    lseek(real, 0, SEEK_SET);
    if (size < (off_t)sizeof(BitMonoFrontHeader) || size > 0x7FFFFFFF)
        return real;
    uint8_t* enc = (uint8_t*)malloc((size_t)size);
    if (!enc)
        return real;
    if (ReadAllFd(real, enc, (size_t)size) && ((const BitMonoFrontHeader*)enc)->sanity == kBitMonoSanity)
    {
        int served = ServeDecryptedFd(path, enc);
        free(enc);
        if (served >= 0) { close(real); return served; }
        int re = g_origOpen(path, oflag, mode); // decrypt failed - hand back a fresh, rewound fd
        close(real);
        return re;
    }
    free(enc);
    lseek(real, 0, SEEK_SET);
    return real;
}

// Patch our own libil2cpp.so's GOT slot for open() (the ELF analog of the Windows IAT patch): walk the PLT
// relocations (DT_JMPREL) of the module we live in and swap the entry whose dynsym name is open/open64.
bool InstallOpenHook()
{
    Dl_info info;
    if (!dladdr((void*)&HookedOpen, &info) || !info.dli_fbase)
        return false;
    const uint8_t* base = (const uint8_t*)info.dli_fbase;
    const ElfW(Ehdr)* ehdr = (const ElfW(Ehdr)*)base;
    const ElfW(Phdr)* phdr = (const ElfW(Phdr)*)(base + ehdr->e_phoff);

    const ElfW(Dyn)* dyn = nullptr;
    for (int i = 0; i < ehdr->e_phnum; i++)
        if (phdr[i].p_type == PT_DYNAMIC) { dyn = (const ElfW(Dyn)*)(base + phdr[i].p_vaddr); break; }
    if (!dyn)
        return false;

    const ElfW(Sym)* symtab = nullptr;
    const char* strtab = nullptr;
    const ElfW(Rela)* jmprel = nullptr;
    size_t pltrelsz = 0;
    for (const ElfW(Dyn)* d = dyn; d->d_tag != DT_NULL; d++)
    {
        if (d->d_tag == DT_SYMTAB)   symtab = (const ElfW(Sym)*)(base + d->d_un.d_ptr);
        else if (d->d_tag == DT_STRTAB)   strtab = (const char*)(base + d->d_un.d_ptr);
        else if (d->d_tag == DT_JMPREL)   jmprel = (const ElfW(Rela)*)(base + d->d_un.d_ptr);
        else if (d->d_tag == DT_PLTRELSZ) pltrelsz = (size_t)d->d_un.d_val;
    }
    if (!symtab || !strtab || !jmprel || !pltrelsz)
        return false;

    size_t pageSize = (size_t)sysconf(_SC_PAGESIZE);
    size_t count = pltrelsz / sizeof(ElfW(Rela));
    for (size_t i = 0; i < count; i++)
    {
        const char* name = strtab + symtab[ELF64_R_SYM(jmprel[i].r_info)].st_name;
        if (strcmp(name, "open") != 0 && strcmp(name, "open64") != 0)
            continue;
        void** slot = (void**)(base + jmprel[i].r_offset);
        if (!g_origOpen)
            g_origOpen = (open_t)*slot;
        void* page = (void*)((uintptr_t)slot & ~(pageSize - 1));
        if (mprotect(page, pageSize, PROT_READ | PROT_WRITE) == 0)
        {
            *slot = (void*)&HookedOpen;
            mprotect(page, pageSize, PROT_READ); // restore (RELRO leaves the GOT read-only)
            return g_origOpen != nullptr;
        }
    }
    return false;
}

} // namespace
#endif // __ANDROID__ && __LP64__ && !BITMONO_STANDALONE_TEST

// Android auto-install: a .init_array constructor runs at dlopen(libil2cpp.so), before il2cpp_init.
#if defined(__ANDROID__) && defined(__LP64__) && !defined(BITMONO_STANDALONE_TEST) && !defined(BITMONO_HOOK_TEST)
extern "C" __attribute__((visibility("default"), used)) int BitMonoIl2cppKeepAlive() { return 0xB117C0DE; }
namespace {
__attribute__((constructor)) void BitMonoAndroidInstall()
{
    InstallOpenHook();
    (void)&BitMonoIl2cppKeepAlive;
}
} // namespace
#endif // __ANDROID__ && __LP64__ && !BITMONO_STANDALONE_TEST && !BITMONO_HOOK_TEST

// ---------------------------------------------------------------------------------------------------------
// Offline validator: decrypt an .enc and confirm it matches the original .dat byte-for-byte.
// ---------------------------------------------------------------------------------------------------------
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

// ---------------------------------------------------------------------------------------------------------
// Hook validator: install the IAT hook on ourselves, then open+map a file named global-metadata.dat exactly
// like IL2CPP does, and confirm the hook transparently served decrypted bytes. Proves the runtime delivery
// mechanism (CreateFileW -> CreateFileMapping -> MapViewOfFile) against a real .enc, without a Unity rebuild.
// ---------------------------------------------------------------------------------------------------------
#if defined(BITMONO_HOOK_TEST) && defined(_WIN32)
#include <cstdio>

int main(int argc, char** argv)
{
    if (argc < 2) { printf("usage: %s <encrypted.enc> [expected-plaintext.dat]\n", argv[0]); return 2; }

    // Stage the encrypted file under a temp dir as literally "global-metadata.dat" so the name check fires.
    wchar_t tempDir[MAX_PATH], staged[MAX_PATH];
    GetTempPathW(MAX_PATH, tempDir); // always ends with a backslash
    lstrcpyW(staged, tempDir);
    lstrcatW(staged, L"bitmono_hooktest_global-metadata.dat");
    {
        FILE* in = fopen(argv[1], "rb");
        if (!in) { printf("cannot open %s\n", argv[1]); return 2; }
        FILE* out = _wfopen(staged, L"wb");
        if (!out) { fclose(in); printf("cannot stage temp\n"); return 2; }
        uint8_t buf[65536]; size_t n;
        while ((n = fread(buf, 1, sizeof(buf), in)) > 0) fwrite(buf, 1, n, out);
        fclose(in); fclose(out);
    }

    if (!InstallCreateFileWHook(GetModuleHandleW(nullptr)))
    {
        printf("FAILED: could not install CreateFileW IAT hook\n");
        return 1;
    }

    // Mimic libil2cpp's MetadataLoader: CreateFileW -> CreateFileMapping -> MapViewOfFile (read-only).
    HANDLE file = CreateFileW(staged, GENERIC_READ, FILE_SHARE_READ, nullptr, OPEN_EXISTING,
        FILE_ATTRIBUTE_NORMAL, nullptr);
    if (file == INVALID_HANDLE_VALUE) { printf("FAILED: CreateFileW returned invalid handle\n"); return 1; }

    HANDLE mapping = CreateFileMappingW(file, nullptr, PAGE_READONLY, 0, 0, nullptr);
    if (!mapping) { printf("FAILED: CreateFileMapping\n"); CloseHandle(file); return 1; }
    const uint8_t* view = (const uint8_t*)MapViewOfFile(mapping, FILE_MAP_READ, 0, 0, 0);
    if (!view) { printf("FAILED: MapViewOfFile\n"); CloseHandle(mapping); CloseHandle(file); return 1; }

    uint32_t magic = *(const uint32_t*)view;
    int32_t version = *(const int32_t*)(view + 4);
    bool ok = magic == kIl2cppMetadataMagic;
    printf("mapped magic: 0x%08X (want 0xFAB11BAF), version: %d -> %s\n",
        magic, version, ok ? "HOOK SERVED DECRYPTED METADATA" : "FAILED (got ciphertext)");

    if (ok && argc >= 3)
    {
        FILE* ef = fopen(argv[2], "rb");
        if (ef)
        {
            fseek(ef, 0, SEEK_END); long expSize = ftell(ef); fseek(ef, 0, SEEK_SET);
            uint8_t* expected = (uint8_t*)malloc(expSize);
            if (expected && fread(expected, 1, (size_t)expSize, ef) == (size_t)expSize)
            {
                bool same = memcmp(view, expected, (size_t)expSize) == 0;
                printf("matches expected plaintext byte-for-byte: %s\n", same ? "YES" : "NO");
                ok = ok && same;
            }
            free(expected);
            fclose(ef);
        }
    }

    UnmapViewOfFile(view);
    CloseHandle(mapping);
    CloseHandle(file);
    return ok ? 0 : 1;
}
#endif

#endif // BITMONO_IL2CPP_ENABLED

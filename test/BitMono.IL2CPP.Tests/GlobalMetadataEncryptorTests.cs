namespace BitMono.IL2CPP.Tests;

public class GlobalMetadataEncryptorTests
{
    private static readonly byte[] Key = Enumerable.Range(100, 16).Select(i => (byte)i).ToArray();
    private static readonly string[] Names = { "", "Awake", "Player" };
    private static readonly string[] Literals = { "hello", "secret123" };

    [Fact]
    public void Encrypt_HidesTheMetadata_AndDecryptRestoresItExactly()
    {
        var metadata = SyntheticMetadata.Build(29, Names, Literals);

        var encrypted = GlobalMetadataEncryptor.Encrypt(metadata, Key);

        // A static dumper can no longer parse it (the il2cpp magic is gone).
        var readEncrypted = () => GlobalMetadataFile.Read(encrypted);
        readEncrypted.ShouldThrow<InvalidDataException>();

        // The decryptor restores the original bytes, and they parse again.
        var decrypted = GlobalMetadataEncryptor.Decrypt(encrypted, Key);
        decrypted.ShouldBe(metadata);
        GlobalMetadataFile.Read(decrypted).Strings.ShouldBe(new[] { "", "Awake", "Player" });
    }

    [Fact]
    public void Decrypt_WithWrongKey_FailsIntegrityCheck()
    {
        var encrypted = GlobalMetadataEncryptor.Encrypt(SyntheticMetadata.Build(29, Names, Literals), Key);
        var wrongKey = Enumerable.Range(0, 16).Select(i => (byte)i).ToArray();

        var decrypt = () => GlobalMetadataEncryptor.Decrypt(encrypted, wrongKey);

        decrypt.ShouldThrow<InvalidDataException>().Message.ShouldContain("integrity");
    }

    [Fact]
    public void Encrypt_RejectsNonMetadata()
    {
        var encrypt = () => GlobalMetadataEncryptor.Encrypt(new byte[64], Key);

        encrypt.ShouldThrow<InvalidDataException>().Message.ShouldContain("magic");
    }

    [Fact]
    public void Decrypt_RejectsUnencryptedInput()
    {
        var metadata = SyntheticMetadata.Build(29, Names, Literals);

        var decrypt = () => GlobalMetadataEncryptor.Decrypt(metadata, Key);

        decrypt.ShouldThrow<InvalidDataException>().Message.ShouldContain("sanity");
    }
}

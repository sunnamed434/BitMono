namespace BitMono.IL2CPP.Tests;

public class XxteaTests
{
    private static readonly byte[] Key = Enumerable.Range(0, 16).Select(i => (byte)i).ToArray();

    [Fact]
    public void Encrypt_MatchesIndependentReferenceVector()
    {
        // Known-answer cross-checked against an independent (Python) XXTEA implementation, so a symmetric
        // bug that still round-trips can't slip through. key = bytes 0..15, plaintext = "BitMono!".
        var cipher = Xxtea.Encrypt(Encoding.ASCII.GetBytes("BitMono!"), Key);

        BitConverter.ToString(cipher).Replace("-", "").ShouldBe("80E18B7F6BEB1EE1");
    }

    [Fact]
    public void Decrypt_IsInverseOfEncrypt()
    {
        var plain = Encoding.UTF8.GetBytes("the quick brown fox jumps over!!"); // 32 bytes = 8 words

        var roundTrip = Xxtea.Decrypt(Xxtea.Encrypt(plain, Key), Key);

        roundTrip.ShouldBe(plain);
        Xxtea.Encrypt(plain, Key).ShouldNotBe(plain); // it actually transformed the data
    }

    [Fact]
    public void Encrypt_RejectsWrongKeyLength()
    {
        var encrypt = () => Xxtea.Encrypt(new byte[8], new byte[15]);

        encrypt.ShouldThrow<ArgumentException>().Message.ShouldContain("16 bytes");
    }
}

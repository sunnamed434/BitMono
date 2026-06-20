namespace BitMono.IL2CPP.Tests;

public class Crc32Tests
{
    [Fact]
    public void Compute_MatchesStandardCheckValue()
    {
        // The canonical CRC-32/ISO-HDLC check value for the ASCII string "123456789".
        Crc32.Compute(Encoding.ASCII.GetBytes("123456789")).Should().Be(0xCBF43926);
    }

    [Fact]
    public void Compute_OfEmptyIsZero()
    {
        Crc32.Compute(Array.Empty<byte>()).Should().Be(0u);
    }
}

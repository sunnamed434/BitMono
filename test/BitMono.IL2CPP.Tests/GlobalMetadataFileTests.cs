namespace BitMono.IL2CPP.Tests;

public class GlobalMetadataFileTests
{
    [Fact]
    public void Read_ParsesVersionStringsAndLiterals()
    {
        var bytes = SyntheticMetadata.Build(
            version: 29,
            names: new[] { "", "Awake", "Player" },
            literals: new[] { "hello", "secret123" });

        var metadata = GlobalMetadataFile.Read(bytes);

        metadata.Version.Should().Be(29);
        metadata.Strings.Should().Equal("", "Awake", "Player");
        metadata.StringLiterals.Should().Equal("hello", "secret123");
    }

    [Fact]
    public void Read_RejectsBadMagic()
    {
        var bytes = SyntheticMetadata.Build(29, new[] { "A" }, new[] { "x" });
        bytes[0] = 0xDE; // corrupt the sanity magic

        var read = () => GlobalMetadataFile.Read(bytes);

        read.Should().Throw<InvalidDataException>().WithMessage("*magic*");
    }

    [Fact]
    public void Read_RejectsUnsupportedVersion()
    {
        var bytes = SyntheticMetadata.Build(9999, new[] { "A" }, new[] { "x" });

        var read = () => GlobalMetadataFile.Read(bytes);

        read.Should().Throw<InvalidDataException>().WithMessage("*version 9999*");
    }

    [Fact]
    public void Read_RejectsTruncatedFile()
    {
        var read = () => GlobalMetadataFile.Read(new byte[10]);

        read.Should().Throw<InvalidDataException>().WithMessage("*too small*");
    }

    [Fact]
    public void Read_RejectsRegionOutsideFile()
    {
        var bytes = SyntheticMetadata.Build(29, new[] { "A" }, new[] { "x" });
        SyntheticMetadata.WriteInt32(bytes, 24, int.MaxValue); // stringOffset points past the end of the file

        var read = () => GlobalMetadataFile.Read(bytes);

        read.Should().Throw<InvalidDataException>().WithMessage("*outside the file*");
    }
}

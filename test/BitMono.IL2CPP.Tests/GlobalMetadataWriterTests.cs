namespace BitMono.IL2CPP.Tests;

public class GlobalMetadataWriterTests
{
    private static readonly string[] Names = { "", "Awake", "Player" };
    private static readonly string[] Literals = { "hello", "secret123" };

    [Fact]
    public void ReplaceNames_NoEdits_ReturnsIdenticalCopy()
    {
        var bytes = SyntheticMetadata.Build(29, Names, Literals);

        var result = GlobalMetadataWriter.ReplaceNames(bytes, new Dictionary<int, string>());

        result.Should().Equal(bytes);
        result.Should().NotBeSameAs(bytes);
    }

    [Fact]
    public void ReplaceNames_SameLength_OverwritesNameAndKeepsTheRest()
    {
        var bytes = SyntheticMetadata.Build(29, Names, Literals);
        var playerIndex = SyntheticMetadata.IndexOfName(Names, "Player");

        var result = GlobalMetadataWriter.ReplaceNames(
            bytes, new Dictionary<int, string> { [playerIndex] = "Hidden" }); // both 6 bytes

        var reparsed = GlobalMetadataFile.Read(result);
        reparsed.Version.Should().Be(29);
        reparsed.Strings.Should().Equal("", "Awake", "Hidden");
        reparsed.StringLiterals.Should().Equal("hello", "secret123"); // literals untouched
    }

    [Fact]
    public void ReplaceNames_DifferentLength_Throws()
    {
        var bytes = SyntheticMetadata.Build(29, Names, Literals);
        var playerIndex = SyntheticMetadata.IndexOfName(Names, "Player");

        var replace = () => GlobalMetadataWriter.ReplaceNames(
            bytes, new Dictionary<int, string> { [playerIndex] = "X" });

        replace.Should().Throw<ArgumentException>().WithMessage("*length*");
    }

    [Fact]
    public void ReplaceNames_IndexOutsideRegion_Throws()
    {
        var bytes = SyntheticMetadata.Build(29, Names, Literals);

        var replace = () => GlobalMetadataWriter.ReplaceNames(
            bytes, new Dictionary<int, string> { [int.MaxValue] = "x" });

        replace.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ReplaceNames_NotMetadata_Throws()
    {
        var replace = () => GlobalMetadataWriter.ReplaceNames(
            new byte[64], new Dictionary<int, string>());

        replace.Should().Throw<InvalidDataException>().WithMessage("*magic*");
    }
}

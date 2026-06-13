using BitMono.Shared.Plugins;

namespace BitMono.Shared.Tests.Plugins;

public class PluginCompatibilityTests
{
    [Theory]
    // Plugin built against a newer minor than the host -> incompatible.
    [InlineData("0.40.0.0", "0.41.0.0", true)]
    [InlineData("0.40.5.0", "0.41.0.0", true)]
    [InlineData("1.0.0.0", "2.0.0.0", true)]
    // Equal or older contract -> compatible (best effort).
    [InlineData("0.40.0.0", "0.40.0.0", false)]
    [InlineData("0.40.0.0", "0.40.9.0", false)] // same Major.Minor, patch differences ignored
    [InlineData("0.41.0.0", "0.40.0.0", false)]
    [InlineData("2.0.0.0", "1.5.0.0", false)]
    // Locally-built host (0.0.x) can't be compared -> never flagged.
    [InlineData("0.0.0.0", "0.41.0.0", false)]
    // An unversioned/locally-built plugin (0.0.x) against a real host is never "newer".
    [InlineData("0.40.0.0", "0.0.0.0", false)]
    public void IsBuiltAgainstNewerContract_ComparesMajorMinor(string host, string plugin, bool expected)
    {
        var result = PluginCompatibility.IsBuiltAgainstNewerContract(new Version(host), new Version(plugin));

        result.Should().Be(expected);
    }

    [Fact]
    public void IsBuiltAgainstNewerContract_ReturnsFalse_WhenEitherVersionMissing()
    {
        PluginCompatibility.IsBuiltAgainstNewerContract(null, new Version(9, 9)).Should().BeFalse();
        PluginCompatibility.IsBuiltAgainstNewerContract(new Version(0, 40), null).Should().BeFalse();
    }
}

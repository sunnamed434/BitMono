using System.IO;
using System.Linq;
using BitMono.Shared.Plugins;

namespace BitMono.Shared.Tests.Plugins;

public class PluginProbingTests : IDisposable
{
    private readonly string _root;

    public PluginProbingTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "BitMonoPluginTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private string Touch(string relativePath)
    {
        var fullPath = Path.Combine(_root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, string.Empty);
        return fullPath;
    }

    [Fact]
    public void EnumeratePluginAssemblies_ReturnsEmpty_WhenDirectoryMissing()
    {
        var missing = Path.Combine(_root, "does-not-exist");

        PluginProbing.EnumeratePluginAssemblies(missing).Should().BeEmpty();
    }

    [Fact]
    public void EnumeratePluginAssemblies_ReturnsEmpty_WhenPathBlank()
    {
        PluginProbing.EnumeratePluginAssemblies("  ").Should().BeEmpty();
    }

    [Fact]
    public void EnumeratePluginAssemblies_FindsFlatDlls()
    {
        var first = Touch("First.dll");
        var second = Touch("Second.dll");

        var result = PluginProbing.EnumeratePluginAssemblies(_root);

        result.Should().BeEquivalentTo(new[] { first, second });
    }

    [Fact]
    public void EnumeratePluginAssemblies_FindsDllsAtRootAndOneLevelDeep()
    {
        var flat = Touch("Flat.dll");
        var nested = Touch(Path.Combine("MyPlugin", "MyPlugin.dll"));
        var shallowDependency = Touch(Path.Combine("MyPlugin", "SomeDependency.dll"));

        var result = PluginProbing.EnumeratePluginAssemblies(_root);

        result.Should().Contain(flat);
        result.Should().Contain(nested);
        result.Should().Contain(shallowDependency);
    }

    [Fact]
    public void EnumeratePluginAssemblies_IgnoresDeeplyNestedDependencies()
    {
        var nested = Touch(Path.Combine("MyPlugin", "MyPlugin.dll"));
        // Dependencies in a deeper folder are resolved lazily by the resolver, not loaded as plugins.
        var deepDependency = Touch(Path.Combine("MyPlugin", "libs", "SomeDependency.dll"));

        var result = PluginProbing.EnumeratePluginAssemblies(_root);

        result.Should().Contain(nested);
        result.Should().NotContain(deepDependency);
    }

    [Fact]
    public void EnumeratePluginAssemblies_IgnoresNonDllFiles()
    {
        Touch("readme.txt");
        Touch("plugin.json");
        var dll = Touch("Real.dll");

        var result = PluginProbing.EnumeratePluginAssemblies(_root);

        result.Should().ContainSingle().Which.Should().Be(dll);
    }

    [Fact]
    public void GetProbeDirectories_ReturnsEmpty_WhenDirectoryMissing()
    {
        PluginProbing.GetProbeDirectories(Path.Combine(_root, "nope")).Should().BeEmpty();
    }

    [Fact]
    public void GetProbeDirectories_IncludesRootAndAllNestedDirectories()
    {
        Touch(Path.Combine("MyPlugin", "MyPlugin.dll"));
        Touch(Path.Combine("MyPlugin", "libs", "SomeDependency.dll"));

        var directories = PluginProbing.GetProbeDirectories(_root);

        directories.Should().Contain(_root);
        directories.Should().Contain(Path.Combine(_root, "MyPlugin"));
        // Nested dependency folders are probe-able so the resolver can find deps placed there.
        directories.Should().Contain(Path.Combine(_root, "MyPlugin", "libs"));
    }
}

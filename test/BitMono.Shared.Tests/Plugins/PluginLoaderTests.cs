using System.IO;
using BitMono.Shared.Logging;
using BitMono.Shared.Plugins;

namespace BitMono.Shared.Tests.Plugins;

public class PluginLoaderTests
{
    [Fact]
    public void LoadPlugins_ReturnsEmpty_WhenDirectoryMissing()
    {
        var missing = Path.Combine(Path.GetTempPath(), "BitMonoPluginTests", Guid.NewGuid().ToString("N"));
        var loader = new PluginLoader(missing, new StubLogger());

        loader.LoadPlugins().ShouldBeEmpty();
    }

    [Fact]
    public void LoadPlugins_ReturnsEmpty_WhenDirectoryHasNoAssemblies()
    {
        var directory = Path.Combine(Path.GetTempPath(), "BitMonoPluginTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllText(Path.Combine(directory, "notes.txt"), "not a plugin");
            var loader = new PluginLoader(directory, new StubLogger());

            loader.LoadPlugins().ShouldBeEmpty();
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private sealed class StubLogger : ILogger
    {
        public void Debug(string messageTemplate, params object[] args) { }
        public void Information(string messageTemplate, params object[] args) { }
        public void Warning(string messageTemplate, params object[] args) { }
        public void Error(string messageTemplate, params object[] args) { }
        public void Error(Exception exception, string messageTemplate, params object[] args) { }
        public void Fatal(string messageTemplate, params object[] args) { }
        public void Fatal(Exception exception, string messageTemplate, params object[] args) { }
        public ILogger ForContext<T>() => this;
        public ILogger ForContext(Type type) => this;
    }
}

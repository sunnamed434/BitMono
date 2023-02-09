namespace BitMono.CLI.Modules;

public static class LoggerConfiguratorExtensions
{
    private const string OutputTemplate =
        "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}][{SourceContext}] {Message:lj}{NewLine}{Exception}";
    public static LoggerConfiguration AddConsoleLogger(this LoggerSinkConfiguration source)
    {
        return source.Async(configure => configure.Console(outputTemplate: OutputTemplate));
    }
}
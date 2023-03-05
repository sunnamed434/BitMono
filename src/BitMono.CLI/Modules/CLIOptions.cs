namespace BitMono.CLI.Modules;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class CLIOptions
{
    [Option('f', "file", Required = true, HelpText = "Set file path.")]
    public string? File { get; set; }

    [Option('l', "libraries", Required = false, HelpText = "Set libraries path.")]
    public string? Libraries { get; set; }

    [Option('o', "output", Required = false, HelpText = "Set output path.")]
    public string? Output { get; set; }
}
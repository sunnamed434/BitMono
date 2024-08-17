namespace BitMono.CLI.Modules;

internal class Options
{
    [Option('f', "file", Required = true, HelpText = "Set file path.")]
    public string? File { get; set; }

    [Option('l', "libraries", Required = false, HelpText = "Set libraries path.")]
    public string? Libraries { get; set; }

    [Option('o', "output", Required = false, HelpText = "Set output path.")]
    public string? Output { get; set; }
}
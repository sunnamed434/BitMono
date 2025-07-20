namespace BitMono.CLI.Modules;

internal class Options
{
    [Option('f', "file", Required = true, HelpText = "Set file path.")]
    public string? File { get; set; }

    [Option('l', "libraries", Required = false, HelpText = "Set libraries path.")]
    public string? Libraries { get; set; }

    [Option('o', "output", Required = false, HelpText = "Set output path.")]
    public string? Output { get; set; }

    [Option('p', "protections", Required = false, HelpText = "Set protections, also can be set via protections.json.")]
    public IEnumerable<string> Protections { get; set; } = [];
}
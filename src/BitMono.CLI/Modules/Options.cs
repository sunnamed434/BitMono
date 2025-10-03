namespace BitMono.CLI.Modules;

internal class Options
{
    [Option('f', "file", Required = true, HelpText = "Set file path.")]
    public string? File { get; set; }

    [Option('l', "libraries", Required = false, HelpText = "Set libraries path.")]
    public string? Libraries { get; set; }

    [Option('o', "output", Required = false, HelpText = "Set output path.")]
    public string? Output { get; set; }

    [Option('p', "protections", Required = false, HelpText = "Set protections list, also can be set via protections.json.")]
    public IEnumerable<string> Protections { get; set; } = [];

    [Option("protections-file", Required = false, HelpText = "Set protections configuration file path.")]
    public string? ProtectionsFile { get; set; }

    [Option("criticals-file", Required = false, HelpText = "Set criticals configuration file path.")]
    public string? CriticalsFile { get; set; }

    [Option("logging-file", Required = false, HelpText = "Set logging configuration file path.")]
    public string? LoggingFile { get; set; }

    [Option("obfuscation-file", Required = false, HelpText = "Set obfuscation configuration file path.")]
    public string? ObfuscationFile { get; set; }
}
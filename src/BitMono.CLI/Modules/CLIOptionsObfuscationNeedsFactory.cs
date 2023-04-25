namespace BitMono.CLI.Modules;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class CLIOptionsObfuscationNeedsFactory
{
    private readonly string[] _args;
    private readonly ILogger _logger;

    public CLIOptionsObfuscationNeedsFactory(string[] args, ILogger logger)
    {
        _args = args;
        _logger = logger.ForContext<CLIOptionsObfuscationNeedsFactory>();
    }

    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public ObfuscationNeeds? Create()
    {
        var parser = new Parser(with =>
        {
            with.EnableDashDash = true;
            with.HelpWriter = Console.Error;
        });
        var parserResult = parser.ParseArguments<CLIOptions>(_args);
        if (parserResult.Errors.IsEmpty() == false)
        {
            return null;
        }
        var options = parserResult.Value;
        if (File.Exists(options.File) == false)
        {
            _logger.Fatal("File cannot be found, please, try again!");
            return null;
        }
        var fileBaseDirectory = Path.GetDirectoryName(options.File);
        var needs = new ObfuscationNeeds();
        needs.FileName = options.File!;
        needs.FileBaseDirectory = fileBaseDirectory;
        needs.ReferencesDirectoryName = options.Libraries?.IsNullOrEmpty() == false
            ? options.Libraries
            : Path.Combine(fileBaseDirectory, "libs");
        needs.OutputPath = options.Output?.IsNullOrEmpty() == false
            ? options.Output
            : Path.Combine(fileBaseDirectory, "output");

        Directory.CreateDirectory(needs.OutputPath);
        Directory.CreateDirectory(needs.ReferencesDirectoryName);
        return needs;
    }
}
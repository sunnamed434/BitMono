namespace BitMono.CLI.Modules;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class CLIOptionsObfuscationNeedsFactory
{
    private readonly string[] _args;
    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly ILogger _logger;

    public CLIOptionsObfuscationNeedsFactory(string[] args, ObfuscationSettings obfuscationSettings, ILogger logger)
    {
        _args = args;
        _obfuscationSettings = obfuscationSettings;
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
        ObfuscationNeeds needs;
        if (_obfuscationSettings.ForceObfuscation)
        {
            needs = new ObfuscationNeeds
            {
                FileName = options.File!,
                FileBaseDirectory = fileBaseDirectory,
                ReferencesDirectoryName = fileBaseDirectory,
                OutputPath = fileBaseDirectory
            };
        }
        else
        {
            needs = new ObfuscationNeeds
            {
                FileName = options.File!,
                FileBaseDirectory = fileBaseDirectory,
                ReferencesDirectoryName = options.Libraries?.IsNullOrEmpty() == false
                    ? options.Libraries
                    : Path.Combine(fileBaseDirectory, _obfuscationSettings.ReferencesDirectoryName),
                OutputPath = options.Output?.IsNullOrEmpty() == false
                    ? options.Output
                    : Path.Combine(fileBaseDirectory, _obfuscationSettings.OutputDirectoryName)
            };
        }

        Directory.CreateDirectory(needs.OutputPath);
        Directory.CreateDirectory(needs.ReferencesDirectoryName);
        return needs;
    }
}
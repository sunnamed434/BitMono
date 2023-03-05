#pragma warning disable CS8604
namespace BitMono.CLI.Modules;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class CLIOptionsObfuscationNeedsFactory : IObfuscationNeedsFactory
{
    private readonly string[] m_Args;
    private readonly ILogger m_Logger;

    public CLIOptionsObfuscationNeedsFactory(string[] args, ILogger logger)
    {
        m_Args = args;
        m_Logger = logger.ForContext<CLIOptionsObfuscationNeedsFactory>();
    }

    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public ObfuscationNeeds? Create()
    {
        var parser = new Parser(with =>
        {
            with.EnableDashDash = true;
            with.HelpWriter = Console.Error;
        });
        var parserResult = parser.ParseArguments<CLIOptions>(m_Args);
        if (parserResult.Errors.IsEmpty() == false)
        {
            return null;
        }
        var options = parserResult.Value;
        if (File.Exists(options.File) == false)
        {
            m_Logger.Fatal("File cannot be found, please, try again!");
            return null;
        }
        var fileBaseDirectory = Path.GetDirectoryName(options.File);
        var needs = new ObfuscationNeeds
        {
            FileName = options.File,
            FileBaseDirectory = fileBaseDirectory,
            ReferencesDirectoryName = options.Libraries.IsNullOrEmpty() == false
                ? options.Libraries
                : Path.Combine(fileBaseDirectory, "libs"),
            OutputDirectoryName = options.Output.IsNullOrEmpty() == false
                ? options.Output
                : Path.Combine(fileBaseDirectory, "output")
        };

        Directory.CreateDirectory(needs.OutputDirectoryName);
        Directory.CreateDirectory(needs.ReferencesDirectoryName);
        return needs;
    }
}
#pragma warning disable CS8604
namespace BitMono.CLI.Modules;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class CLIOptionsObfuscationNeedsFactory : IObfuscationNeedsFactory
{
    private readonly string[] m_Args;

    public CLIOptionsObfuscationNeedsFactory(string[] args)
    {
        m_Args = args;
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
            Console.WriteLine("File cannot be found, please, try again!");
            return null;
        }
        var fileBaseDirectory = Path.GetDirectoryName(options.File);
        var needs = new ObfuscationNeeds
        {
            FileName = options.File,
            FileBaseDirectory = fileBaseDirectory,
            DependenciesDirectoryName = options.Libraries.IsNullOrEmpty() == false
                ? options.Libraries
                : Path.Combine(fileBaseDirectory, "libs"),
            OutputDirectoryName = options.Output.IsNullOrEmpty() == false
                ? options.Output
                : Path.Combine(fileBaseDirectory, "output")
        };

        Directory.CreateDirectory(needs.OutputDirectoryName);
        Directory.CreateDirectory(needs.DependenciesDirectoryName);
        return needs;
    }
}
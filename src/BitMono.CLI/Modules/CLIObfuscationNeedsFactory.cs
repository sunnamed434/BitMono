namespace BitMono.CLI.Modules;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class CLIObfuscationNeedsFactory
{
    private readonly string[] _args;
    private readonly ILogger _logger;

    public CLIObfuscationNeedsFactory(string[] args, ILogger logger)
    {
        _args = args;
        _logger = logger.ForContext<CLIObfuscationNeedsFactory>();
    }

    public ObfuscationNeeds Create()
    {
        var fileName = CLIBitMonoModuleFileResolver.Resolve(_args);
        var specifyingFile = true;
        while (specifyingFile)
        {
            try
            {
                _logger.Information("Please, specify file or drag-and-drop in BitMono CLI");
                fileName = PathFormatterUtility.Format(Console.ReadLine());
                if (string.IsNullOrWhiteSpace(fileName) == false)
                {
                    if (File.Exists(fileName))
                    {
                        specifyingFile = false;
                        _logger.Information("File successfully specified: {0}", fileName);
                    }
                    else
                    {
                        _logger.Warning("File cannot be found, please, try again!");
                    }
                }
                else
                {
                    _logger.Warning("Unable to specify empty null or whitespace file, please, try again!");
                }
            }
            catch (Exception ex)
            {
                _logger.Warning("Something went wrong while specifying the file: " + ex);
            }
        }

        var fileBaseDirectory = Path.GetDirectoryName(fileName);
        var dependenciesDirectoryName = Path.Combine(fileBaseDirectory, "libs");
        if (Directory.Exists(dependenciesDirectoryName) == false)
        {
            var specifyingDependencies = true;
            while (specifyingDependencies)
            {
                try
                {
                    if (Directory.Exists(dependenciesDirectoryName))
                    {
                        _logger.Information("Dependencies (libs) successfully found automatically: {0}!", dependenciesDirectoryName);
                        specifyingDependencies = false;
                        break;
                    }

                    _logger.Information("Please, specify dependencies (libs) path: ");
                    var newDependenciesDirectoryName = PathFormatterUtility.Format(Console.ReadLine());
                    if (string.IsNullOrWhiteSpace(newDependenciesDirectoryName) == false)
                    {
                        if (Directory.Exists(newDependenciesDirectoryName))
                        {
                            dependenciesDirectoryName = newDependenciesDirectoryName;
                            _logger.Information("Dependencies (libs) successfully specified: {0}!", newDependenciesDirectoryName);
                            specifyingDependencies = false;
                        }
                        else
                        {
                            _logger.Information("Libs directory doesn't exist, please, try again!");
                        }
                    }
                    else
                    {
                        _logger.Information("Unable to specify empty (libs), please, try again!");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Information("Something went wrong while specifying the dependencies (libs) path: " + ex);
                }
            }
        }
        else
        {
            _logger.Information("Dependencies (libs) directory was automatically found in: {0}!", dependenciesDirectoryName);
        }

        var outputDirectoryName = Path.Combine(fileBaseDirectory, "output");
        Directory.CreateDirectory(outputDirectoryName);
        Directory.CreateDirectory(dependenciesDirectoryName);
        return new ObfuscationNeeds
        {
            FileName = fileName,
            FileBaseDirectory = fileBaseDirectory,
            ReferencesDirectoryName = dependenciesDirectoryName,
            OutputPath = outputDirectoryName
        };
    }
}